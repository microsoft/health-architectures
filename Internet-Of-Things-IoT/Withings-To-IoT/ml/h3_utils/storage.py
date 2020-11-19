# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

from dataclasses import dataclass

from pyspark.sql.session import SparkSession


@dataclass
class StorageAccount:
    account: str
    container: str
    key: str

    def authenticate(self, spark: SparkSession) -> None:
        # TODO: replace they account key with a SAS token
        spark.conf.set(
            f"fs.azure.account.key.{self.account}.dfs.core.windows.net",
            self.key)

    @property
    def root(self) -> str:
        return f"abfss://{self.container}@{self.account}.dfs.core.windows.net"
