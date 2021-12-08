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
module "st_data_lake" {
  source                    = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/storage-account?ref=5.1.0"

  name                      = "datalake"
  project_name              = var.domain_name_short
  environment_short         = var.environment_short
  environment_instance      = var.environment_instance
  resource_group_name       = azurerm_resource_group.this.name
  location                  = azurerm_resource_group.this.location
  account_replication_type  = "LRS"
  account_tier              = "Standard"
  is_hns_enabled            = true
  containers                = [
    {
      name  = local.DATA_LAKE_DATA_CONTAINER_NAME,
    },
  ]

  tags                      = azurerm_resource_group.this.tags
}

resource "azurerm_storage_blob" "master_data" {
  name                    = "${local.DATA_LAKE_MASTER_DATA_BLOB_NAME}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_blob" "events" {
  name                    = "${local.DATA_LAKE_EVENTS_BLOB_NAME}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_blob" "results" {
  name                    = "${local.DATA_LAKE_RESULTS_BLOB_NAME}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

resource "azurerm_storage_blob" "snapshots" {
  name                    = "${local.DATA_LAKE_SNAPSHOTS_BLOB_NAME}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.DATA_LAKE_DATA_CONTAINER_NAME
  type                    = "Block"
}

module "kvs_st_data_lake_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "st-data-lake-name"
  value         = module.st_data_lake.name
  key_vault_id  = module.kv_aggregation.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_primary_access_key" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "st-data-lake-primary-access-key"
  value         = module.st_data_lake.primary_access_key
  key_vault_id  = module.kv_aggregation.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_data_container_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "st-data-lake-data-container-name"
  value         = local.DATA_LAKE_CONTAINER_NAME
  key_vault_id  = module.kv_aggregation.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_master_data_blob_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "st-data-lake-master-data-blob-name"
  value         = local.DATA_LAKE_MASTER_DATA_BLOB_NAME
  key_vault_id  = module.kv_aggregation.id

  tags          = azurerm_resource_group.this.tags
}

module "kvs_st_data_lake_events_blob_name" {
  source        = "git::https://github.com/Energinet-DataHub/geh-terraform-modules.git//azure/key-vault-secret?ref=5.1.0"

  name          = "st-data-lake-events-blob-name"
  value         = local.DATA_LAKE_EVENTS_BLOB_NAME
  key_vault_id  = module.kv_aggregation.id

  tags          = azurerm_resource_group.this.tags
}