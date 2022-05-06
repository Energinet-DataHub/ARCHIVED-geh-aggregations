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
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=6.0.0"

  name                                      = local.COORDINATOR_FUNCTION_NAME
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  vnet_integration_subnet_id                = data.azurerm_key_vault_secret.snet_vnet_integrations_id.value
  private_endpoint_subnet_id                = data.azurerm_key_vault_secret.snet_private_endpoints_id.value
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  always_on                                 = true
  health_check_path                         = "/api/monitor/ready"
  health_check_alert_action_group_id        = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled                = var.enable_health_check_alerts
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                     = true
    WEBSITE_RUN_FROM_PACKAGE                            = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                 = true
    FUNCTIONS_WORKER_RUNTIME                            = "dotnet-isolated"
    CONNECTION_STRING_DATABRICKS                        = "https://${data.azurerm_key_vault_secret.dbw_databricks_workspace_url.value}"
    TOKEN_DATABRICKS                                    = "!!!!!If this is missing run databricks cluster job"
    DATA_STORAGE_CONTAINER_NAME                         = local.DATA_LAKE_DATA_CONTAINER_NAME
    DATA_STORAGE_ACCOUNT_NAME                           = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
    DATA_STORAGE_ACCOUNT_KEY                            = data.azurerm_key_vault_secret.st_shared_data_lake_primary_access_key.value
    SHARED_STORAGE_AGGREGATIONS_CONTAINER_NAME          = local.DATA_LAKE_DATA_CONTAINER_NAME
    SHARED_STORAGE_TIME_SERIES_CONTAINER_NAME           = local.DATA_LAKE_TIME_SERIES_CONTAINER_NAME
    SHARED_STORAGE_ACCOUNT_NAME                         = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
    SHARED_STORAGE_ACCOUNT_KEY                          = data.azurerm_key_vault_secret.st_shared_data_lake_primary_access_key.value
    TIME_SERIES_POINTS_DELTA_TABLE_NAME                 = local.TIME_SERIES_POINTS_DELTA_TABLE_NAME
    SNAPSHOTS_BASE_PATH                                 = local.DATA_LAKE_SNAPSHOTS_BLOB_NAME
    RESULT_URL                                          = "https://func-${local.COORDINATOR_FUNCTION_NAME}-${var.domain_name_short}-${var.environment_short}-${var.environment_instance}.azurewebsites.net/api/ResultReceiver"
    SNAPSHOT_NOTIFY_URL                                 = "https://func-${local.COORDINATOR_FUNCTION_NAME}-${var.domain_name_short}-${var.environment_short}-${var.environment_instance}.azurewebsites.net/api/SnapshotReceiver"
    AGGREGATION_PYTHON_FILE                             = "dbfs:/aggregation/aggregation_trigger.py"
    WHOLESALE_PYTHON_FILE                               = "dbfs:/aggregation/wholesale_trigger.py"
    DATA_PREPARATION_PYTHON_FILE                        = "dbfs:/aggregation/preparation_trigger.py"
    CLUSTER_TIMEOUT_MINUTES                             = 10
    DATABASE_CONNECTIONSTRING                           = local.MS_DATABASE_CONNECTION_STRING
    MASTER_DATA_DATABASE_CONNECTION_STRING              = local.MS_DATABASE_MASTERDATA_CONNECTION_STRING
    B2C_TENANT_ID                                       = data.azurerm_key_vault_secret.b2c_tenant_id.value
    BACKEND_SERVICE_APP_ID                              = data.azurerm_key_vault_secret.backend_service_app_id.value
  }
  
  tags                                      = azurerm_resource_group.this.tags
}
