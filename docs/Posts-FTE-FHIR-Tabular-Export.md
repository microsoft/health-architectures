---
title: FHIR Tabular Export (FHIR to parquet)
author: Cory
parent: Posts
nav_order: 1
---

# FHIR Tabular Export - FTE
## Transforming FHIR to parquet

## Introduction

In the continued conversation on 'What to do with your data after FHIR?' we are introducing some new code samples. In my last post, Exporting Data to HIVE, HDFS or Azure Data Bricks (insert link), we talked about to get data out of the Azure API for FHIR and in a Data Lake. In this post we will cover how do we convert the export into a data lake file format standard like parquet. When data is exported out of the Azure API for FHIR into a data lake, Microsoft follows the FHIR specification for [Bulk data](http://build.fhir.org/ig/HL7/bulk-data/export.html). The FHIR Bulk data specification calls for the data to be exported via [NDJson format](http://ndjson.org/) by resource type. This means upon export the data is organized by resource type (Patient, Encounter, Organization, etc.) into NDJson files.

Many organizations we work with standardize with one of two approaches, data lake designs with parquet or relational data warehouses with SQL. With the FTE we have considered the needs of both. The FTE converts the NDJson into parquet with the option to output CREATE TABLE statements for each resource type.

Conversions can be messy. This conversion between FHIR to parquet is not any different.  FHIR is JSON which supports nesting. Parquet is a columnar store. While Parquet does support nesting the feature is not used consistently. Therefore, how to de we get from NDJson to Parquet in an efficient manner. Efficient meaning timely, cost effective, and differential transformations.

Let's break down each of those goals for efficiency before continuing.

- **Timely** - The transformation from NDJson to Parquet needs to complete in a micro-batching manner. This micro-batching allows the data processing to run throughout the day. Our goal for the FTE has been to perform the transformation in the same amount of time or less than the export from the API for FHIR to the Azure Data Lake took to perform.
- **Cost Effective** - The code needs to scale up and down as needed to perform the task only using necessary resources.
- **Differential Appends** - Because FHIR exports can use the _since parameter, the FTE needs to update the parquet files store with the new information.

Before we move on, **why parquet?** Parquet provides several benefits for those in or supporting the Advanced Analytics community. The benefit we focused on was the flexibility for downstream data usage. Parquet provides great performance inside the Azure Data Bricks ecosystem as well as the Azure Synapse ecosystem. Parquet is one of recommended formats by the Azure Synapse team when pulling data from a data lake into the Synapse engine via COPY or Polybase. Therefore, by using the parquet file format we maximize downstream usages for Azure DataBricks or Azure Synapse workloads.

## Solution

Now that we understand the problem and the goals, what is a solution? A solution is a piece of python code optimized for FHIR NDJson files. In this case we used Azure DataBricks to build the sample code.

As you will see in the code, we perform a few steps to create the parquet setup in Azure Data Lake Gen2. The top pieces of the code are standard setup steps.

- Import libaries
- Setup DataBricks widgets/parameters
- Mount the import and export storage

```python
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
```

Next, we get into the main pieces of the transformation code. The first function called ‘explode_arrays’ takes a spark dataframe input, loops through the arrays and creates new rows in the output dataframe. Each new field in the array will create a new row. Keep this in mind when writing SQL queries against the data later. A SQL query will have some columns with repeated values. The output is not following any normalization.

```python
def explode_arrays(dfflat):
  '''Takes a spark dataframe input and explodes the array columns to multiple rows, returns a dataframe'''
  flat_cols = [field.name for field in dfflat.schema.fields if type(field.dataType) != ArrayType]
  cols = [field.name for field in dfflat.schema.fields if type(field.dataType) == ArrayType]
  exploded_df = dfflat.withColumn('vals', explode_outer(arrays_zip(*cols))) \
           .select(*flat_cols,'vals.*') \
           .fillna('', subset=cols)
  return exploded_df
```

The next function called ‘flatten_structs’ flattens the structs in the spark dataframe. This is done to create the columns for the output dataframe. At this point the data types are string. We will make suggestions for converting these to other data types later.

```python
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
```

Finally, we bring both functions together in the ‘flatten_df’ function. This function loops through the spark dataframe creating the columns and rows for each FHIR resource export file.

```python
def flatten_df(dfflat):
  '''Takes a spark data frame input and flattens struct columns into multiple columns and explodes array columns into multiple rows, returns a dataframe'''
  while len([field.name for field in dfflat.schema.fields if type(field.dataType) == StructType or type(field.dataType) == ArrayType ]) !=0 :
    dfflat = flatten_structs(dfflat)
    dfflat = explode_arrays(dfflat)
  
  return dfflat
```

There is one more function called ‘generate_ddl’. This   function creates ACSI SQL DDLs for use in Azure DataBricks or Azure Synapse table creation. Running the function is optional, and the parameter flag for DDL creation is set in the ‘Output’ combo box. The ‘generate_ddl’ function is a good starting point for writing the DDLs you may need downstream.

The DDLs are starter SQL CREATE TABLE statement. The goal of the CREATE TABLE statements is to provide end users with the names of the columns created by the FTE. We do not want to assume data types therefore all data types are nvarchar(50). Adjusting the data types in the DDL to the appropriate data types is encouraged.

The DDL code is stored in a separate folder called ‘ddl’ to prevent confusion with the parquet output.

```python
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
```

The final set of code pulls everything together. This code manages the workflow based on input parameters. Telling the code where to find the original NDJson FHIR files, sending the files into the flattener, where to store the transformed parquet output and unmounting the storage locations.

```python
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
```

That's it. Very simple but powerful piece of code.

Where can I get the FTE? Right, [**here**](https://github.com/microsoft/health-architectures/tree/master/Research-and-Analytics/FTE-FHIR-Tabular-Export).
What about automation and performance? Azure Data Factory is very useful for automation and orchestration of processes like FHIR bulk export and running the FTE process. We are working on a sample Azure Data Factory template which will automate the FHIR bulk export, arrange the input files to optimize transformation performance, and clean up the FHIR NDJson staging files.

Stay tuned next week for the upcoming Azure Data Factory pipeline.

For more Azure API for FHIR architectures check out [https://aka.ms/HealthArchitectures](https://aka.ms/HealthArchitectures).

For questions/ comments email HealthArchitectures@Microsoft.com

Special thanks to Katie Claveau @kamoclav and Todd Morris @ToddM2 for editing and code optimizations.
