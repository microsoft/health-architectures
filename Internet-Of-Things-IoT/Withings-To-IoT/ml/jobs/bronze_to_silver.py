# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

from datetime import datetime
from enum import Enum, unique
from typing import Callable, Optional

from delta.tables import DeltaTable
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col, from_json, udf
from pyspark.sql.session import SparkSession
from pyspark.sql.utils import AnalysisException

from h3_utils.runtime import DatabricksRuntime
from h3_utils.storage import StorageAccount
from h3_utils.tables import table_exists


@unique
class RunStatus(Enum):
    NoActionRequired = 1
    TableCreated = 2
    TableUpdated = 3


def load_eventhub_json(
    spark: SparkSession,
    path: str,
    col_name: str = "json",
) -> Optional[DataFrame]:

    try:
        raw_df = spark.read.format("avro").load(path)
    except AnalysisException:
        return None

    tmp_col = "jsonText"
    bytes_to_string = udf(lambda payload: payload.decode("utf-8"))

    json_text_df = raw_df.withColumn(tmp_col, bytes_to_string(col("Body")))
    json_text_rdd = json_text_df.rdd.map(lambda row: row.jsonText)
    json_schema = spark.read.json(json_text_rdd).schema

    return (
        json_text_df
        .withColumn(col_name, from_json(col(tmp_col), json_schema))
        .drop(tmp_col)
    )


def main(
    spark: SparkSession,
    date: str,
    in_storage: StorageAccount,
    out_storage: StorageAccount,
    eventhub_name: str,
    capture_name: str,
    delete_files: Callable[[str], None],
) -> RunStatus:

    spark.conf.set("spark.sql.avro.compression.codec", "deflate")
    spark.conf.set("spark.sql.avro.deflate.level", "5")

    in_storage.authenticate(spark)
    out_storage.authenticate(spark)

    in_prefix = f"{in_storage.root}/{eventhub_name}/{capture_name}/{date}"
    in_suffix = "/*" * (7 - len(date.split("/")))
    out_table = "observations"
    out_location = f"{out_storage.root}/{out_table}"

    eventhub_df = load_eventhub_json(spark, f"{in_prefix}{in_suffix}.avro")

    if eventhub_df is None:
        return RunStatus.NoActionRequired

    observations_df = eventhub_df \
        .select("json") \
        .filter(col("json.resource.resourceType") == "Observation") \
        .repartition("json.resource.subject.reference")

    if table_exists(spark, out_table):
        DeltaTable \
            .forPath(spark, out_location) \
            .alias("old") \
            .merge(
                observations_df.alias("new"),
                "old.json.resource.id = new.json.resource.id") \
            .whenMatchedDelete("new.json.operation = 'Delete'") \
            .whenNotMatchedInsertAll("new.json.operation = 'Create'") \
            .execute()
        status = RunStatus.TableUpdated
    else:
        observations_df \
            .filter(col("json.operation") == "Create") \
            .write \
            .format("delta") \
            .save(out_location)
        spark.sql(f"""
            CREATE TABLE {out_table}
            USING DELTA
            LOCATION '{out_location}'
        """)
        status = RunStatus.TableCreated

    delete_files(f"{in_prefix}/")

    return status


if __name__ == "__main__":
    runtime = DatabricksRuntime(spark, dbutils)  # noqa: F821

    date = runtime.get_arg("date", datetime.utcnow().strftime(r"%Y/%m/%d/%H"))
    input_storage = runtime.get_arg("inputStorageAccount")
    output_storage = runtime.get_arg("outputStorageAccount")
    input_container = runtime.get_arg("inputContainer")
    output_container = runtime.get_arg("outputContainer")
    eventhub_name = runtime.get_arg("eventHubName")
    capture_name = runtime.get_arg("captureName")

    status = main(
        spark=runtime.session,
        date=date,
        in_storage=StorageAccount(
            account=input_storage,
            container=input_container,
            key=runtime.get_secret(
                scope="storage",
                key=f"{input_storage}AccessKey"
            ),
        ),
        out_storage=StorageAccount(
            account=output_storage,
            container=output_container,
            key=runtime.get_secret(
                scope="storage",
                key=f"{output_storage}AccessKey"
            ),
        ),
        eventhub_name=eventhub_name,
        capture_name=capture_name,
        delete_files=runtime.delete_files,
    )

    print(status)
