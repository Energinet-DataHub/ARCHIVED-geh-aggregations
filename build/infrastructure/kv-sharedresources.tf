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
data "azurerm_key_vault" "kv_sharedresources" {
  name                = var.sharedresources_keyvault_name
  resource_group_name = var.sharedresources_resource_group_name
}

data "azurerm_key_vault_secret" "POST_OFFICE_QUEUE_CONNECTION_STRING" {
  name         = "POST-OFFICE-QUEUE-CONNECTION-STRING"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "POST-OFFICE-QUEUE-AGGREGATIONS-TOPIC-NAME" {
  name         = "POST-OFFICE-QUEUE-AGGREGATIONS-TOPIC-NAME"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "SHARED-RESOURCES-DB-ADMIN-NAME" {
  name         = "SHARED-RESOURCES-DB-ADMIN-NAME"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}

data "azurerm_key_vault_secret" "SHARED-RESOURCES-DB-ADMIN-PASSWORD" {
  name         = "SHARED-RESOURCES-DB-ADMIN-PASSWORD"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}
data "azurerm_key_vault_secret" "SHARED-RESOURCES-DB-URL" {
  name         = "SHARED-RESOURCES-DB-URL"
  key_vault_id = data.azurerm_key_vault.kv_sharedresources.id
}



