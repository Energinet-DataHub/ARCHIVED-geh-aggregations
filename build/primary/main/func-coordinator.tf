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
module "func_coordinator" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=5.1.0"

  name                                      = local.COORDINATOR_FUNCTION_NAME
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_shared.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                     = true
    WEBSITE_RUN_FROM_PACKAGE                            = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                 = true
    FUNCTIONS_WORKER_RUNTIME                            = "dotnet-isolated"
    CONNECTION_STRING_DATABRICKS                        = "https://${azurerm_databricks_workspace.dbw_aggregations.workspace_url}"
    TOKEN_DATABRICKS                                    = "!!!!!If this is missing run databricks cluster job"
    DATA_STORAGE_CONTAINER_NAME                         = local.DATA_LAKE_DATA_CONTAINER_NAME
    DATA_STORAGE_ACCOUNT_NAME                           = module.st_data_lake.name
    DATA_STORAGE_ACCOUNT_KEY                            = module.st_data_lake.primary_access_key
    SHARED_STORAGE_CONTAINER_NAME                       = data.azurerm_key_vault_secret.st_shared_data_lake_data_container_name.value
    SHARED_STORAGE_ACCOUNT_NAME                         = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
    SHARED_STORAGE_ACCOUNT_KEY                          = data.azurerm_key_vault_secret.st_shared_data_lake_primary_access_key.value
    TIME_SERIES_PATH                                    = data.azurerm_key_vault_secret.st_shared_data_lake_timeseries_blob_name.value
    GRID_LOSS_SYSTEM_CORRECTION_PATH                    = local.MASTER_DATA_PATH_GRID_LOSS_SYSTEM_CORRECTION
    METERING_POINTS_PATH                                = local.MASTER_DATA_PATH_METERING_POINTS
    MARKET_ROLES_PATH                                   = local.MASTER_DATA_PATH_MARKET_ROLES
    CHARGES_PATH                                        = local.MASTER_DATA_PATH_CHARGES
    CHARGE_LINKS_PATH                                   = local.MASTER_DATA_PATH_CHARGE_LINKS
    CHARGE_PRICES_PATH                                  = local.MASTER_DATA_PATH_CHARGE_PRICES
    ES_BRP_RELATIONS_PATH                               = local.MASTER_DATA_PATH_ES_BRP_RELATIONS
    SNAPSHOT_PATH                                       = local.DATA_LAKE_SNAPSHOTS_BLOB_NAME
    RESULT_URL                                          = "https://${local.COORDINATOR_FUNCTION_NAME}.azurewebsites.net/api/ResultReceiver"
    SNAPSHOT_URL                                        = "https://${local.COORDINATOR_FUNCTION_NAME}.azurewebsites.net/api/SnapshotReceiver"
    AGGREGATION_PYTHON_FILE                             = "dbfs:/aggregation/aggregation_trigger.py"
    WHOLESALE_PYTHON_FILE                               = "dbfs:/aggregation/wholesale_trigger.py"
    DATA_PREPARATION_PYTHON_FILE                        = "dbfs:/aggregation/preparation_trigger.py"
    CLUSTER_TIMEOUT_MINUTES                             = 10
    DATABASE_CONNECTIONSTRING                           = "Server=tcp:${data.azurerm_key_vault_secret.sql_data_url.value},1433;Initial Catalog=${module.sqldb_aggregations.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.sql_data_admin_name.value};Password=${data.azurerm_key_vault_secret.sql_data_admin_password.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  }
  
  tags                                      = azurerm_resource_group.this.tags
}