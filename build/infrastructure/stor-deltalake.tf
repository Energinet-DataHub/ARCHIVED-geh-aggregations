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

module "stor_aggregation_data" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-account?ref=1.7.0"
  name                            = "data${lower(var.project)}${lower(var.organisation)}${lower(var.environment)}"
  resource_group_name             = data.azurerm_resource_group.main.name
  location                        = data.azurerm_resource_group.main.location
  account_replication_type        = "LRS"
  access_tier                     = "Hot"
  account_tier                    = "Standard"
  is_hns_enabled                  = true
  tags                            = data.azurerm_resource_group.main.tags
}

module "stor_aggregation_container" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//storage-container?ref=1.3.0"
  container_name                  = "data-lake"
  storage_account_name            = module.stor_aggregation_data.name
  container_access_type           = "private"
  dependencies                    = [ module.stor_aggregation_data.dependent_on ]
}

resource "azurerm_storage_blob" "master_data" {
  name                            = local.master-data-blob-name
  storage_account_name            = module.stor_aggregation_data.name
  storage_container_name          = module.stor_aggregation_container.name
  type                            = "Block"
}

resource "azurerm_storage_blob" "events" {
  name                            = local.events-blob-name
  storage_account_name            = module.stor_aggregation_data.name
  storage_container_name          = module.stor_aggregation_container.name
  type                            = "Block"
}

resource "azurerm_storage_blob" "results" {
  name                            = local.results-blob-name
  storage_account_name            = module.stor_aggregation_data.name
  storage_container_name          = module.stor_aggregation_container.name
  type                            = "Block"
}

resource "azurerm_storage_blob" "snapshots" {
  name                            = local.snapshots-blob-name
  storage_account_name            = module.stor_aggregation_data.name
  storage_container_name          = module.stor_aggregation_container.name
  type                            = "Block"
}

module "kvs_aggregation_storage_account_key" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "aggregation-storage-account-key"
  value                           = module.stor_aggregation_data.primary_access_key
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}

module "kvs_aggregation_storage_account_name" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "aggregation-storage-account-name"
  value                           = module.stor_aggregation_data.name
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}

module "kvs_aggregation_container_name" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "delta-lake-container-name"
  value                           = module.stor_aggregation_container.name
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}

module "kvs_master_data_blob_name" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "master-data-blob-name"
  value                           = local.master-data-blob-name
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}

module "kvs_events_data_blob_name" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "events-data-blob-name"
  value                           = local.events-blob-name
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}

module "kvs_results_data_blob_name" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "results-data-blob-name"
  value                           = local.results-blob-name
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}

module "kvs_snapshots_data_blob_name" {
  source                          = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//key-vault-secret?ref=1.3.0"
  name                            = "snapshots-data-blob-name"
  value                           = local.snapshots-blob-name
  key_vault_id                    = module.kv_aggregation.id
  dependencies = [
    module.kv_aggregation.dependent_on,
    module.stor_aggregation_data.dependent_on
  ]
}