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
resource "azurerm_cosmosdb_account" "masterdata" {
  name                = "cosmos-masterdata-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  # To enable global failover change to true and uncomment second geo_location
  enable_automatic_failover = false

  consistency_policy {
    consistency_level = "Session"
  }
  
  geo_location {
    location          = data.azurerm_resource_group.main.location
    failover_priority = 0
  }

  tags                = data.azurerm_resource_group.main.tags
}

resource "azurerm_cosmosdb_sql_database" "db" {
  name                = "masterdata"
  resource_group_name = data.azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.masterdata.name
}

resource "azurerm_cosmosdb_sql_container" "collection_meteringpoints" {
  name                = "meteringpoints"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
}

resource "azurerm_cosmosdb_sql_container" "collection_timeseries" {
  name                = "timeseries"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
}

resource "azurerm_cosmosdb_sql_container" "collection_marketroles" {
  name                = "marketroles"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
}

resource "azurerm_cosmosdb_sql_container" "collection_charges" {
  name                = "charges"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
}

resource "azurerm_cosmosdb_sql_container" "collection_chargelinks" {
  name                = "chargelinks"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
}
