# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

from pyspark.sql.session import SparkSession
from pyspark.sql.utils import AnalysisException


def table_exists(spark: SparkSession, name: str) -> bool:
    try:
        spark.sql(f"SHOW TBLPROPERTIES {name}")
    except AnalysisException:
        return False
    else:
        return True
