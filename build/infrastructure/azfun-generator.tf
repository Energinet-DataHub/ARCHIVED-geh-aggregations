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
    azfun_generator_name = "azfun-generator-${var.project}-${var.organisation}-${var.environment}"
}
module "azfun_generator" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.2.0"
  name                                      = local.azfun_generator_name
  resource_group_name                       = data.azurerm_resource_group.main.name
  location                                  = data.azurerm_resource_group.main.location
  storage_account_access_key                = module.azfun_generator_stor.primary_access_key
  storage_account_name                      = module.azfun_generator_stor.name
  app_service_plan_id                       = module.azfun_generator_plan.id
  application_insights_instrumentation_key  = module.appi.instrumentation_key
  tags                                      = data.azurerm_resource_group.main.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE         = true
    WEBSITE_RUN_FROM_PACKAGE                = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE     = true
    FUNCTIONS_WORKER_RUNTIME                = "dotnet"
    TEST_DATA_SOURCE_CONNECTION_STRING      = module.stor_generator.primary_connection_string
    TEST_DATA_SOURCE_CONTAINER_NAME         = module.container_generator.name
    MASTERDATA_DB_CONNECTION_STRING         = local.masterdata_db_connection_string
    METERINGPOINTS_DB_NAME                  = azurerm_cosmosdb_sql_container.collection_meteringpoints.name
    MARKETROLES_DB_NAME                     = azurerm_cosmosdb_sql_container.collection_marketroles.name
    CHARGES_DB_NAME                         = azurerm_cosmosdb_sql_container.collection_charges.name
    CHARGELINKS_DB_NAME                     = azurerm_cosmosdb_sql_container.collection_chargelinks.name
  }
  
  dependencies                              = [
    module.appi.dependent_on,
    module.azfun_generator_plan.dependent_on,
    module.azfun_generator_stor.dependent_on,
  ]
}

module "azfun_generator_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.2.0"
  name                = "asp-generator-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Free"
    size  = "F1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun_generator_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.2.0"
  name                      = "stor${random_string.generator.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "generator" {
  length  = 10
  special = false
  upper   = false
}