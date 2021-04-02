# Databricks notebook source
import pandas as pd
from pyspark.sql.types import ArrayType, StructType
from pyspark.sql.functions import explode_outer, col, arrays_zip
import os
from pyspark.sql.functions import pandas_udf, explode


# COMMAND ----------

#Options for mounting the blob storage account to Azure DataBricks
dbutils.widgets.combobox("Premium", "Y", ["Y", "N"])
dbutils.widgets.combobox("Output", "Parquet", ["Parquet", "DDL", "Parquet+DDL"])
dbutils.widgets.combobox("OutputStorageAccount", "<Blob Storage Account>", ["<Blob Storage Account>"])
dbutils.widgets.combobox("InputContainerName", "<Container>", ["<Container>"])
dbutils.widgets.combobox("OutputContainerName", "<Container>", ["<Container>"])
dbutils.widgets.combobox("InputMountPoint", "<MountPoint>", ["<MountPoint>"])
dbutils.widgets.combobox("SasToken", "N/A", ["N/A", "<SAS token>"])
dbutils.widgets.combobox("Index", "N/A", ["N/A","<Index>"])
dbutils.widgets.combobox("SasKey", "N/A", ["N/A","<SasKey>"])
dbutils.widgets.combobox("OutputMountPoint",  "<MountPoint>", [ "<MountPoint>"])
dbutils.widgets.combobox("InputStorageAccount",  "<Blob Storage Account>", [ "<Blob Storage Account>"])

# COMMAND ----------

def mount_storage(container, storage, mountpoint):
  '''Take a container name and mount point input and mounts the mountpoint to the storageaccount container'''
  configs = {
    "fs.azure.account.auth.type": "CustomAccessToken",
    "fs.azure.account.custom.token.provider.class":   spark.conf.get("spark.databricks.passthrough.adls.gen2.tokenProviderClassName")
  }

  #Only mount storage if it not already mounted
  if not any(mount.mountPoint == mountpoint for mount in dbutils.fs.mounts()):

    if dbutils.widgets.get("Premium") == 'Y':
    #Mounting storage when account is Premium  
      dbutils.fs.mount(
        source = "abfss://"+ container + "@" + storage + ".dfs.core.windows.net/",
        mount_point = mountpoint,
        extra_configs = configs)

    else:
    #Mounting storage when account is not Premium
      dbutils.fs.mount(
        source = "wasbs://%s@%s.blob.core.windows.net/" % (container, storage),
        mount_point = mountpoint,
        extra_configs = {"fs.azure.sas.%s.%s.blob.core.windows.net" % (container, storage) : "%s" % dbutils.widgets.get("SasKey")})


# COMMAND ----------

#Mounting the output blob storage account to Azure DataBricks
mount_storage(dbutils.widgets.get("InputContainerName"), dbutils.widgets.get("InputStorageAccount"), dbutils.widgets.get("InputMountPoint"))
mount_storage(dbutils.widgets.get("OutputContainerName"), dbutils.widgets.get("OutputStorageAccount"), dbutils.widgets.get("OutputMountPoint"))


# COMMAND ----------

def explode_arrays(dfflat):
  '''Takes a spark dataframe input and explodes the array columns to multiple rows, returns a dataframe'''
  flat_cols = [field.name for field in dfflat.schema.fields if type(field.dataType) != ArrayType]
  cols = [field.name for field in dfflat.schema.fields if type(field.dataType) == ArrayType]
  exploded_df = dfflat.withColumn('vals', explode_outer(arrays_zip(*cols))) \
           .select(*flat_cols,'vals.*') \
           .fillna('', subset=cols)
  return exploded_df

def flatten_structs(nested_df):
  '''Takes a spark dataframe input and flattens the struct columns into multiple columns prefaced with the parent name, returns a dataframe'''
  stack = [((), nested_df)]
  columns = []
  while len(stack) > 0:
      parents, df = stack.pop()
      flat_cols = [col(".".join(parents + (c[0],))).alias("_".join(parents + (c[0],))) for c in df.dtypes if c[1][:6] != "struct"]
      nested_cols = [c[0] for c in df.dtypes if c[1][:6] == "struct" ]
      columns.extend(flat_cols)
      for nested_col in nested_cols:
          projected_df = df.select(nested_col + ".*")
          stack.append((parents + (nested_col,), projected_df))
  return nested_df.select(columns)

# Flatten the struct columns and explode the array columns to fully flatten the dataframe
def flatten_df(dfflat):
  '''Takes a spark data frame input and flattens struct columns into multiple columns and explodes array columns into multiple rows, returns a dataframe'''
  while len([field.name for field in dfflat.schema.fields if type(field.dataType) == StructType or type(field.dataType) == ArrayType ]) !=0 :
    dfflat = flatten_structs(dfflat)
    dfflat = explode_arrays(dfflat)
  
  return dfflat

# Generate DDL scripts based on flatten dataframe columns
def generate_ddl(df, tblname):
  '''Takes a flatten json spark data frame input, generates a create table DDL with 50 varchar datatype columns, and writes the DDLs to a DBFS folder'''
  createtbl = 'CREATE TABLE [' + tblname +  '] ( \n'
  num_columns = len(dfflat.columns)
  for i, y in enumerate(dfflat.columns):
    if i == num_columns-1:
      column_name = '\t['+ y + '] nvarchar(50)); \n\n'
    else:
      column_name = '\t[' + y + '] nvarchar(50), \n'
    createtbl += column_name
  dbutils.fs.put(dbutils.widgets.get("OutputMountPoint") + '/ddl/'+ tblname+'.sql', contents = createtbl, overwrite = True)


# COMMAND ----------

#Loop through the FHIR source and generate parquet, ddl, or both outputs based on output parameter
#Assumes files are in folders by FHIR resource type. Ex. CareTeam
try:
  dir = dbutils.fs.ls(dbutils.widgets.get("InputMountPoint"))
  dirdf = pd.DataFrame(dir,columns=['path','name','size'])
  
  for x in dirdf.path:
    df = spark.read.json(x)
    dfflat = flatten_df(df)
    tblname = x[len(dbutils.widgets.get("InputMountPoint"))+6:(len(x)-1)]
    dfoutpath = dbutils.widgets.get("OutputMountPoint") + '/' + tblname + '.parquet'
    print(dfoutpath)

    if dbutils.widgets.get("Output") == 'Parquet':
      #write parquet files out
      dfflat.write.mode('append').parquet(dfoutpath)   

    elif dbutils.widgets.get("Output") == 'DDL':
      #Create Tables scripts based on flatten dataframe columns
      generate_ddl(dfflat, tblname)

    elif dbutils.widgets.get("Output") == 'Parquet+DDL':
      dfflat.write.mode('append').parquet(dfoutpath)
      generate_ddl(dfflat, tblname)
except:  
  # Unmount if fails
  dbutils.fs.unmount(dbutils.widgets.get("InputMountPoint"))
  dbutils.fs.unmount(dbutils.widgets.get("OutputMountPoint"))
  raise

# COMMAND ----------

  # Unmount storage account
  dbutils.fs.unmount(dbutils.widgets.get("InputMountPoint"))
  dbutils.fs.unmount(dbutils.widgets.get("OutputMountPoint"))