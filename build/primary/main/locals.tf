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
  DATA_LAKE_DATA_CONTAINER_NAME                 = "data"
  MASTER_DATA_PATH_METERING_POINTS              = "metering-points"
  MASTER_DATA_PATH_MARKET_ROLES                 = "market-roles"
  MASTER_DATA_PATH_CHARGES                      = "charges"
  MASTER_DATA_PATH_CHARGE_LINKS                 = "charge-links"
  MASTER_DATA_PATH_CHARGE_PRICES                = "charge-prices"
  MASTER_DATA_PATH_ES_BRP_RELATIONS             = "es-brp-relations"
  MASTER_DATA_PATH_GRID_LOSS_SYSTEM_CORRECTION  = "grid-loss-system-correction"
  DATABASE_CONNECTION_STRING                    = "Server=tcp:${data.azurerm_key_vault_secret.sql_data_url.value},1433;Initial Catalog=${module.sqldb_aggregations.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.sql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.sql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  MS_DATABASE_CONNECTION_STRING                 = "Server=tcp:${data.azurerm_key_vault_secret.mssql_data_url.value},1433;Initial Catalog=${module.mssqldb_aggregations.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.mssql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.mssql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
}
