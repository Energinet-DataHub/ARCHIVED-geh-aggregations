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
locals {
  COORDINATOR_FUNCTION_NAME                     = "coordinator"
  DATA_LAKE_MASTER_DATA_BLOB_NAME               = "master-data"
  DATA_LAKE_EVENTS_BLOB_NAME                    = "events"
  DATA_LAKE_RESULTS_BLOB_NAME                   = "results"
  DATA_LAKE_SNAPSHOTS_BLOB_NAME                 = "snapshots"
  DATA_LAKE_DATA_CONTAINER_NAME                 = "aggregation-data"
  # Defined in the geh-timeseries domain
  DATA_LAKE_TIME_SERIES_CONTAINER_NAME          = "timeseries-data"
  TIME_SERIES_POINTS_DELTA_TABLE_NAME           = "time-series-points"
  MS_DATABASE_CONNECTION_STRING                 = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_aggregations.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  MS_DATABASE_MASTERDATA_CONNECTION_STRING      = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_aggregations_masterdata.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
