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
module "vnet_databricks" {
  source                = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/vnet?ref=6.0.0"
  name                  = "databricks"
  project_name          = var.domain_name_short
  environment_short     = var.environment_short
  environment_instance  = var.environment_instance
  resource_group_name   = azurerm_resource_group.this.name
  location              = azurerm_resource_group.this.location
  address_space         = ["10.42.0.0/23"]
  peerings              = [
    {
      name                                        = "main"
      remote_virtual_network_id                   = var.main_virtual_network_id
      remote_virtual_network_name                 = var.main_virtual_network_name
      remote_virtual_network_resource_group_name  = var.main_resource_group_name
      remote_virtual_network_subscription_id      = var.subscription_id
    }
  ]
}

# Link the Private Zone with the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "blob" {
  name                  = "pdnsz-blob-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = var.private_dns_resource_group_name
  private_dns_zone_name = var.private_dns_zone_blob_name
  virtual_network_id    = module.vnet_main.id

  tags                  = azurerm_resource_group.this.tags
}

# Link the Private Zone with the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "file" {
  name                  = "pdnsz-file-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = var.private_dns_resource_group_name
  private_dns_zone_name = var.private_dns_zone_file_name
  virtual_network_id    = module.vnet_main.id

  tags                  = azurerm_resource_group.this.tags
}

# Link the Private Zone with the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "db" {
  name                  = "pdnsz-database-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = var.private_dns_resource_group_name
  private_dns_zone_name = var.private_dns_zone_database_name
  virtual_network_id    = module.vnet_main.id

  tags                  = azurerm_resource_group.this.tags
}

# Link the Private Zone with the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "servicebus" {
  name                  = "pdnsz-servicebus-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = var.private_dns_resource_group_name
  private_dns_zone_name = var.private_dns_zone_servicebus_name
  virtual_network_id    = module.vnet_main.id

  tags                  = azurerm_resource_group.this.tags
}

# Link the Private Zone with the VNet
resource "azurerm_private_dns_zone_virtual_network_link" "vault" {
  name                  = "pdnsz-keyvault-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = var.private_dns_resource_group_name
  private_dns_zone_name = var.private_dns_zone_keyvault_name
  virtual_network_id    = module.vnet_main.id

  tags                  = azurerm_resource_group.this.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "cosmos" {
  name                  = "pdnsz-cosmos-${lower(var.domain_name_short)}-${lower(var.environment_short)}-${lower(var.environment_instance)}"
  resource_group_name   = var.private_dns_resource_group_name
  private_dns_zone_name = var.private_dns_zone_cosmos_name
  virtual_network_id    = module.vnet_main.id

  tags                  = azurerm_resource_group.this.tags
}

module "kvs_vnet_shared_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=6.0.0"

  name          = "vnet-shared-name"
  value         = module.vnet_main.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_vnet_shared_resource_group_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=6.0.0"

  name          = "vnet-shared-resource-group-name"
  value         = azurerm_resource_group.this.name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_pdns_resource_group_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=6.0.0"

  name          = "pdns-resource-group-name"
  value         = var.private_dns_resource_group_name
  key_vault_id  = module.kv_shared.id

  tags          = azurerm_resource_group.this.tags
}