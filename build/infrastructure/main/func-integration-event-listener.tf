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
module "func_integration_event_listener" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=5.1.0"

  name                                      = "integration-event-listener"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = module.plan_shared.id
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                       = true
    WEBSITE_RUN_FROM_PACKAGE                              = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                   = true
    FUNCTIONS_WORKER_RUNTIME                              = "dotnet-isolated"
    INTEGRATION_EVENT_LISTENER_CONNECTION_STRING          = data.azurerm_key_vault_secret.sb_domain_relay_listener_connection_string.value
    EVENT_HUB_CONNECTION                                  = module.evhar_aggregation_sender.primary_connection_string
    EVENT_HUB_NAME                                        = module.evh_aggregation.name
    CONSUMPTION_METERING_POINT_CREATED_TOPIC_NAME         = data.azurerm_key_vault_secret.sbt_consumption_metering_point_created_name.value
    CONSUMPTION_METERING_POINT_CREATED_SUBSCRIPTION_NAME  = data.azurerm_key_vault_secret.sbs_consumption_metering_point_created_to_aggregations_name.value
    METERING_POINT_CONNECTED_TOPIC_NAME                   = data.azurerm_key_vault_secret.sbt_metering_point_connected_name.value
    METERING_POINT_CONNECTED_SUBSCRIPTION_NAME            = data.azurerm_key_vault_secret.sbs_metering_point_connected_to_aggregations_name.value
    ENERGY_SUPPLIER_CHANGED_TOPIC_NAME                    = data.azurerm_key_vault_secret.sbt_energy_supplier_changed_name.value
    ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME             = data.azurerm_key_vault_secret.sbs_energy_supplier_change_to_aggregations_name.value
  }
  
  tags                                      = azurerm_resource_group.this.tags
}