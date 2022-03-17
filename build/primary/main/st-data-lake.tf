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

resource "azurerm_storage_container" "container" {
  name                  = local.DATA_LAKE_DATA_CONTAINER_NAME
  storage_account_name  = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
}

resource "azurerm_storage_blob" "master_data" {
  name                    = "${local.DATA_LAKE_MASTER_DATA_BLOB_NAME}/notused"
  storage_account_name    = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_blob" "events" {
  name                    = "${local.DATA_LAKE_EVENTS_BLOB_NAME}/notused"
  storage_account_name    = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_blob" "results" {
  name                    = "${local.DATA_LAKE_RESULTS_BLOB_NAME}/notused"
  storage_account_name    = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_blob" "snapshots" {
  name                    = "${local.DATA_LAKE_SNAPSHOTS_BLOB_NAME}/notused"
  storage_account_name    = data.azurerm_key_vault_secret.st_shared_data_lake_name.value
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

module "kvs_st_data_lake_aggregation_container_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "st-data-lake-aggregation-container-name"
  value         = local.DATA_LAKE_DATA_CONTAINER_NAME
  key_vault_id  = module.kv_aggregations.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_master_data_blob_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.6.0"

  name          = "st-data-lake-master-data-blob-name"
  value         = local.DATA_LAKE_MASTER_DATA_BLOB_NAME
  key_vault_id  = module.kv_aggregations.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_events_blob_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.6.0"

  name          = "st-data-lake-events-blob-name"
  value         = local.DATA_LAKE_EVENTS_BLOB_NAME
  key_vault_id  = module.kv_aggregations.id

  tags          = azurerm_resource_group.this.tags
}