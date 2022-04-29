# Copyright 2020 Energinet DataHub A/S
#
# Licensed under the Apache License, Version 2.0 (the "License2");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

import os
import pytest
import shutil
import subprocess
from pyspark.sql.types import StructType, StringType, StructField

from geh_stream.codelists import Colname
from geh_stream.schemas import time_series_points_schema


time_series_points_delta_table_name = "time-series-points"


@pytest.fixture
def time_series_points_delta_table(spark, delta_lake_path):
    # Remove Delta table created by previous test executions
    time_series_points_path = f"{delta_lake_path}/timeseries-data/{time_series_points_delta_table_name}"
    shutil.rmtree(time_series_points_path, ignore_errors=True)

    # Create empty Delta table but with correct schema
    time_series_points = []
    rdd = (spark
           .sparkContext
           .parallelize(time_series_points))
    (spark
     .createDataFrame(rdd, schema=time_series_points_schema)
     .write
     .format("delta")
     .save(time_series_points_path))


def test_preparation_trigger_returns_0(
    databricks_path,
    delta_lake_path,
    master_data_database,
    master_data_db_info,
    snapshot_http_server_url,
    time_series_points_delta_table
):
    # Arrange: Remove delta table created by prepare job in previous test runs
    snapshots_path = f"{delta_lake_path}/aggregations/SNAPSHOTS_BASE_PATH/SNAPSHOT_ID"
    shutil.rmtree(snapshots_path, ignore_errors=True)

    # Act
    exit_code = subprocess.call([
        "python",
        f"{databricks_path}/aggregation-jobs/preparation_trigger.py",
        "--job-id", "JOB_ID",
        "--beginning-date-time", "2020-01-01T00:00:00+0000",
        "--end-date-time", "2020-01-01T00:00:00+0000",
        "--snapshot-id", "SNAPSHOT_ID",
        "--snapshot-notify-url", snapshot_http_server_url,
        "--snapshots-base-path", "SNAPSHOTS_BASE_PATH",
        "--time-series-points-delta-table-name", time_series_points_delta_table_name,
        "--shared-storage-account-name", "SHARED_STORAGE_ACCOUNT_NAME",
        "--shared-storage-account-key", "SHARED_STORAGE_ACCOUNT_KEY",
        "--shared-storage-time-series-base-path", f"{delta_lake_path}/timeseries-data",
        "--shared-storage-aggregations-base-path", f"{delta_lake_path}/aggregations",
        "--shared-database-url", master_data_db_info["server_name"],
        "--shared-database-aggregations", master_data_db_info["database_name"],
        "--shared-database-username", master_data_db_info["sa_user_id"],
        "--shared-database-password", master_data_db_info["sa_user_pass"]
        ])

    # Assert
    assert exit_code == 0, "Preparation job did not return exit code 0"


def test_preparation_requires_expected_parameters(databricks_path):
    """
    Prepare job must expect exactly the parameters specified in the reference text file.
    Exception is the "--only-validate-params" parameter used when validating parameters.

    This test works in tandem with a .NET test verifying that the trigger in the coordinator
    function matches the exact same expected parameters by also validating against the
    same reference file. Basically this is a workaround instead of creating an integration
    test spanning the two services together.
    """
    # Arrange
    with open(f"{databricks_path}/tests/integration/jobs/prepare-job-required-parameters.txt", "r") as file:
        params = file.readlines()

    expected_params = []
    for p in params:
        expected_params = expected_params + [p.strip(), "x"]

    # Act
    exit_code = subprocess.call([
        "python",
        f"{databricks_path}/aggregation-jobs/preparation_trigger.py",
        ] + expected_params + ["--only-validate-params"])

    # Assert
    assert exit_code == 0, "Preparation job did not require the expected parameters"
