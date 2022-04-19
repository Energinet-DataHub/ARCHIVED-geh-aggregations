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
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/function-app?ref=5.8.0"

  name                                      = "integration-event-listener"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  app_service_plan_id                       = data.azurerm_key_vault_secret.plan_shared_id.value
  application_insights_instrumentation_key  = data.azurerm_key_vault_secret.appi_instrumentation_key.value
  health_check_path                         = "/api/monitor/ready"
  health_check_alert_action_group_id        = data.azurerm_key_vault_secret.primary_action_group_id.value
  health_check_alert_enabled                = var.enable_health_check_alerts
  app_settings                              = {
    # Region: Default Values
    WEBSITE_ENABLE_SYNC_UPDATE_SITE                       = true
    WEBSITE_RUN_FROM_PACKAGE                              = 1
    WEBSITES_ENABLE_APP_SERVICE_STORAGE                   = true
    FUNCTIONS_WORKER_RUNTIME                              = "dotnet-isolated"
    INTEGRATION_EVENT_LISTENER_CONNECTION_STRING          = data.azurerm_key_vault_secret.sb_domain_relay_listener_connection_string.value
    INTEGRATION_EVENT_MANAGER_CONNECTION_STRING            = data.azurerm_key_vault_secret.sb_domain_relay_manage_connection_string.value
    EVENT_HUB_CONNECTION                                  = module.evh_aggregations.primary_connection_strings["send"]
    EVENT_HUB_NAME                                        = module.evh_aggregations.name
    METERING_POINT_CREATED_TOPIC_NAME         = data.azurerm_key_vault_secret.sbt_consumption_metering_point_created_name.value
    METERING_POINT_CREATED_SUBSCRIPTION_NAME  = data.azurerm_key_vault_secret.sbs_consumption_metering_point_created_to_aggregations_name.value
    METERING_POINT_CONNECTED_TOPIC_NAME                   = data.azurerm_key_vault_secret.sbt_metering_point_connected_name.value
    METERING_POINT_CONNECTED_SUBSCRIPTION_NAME            = data.azurerm_key_vault_secret.sbs_metering_point_connected_to_aggregations_name.value
    ENERGY_SUPPLIER_CHANGED_TOPIC_NAME                    = data.azurerm_key_vault_secret.sbt_energy_supplier_changed_name.value
    ENERGY_SUPPLIER_CHANGED_SUBSCRIPTION_NAME             = data.azurerm_key_vault_secret.sbs_energy_supplier_change_to_aggregations_name.value
    DATABASE_MASTERDATA_CONNECTIONSTRING                  = local.MS_DATABASE_MASTERDATA_CONNECTION_STRING
  }
  
  tags                                      = azurerm_resource_group.this.tags
}