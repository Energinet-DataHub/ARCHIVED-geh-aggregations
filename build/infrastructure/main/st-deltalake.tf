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

  name                      = "-------------------------dAFsafasfgsa-----------------------"
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
      name  = local.data_lake_data_container_name,
    },
  ]

  tags                      = azurerm_resource_group.this.tags
}

resource "azurerm_storage_blob" "master_data" {
  name                    = "${local.data_lake_master_data_blob_name}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.data_lake_data_container_name
  type                    = "Block"
}

resource "azurerm_storage_blob" "events" {
  name                    = "${local.data_lake_events_blob_name}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.data_lake_data_container_name
  type                    = "Block"
}

resource "azurerm_storage_blob" "results" {
  name                    = "${local.data_lake_results_blob_name}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.data_lake_data_container_name
  type                    = "Block"
}

resource "azurerm_storage_blob" "snapshots" {
  name                    = "${local.data_lake_snapshots_blob_name}/notused"
  storage_account_name    = module.st_data_lake.name
  storage_container_name  = local.data_lake_data_container_name
  type                    = "Block"
}

#  st name         = module.st_data_lake.name

# st primary key = module.st_data_lake.primary_access_key

# container name = local.data_lake_data_container_name

# blob name = local.data_lake_master_data_blob_name