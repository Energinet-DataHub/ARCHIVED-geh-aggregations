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
    azfun-integration-event-listener_name = "azfun-integration-event-listener-${var.project}-${var.organisation}-${var.environment}"
}
module "azfun-integration-event-listener" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//function-app?ref=1.9.0"
  name                                      = local.azfun-integration-event-listener_name
  resource_group_name                       = data.azurerm_resource_group.main.name
  location                                  = data.azurerm_resource_group.main.location
  storage_account_access_key                = module.azfun-integration-event-listener_stor.primary_access_key
  storage_account_name                      = module.azfun-integration-event-listener_stor.name
  app_service_plan_id                       = module.azfun-integration-event-listener_plan.id
  application_insights_instrumentation_key  = module.appi.instrumentation_key
  tags                                      = data.azurerm_resource_group.main.tags
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                       = true
    WEBSITE_RUN_FROM_PACKAGE                              = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                   = true
    FUNCTIONS_WORKER_RUNTIME                              = "dotnet-isolated"
    INTEGRATION_EVENT_LISTENER_CONNECTION_STRING          = data.azurerm_key_vault_secret.INTEGRATION_EVENTS_LISTENER_CONNECTION_STRING.value
    EVENT_HUB_CONNECTION                                  = module.evhar_aggregation_sender.primary_connection_string
    EVENT_HUB_NAME                                        = module.evh_aggregation.name
    APPINSIGHTS_INSTRUMENTATIONKEY                        = "8a4daec5-4775-4bdd-9ef0-8fb8724da99f",
    INTEGRATION_EVENT_LISTENER_CONNECTION_STRING          = "Endpoint=sb://sbn-integrationevents-sharedres-endk-u.servicebus.windows.net/;SharedAccessKeyName=sbnar-integrationevents-listener;SharedAccessKey=y8q3rp3HQWyCBxb+ovsRPRJR9Vw5uYs/BptGy6w/oZc=",
    CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME         = "consumption-metering-point-created",
    CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME  = "consumption-metering-point-created-to-aggregations",
    METERING_POINT_CONNECTED_TOPIC_NAME                   = "metering-point-connected",
    METERING_POINT_CONNECTED_SUBSCRIPTION_NAME            = "metering-point-connected-to-aggregations",
    ENERGY_SUPPLIER_CHANGED_TOPIC_NAME                    = "energy-supplier-changed",
    ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME             = "energy-supplier-change-to-aggregations",
    EVENT_HUB_CONNECTION                                  = "Endpoint=sb://evhnm-aggregation-aggregations-endk-u.servicebus.windows.net/;SharedAccessKeyName=evhar-aggregation-listener;SharedAccessKey=65Pfzom3sMCgStfORF+PlVzbMWxFasZaqXR+uWJCc/Q=",
    EVENT_HUB_NAME                                        = "evh-aggregation"
  }
  
  dependencies                              = [
    module.appi.dependent_on,
    module.azfun-integration-event-listener_plan.dependent_on,
    module.azfun-integration-event-listener_stor.dependent_on,
  ]
}

module "azfun-integration-event-listener_plan" {
  source              = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//app-service-plan?ref=1.9.0"
  name                = "asp-integration-event-listener-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  kind                = "FunctionApp"
  sku                 = {
    tier  = "Free"
    size  = "F1"
  }
  tags                = data.azurerm_resource_group.main.tags
}

module "azfun-integration-event-listener_stor" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.9.0"
  name                      = "stor${random_string.integration-event-listener.result}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  account_replication_type  = "LRS"
  access_tier               = "Cool"
  account_tier              = "Standard"
  tags                      = data.azurerm_resource_group.main.tags
}

# Since all functions need a storage connected we just generate a random name
resource "random_string" "integration-event-listener" {
  length  = 10
  special = false
  upper   = false
}