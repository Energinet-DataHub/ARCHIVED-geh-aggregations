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
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub-namespace?ref=5.1.0"

  name                      = "aggregation"
  project_name              = var.domain_name_short
  environment_short         = var.environment_short
  environment_instance      = var.environment_instance
  resource_group_name       = azurerm_resource_group.this.name
  location                  = azurerm_resource_group.this.location
  sku                       = "Standard"
  capacity                  = 1

  tags                      = azurerm_resource_group.this.tags
}

module "evh_aggregation" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/eventhub?ref=5.1.0"

  name                      = "aggregation"
  namespace_name            = module.evhnm_aggregation.name
  resource_group_name       = azurerm_resource_group.this.name
  partition_count           = 4
  message_retention         = 1
  auth_rules            = [
    {
      name    = "listen",
      listen  = true
    },
    {
      name    = "send",
      send    = true
    },
  ]
}

module "kvs_evh_aggregation_listen_key" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "evh-aggregation-listen-connection-string"
  value         = module.evh_aggregation.primary_connection_strings["listen"]
  key_vault_id  = module.kv_aggregation.id

  tags          = azurerm_resource_group.this.tags
}