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
    INPUT_PATH                                          =  var.input_path
    RESULT_URL                                          = "https://${local.azfun_coordinator_name}.azurewebsites.net/api/ResultReceiver"
    PYTHON_FILE                                         = "dbfs:/aggregation/aggregation_trigger.py"
    CLUSTER_TIMEOUT_MINUTES                             = "10"
    GRID_LOSS_SYS_COR_PATH                              = var.grid_loss_sys_cor_path
    
  }
  dependencies                              = [
    module.appi.dependent_on,
    module.azfun_coordinator_plan.dependent_on,
    module.azfun_coordinator_stor.dependent_on,
  ]
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