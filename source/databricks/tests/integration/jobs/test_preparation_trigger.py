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
import subprocess


def test_preparation_trigger_returns_0(databricks_path, master_data_database, master_data_db_info):
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
        "--time-series-points-delta-table-name", "TIME_SERIES_POINTS_DELTA_TABLE_NAME",
        "--shared-storage-account-name", "SHARED_STORAGE_ACCOUNT_NAME",
        "--shared-storage-account-key", "SHARED_STORAGE_ACCOUNT_KEY",
        "--shared-storage-aggregations-container-name", "SHARED_STORAGE_AGGREGATIONS_CONTAINER_NAME",
        "--shared-storage-time-series-container-name", "SHARED_STORAGE_TIME_SERIES_CONTAINER_NAME",
        "--shared-database-url", master_data_db_info["server_name"],
        "--shared-database-aggregations", master_data_db_info["database_name"],
        "--shared-database-username", master_data_db_info["sa_user_id"],
        "--shared-database-password", master_data_db_info["sa_user_password"],
        "--grid-loss-system-correction-path", "GRID_LOSS_SYSTEM_CORRECTION_PATH"
        ], shell=False)
    assert exit_code == 0, "Preparation job did not return exit code 0"
