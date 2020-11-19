# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

from enum import Enum, unique

from pyspark.sql.session import SparkSession

from h3_utils.runtime import DatabricksRuntime
from h3_utils.storage import StorageAccount
from h3_utils.tables import table_exists


@unique
class RunStatus(Enum):
    NoActionRequired = 1
    RecordsDeleted = 2


def main(
    spark: SparkSession,
    fhir_user_id: str,
    storage: StorageAccount,
) -> RunStatus:

    storage.authenticate(spark)

    if not table_exists(spark, "observations"):
        return RunStatus.NoActionRequired

    spark.sql(f"""
        DELETE FROM observations
        WHERE json.resource.subject.reference = '{fhir_user_id}'
    """)

    return RunStatus.RecordsDeleted


if __name__ == "__main__":
    runtime = DatabricksRuntime(spark, dbutils)  # noqa: F821

    fhir_user_id = runtime.get_arg("fhirUserId")
    storage_account = runtime.get_arg("silverStorageAccount")
    storage_container = runtime.get_arg("silverStorageContainer")

    status = main(
        spark=runtime.session,
        fhir_user_id=fhir_user_id,
        storage=StorageAccount(
            account=storage_account,
            container=storage_container,
            key=runtime.get_secret(
                scope="storage",
                key=f"{storage_account}AccessKey"
            ),
        ),
    )

    print(status)
