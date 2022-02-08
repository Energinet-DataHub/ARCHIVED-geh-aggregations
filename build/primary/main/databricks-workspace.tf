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
module "dbw_aggregations" {
  source                                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/databricks-workspace?ref=6.0.0-add-databricks-workspace-module"

  name                                      = "dbws"
  project_name                              = var.domain_name_short
  environment_short                         = var.environment_short
  environment_instance                      = var.environment_instance
  resource_group_name                       = azurerm_resource_group.this.name
  location                                  = azurerm_resource_group.this.location
  sku                                       = "standard"
  main_virtual_network_id                   = data.azurerm_key_vault_secret.vnet_id.value
  main_virtual_network_name                 = data.azurerm_key_vault_secret.vnet_name.value
  main_virtual_network_resource_group_name  = data.azurerm_key_vault_secret.vnet_resource_group_name.value
  databricks_virtual_network_address_space  = "10.142.92.0/23"
  private_subnet_address_prefix             = "10.142.92.0/24"
  public_subnet_address_prefix              = "10.142.93.0/24"

  tags                                      = azurerm_resource_group.this.tags
}

module "kvs_databricks_workspace_id" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "dbw-databricks-workspace-id"
  value         = module.dbw_aggregations.workspace_id
  key_vault_id  = module.kv_aggregations.id

  tags          = azurerm_resource_group.this.tags
}