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

module "evhnm_aggregation" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub-namespace?ref=1.0.0"
  name                      = "evhnm-aggregation-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name       = data.azurerm_resource_group.main.name
  location                  = data.azurerm_resource_group.main.location
  sku                       = "Standard"
  capacity                  = 1
  tags                      = data.azurerm_resource_group.main.tags
}

module "evh_aggregation" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub?ref=1.0.0"
  name                      = "evh-aggregation"
  namespace_name            = module.evhnm_aggregation.name
  resource_group_name       = data.azurerm_resource_group.main.name
  partition_count           = 4
  message_retention         = 1
  dependencies              = [
    module.evhnm_aggregation
  ]
}

module "evhar_aggregation_sender" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub-auth-rule?ref=1.0.0"
  name                      = "evhar-aggregation-sender"
  namespace_name            = module.evhnm_aggregation.name
  eventhub_name             = module.evh_aggregation.name
  resource_group_name       = data.azurerm_resource_group.main.name
  send                      = true
  dependencies              = [
    module.evh_aggregation
  ]
}

module "evhar_aggregation_listener" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//event-hub-auth-rule?ref=1.0.0"
  name                      = "evhar-aggregation-listener"
  namespace_name            = module.evhnm_aggregation.name
  eventhub_name             = module.evh_aggregation.name
  resource_group_name       = data.azurerm_resource_group.main.name
  listen                    = true
  dependencies              = [
    module.evh_aggregation
  ]
}

module "kvs_aggregation_evh_listening_key" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=2.0.0"
  name                            = "aggregation-evh-listening-key"
  value                           = module.evhar_aggregation_listener.primary_connection_string
  key_vault_id                    = module.kv_aggregation.id
  tags          = data.azurerm_resource_group.main.tags
  dependencies = [module.evhar_aggregation_listener]
}
