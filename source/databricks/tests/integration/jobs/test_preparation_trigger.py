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
import shutil
import subprocess
from pyspark.sql.types import StructType, StringType, StructField

from geh_stream.codelists import Colname
from geh_stream.schemas import time_series_points_schema


def test_preparation_trigger_returns_0(databricks_path, delta_lake_path, master_data_database, master_data_db_info, spark):
    # Arrange
    time_series_points_path = f"{delta_lake_path}/timeseries-data/time-series-points"
    if(os.path.exists(time_series_points_path)):
        shutil.rmtree(time_series_points_path)

    # Create Delta table dependencies
    # columns = ["foo"]  # Colname.timeseries, Colname.year, Colname.month, Colname.day, Colname.system_receival_time]
    time_series_points = []  # (unprocessed_time_series_json_string, 2022, 3, 21, "2022-12-17T09:30:47Z")]
    rdd = (spark
           .sparkContext
           .parallelize(time_series_points))
    (spark
     .createDataFrame(rdd, schema=time_series_points_schema)
     # .withColumn(Colname.system_receival_time, to_timestamp(Colname.system_receival_time))
     .write
     .format("delta")
     .save(time_series_points_path))

    # Act
    exit_code = subprocess.call([
        "python",
        f"{databricks_path}/aggregation-jobs/preparation_trigger.py",
        "--job-id", "JOB_ID",
        # "--grid-area", "GRID_AREA",
        "--beginning-date-time", "2020-01-01T00:00:00+0000",
        "--end-date-time", "2020-01-01T00:00:00+0000",
        "--snapshot-id", "SNAPSHOT_ID",
        "--snapshot-notify-url", "SNAPSHOT_NOTIFY_URL",
        "--snapshots-base-path", "SNAPSHOTS_BASE_PATH",
        "--time-series-points-delta-table-name", "time-series-points",
        "--shared-storage-account-name", "SHARED_STORAGE_ACCOUNT_NAME",
        "--shared-storage-account-key", "SHARED_STORAGE_ACCOUNT_KEY",
        "--shared-storage-time-series-base-path", f"{delta_lake_path}/timeseries-data",
        "--shared-storage-aggregations-base-path", f"{delta_lake_path}/aggregations",
        "--shared-database-url", master_data_db_info["server_name"],
        "--shared-database-aggregations", master_data_db_info["database_name"],
        "--shared-database-username", master_data_db_info["sa_user_id"],
        "--shared-database-password", master_data_db_info["sa_user_password"],
        "--grid-loss-system-correction-path", "GRID_LOSS_SYSTEM_CORRECTION_PATH"
        ])

    # Assert
    assert exit_code == 0, "Preparation job did not return exit code 0"
