# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

from pyspark.sql.session import SparkSession


class DatabricksRuntime:
    def __init__(self, spark, dbutils) -> None:
        self.__spark = spark
        self.__dbutils = dbutils

    @property
    def session(self) -> SparkSession:
        return self.__spark

    def get_arg(self, name: str, default: str = "") -> str:
        self.__dbutils.widgets.text(name, default)

        value = self.__dbutils.widgets.get(name)

        if not value:
            raise ValueError(f"Missing argument '{name}'")

        return value

    def get_secret(self, scope: str, key: str) -> str:
        return self.__dbutils.secrets.get(scope, key)

    def delete_files(self, path: str) -> None:
        self.__dbutils.fs.rm(path, recurse=True)
