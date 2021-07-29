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
    azfun_coordinator_name = "azfun-coordinator-${var.project}-${var.organisation}-${var.environment}"
}
module "azfun_coordinator" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.2.0"
  name                                      = local.azfun_coordinator_name
  resource_group_name                       = data.azurerm_resource_group.main.name
  location                                  = data.azurerm_resource_group.main.location
  storage_account_access_key                = module.azfun_coordinator_stor.primary_access_key
  storage_account_name                      = module.azfun_coordinator_stor.name
  app_service_plan_id                       = module.azfun_coordinator_plan.id
  application_insights_instrumentation_key  = module.appi.instrumentation_key
  tags                                      = data.azurerm_resource_group.main.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                     = true
    WEBSITE_RUN_FROM_PACKAGE                            = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                 = true
    FUNCTIONS_WORKER_RUNTIME                            = "dotnet"
    CONNECTION_STRING_SERVICEBUS                        = data.azurerm_key_vault_secret.POST_OFFICE_QUEUE_CONNECTION_STRING.value
    CONNECTION_STRING_DATABRICKS                        = "https://${azurerm_databricks_workspace.databricksworkspace.workspace_url}"
    TOKEN_DATABRICKS                                    = "!!!!!If this is missing run databricks cluster job"
    INPUTSTORAGE_CONTAINER_NAME                         =  var.inputstorage_container_name
    INPUTSTORAGE_ACCOUNT_NAME                           =  var.inputstorage_account_name
    INPUTSTORAGE_ACCOUNT_KEY                            =  var.inputstorage_account_key
    PERSIST_LOCATION                                    =  var.persist_location
    INPUT_PATH                                          =  var.input_path
    RESULT_URL                                          = "https://${local.azfun_coordinator_name}.azurewebsites.net/api/ResultReceiver"
    SNAPSHOT_URL                                        = "https://${local.azfun_coordinator_name}.azurewebsites.net/api/SnapshotReceiver"
    AGGREGATION_PYTHON_FILE                             = "dbfs:/aggregation/aggregation_trigger.py"
    WHOLESALE_PYTHON_FILE                               = "dbfs:/aggregation/wholesale_trigger.py"
    CLUSTER_TIMEOUT_MINUTES                             = "10"
    GRID_LOSS_SYS_COR_PATH                              = var.grid_loss_sys_cor_path
    DATABASE_CONNECTIONSTRING                           = "Server=tcp:${data.azurerm_key_vault_secret.SHARED_RESOURCES_DB_URL.value},1433;Initial Catalog=${azurerm_mssql_database.sqldb_metadata.name};Persist Security Info=False;User ID=${data.azurerm_key_vault_secret.SHARED_RESOURCES_DB_ADMIN_NAME.value};Password=${data.azurerm_key_vault_secret.SHARED_RESOURCES_DB_ADMIN_PASSWORD.value};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    DATAHUB_GLN                                         = "45V000-ENERGINET"
    ESETT_GLN                                           = "45V000-ENERGINET"
    HOST_KEY                                            = data.azurerm_function_app_host_keys.host_keys.default_function_key
    COSMOS_DATABASE                                     = var.cosmos_database
    COSMOS_ACCOUNT_ENDPOINT                             = var.cosmos_account_endpoint
    COSMOS_ACCOUNT_KEY                                  = var.cosmos_account_key
    COSMOS_CONTAINER_METERING_POINTS                    = var.cosmos_container_metering_points
    COSMOS_CONTAINER_MARKET_ROLES                       = var.cosmos_container_market_roles
    COSMOS_CONTAINER_CHARGES                            = var.cosmos_container_charges
    COSMOS_CONTAINER_CHARGE_LINKS                       = var.cosmos_container_charge_links
    COSMOS_CONTAINER_CHARGE_PRICES                      = var.cosmos_container_charge_prices
    COSMOS_CONTAINER_GRID_LOSS_SYS_CORR                 = var.cosmos_container_grid_loss_sys_corr
    COSMOS_CONTAINER_ES_BRP_RELATIONS                   = var.cosmos_container_es_brp_relations
    RESOLUTION                                          = var.resolution
  }
  
  dependencies                              = [
    module.appi.dependent_on,
    module.azfun_coordinator_plan.dependent_on,
    module.azfun_coordinator_stor.dependent_on,
  ]
}

data "azurerm_function_app_host_keys" "host_keys" {
  name                = local.azfun_coordinator_name
  resource_group_name = data.azurerm_resource_group.main.name
}

module "azfun_coordinator_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.2.0"
  name                = "asp-coordinator-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Free"
    size  = "F1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun_coordinator_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.2.0"
  name                      = "stor${random_string.coordinator.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "coordinator" {
  length  = 10
  special = false
  upper   = false
}