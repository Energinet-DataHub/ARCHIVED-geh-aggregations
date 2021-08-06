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
locals {
  max_cosmos_throughput = 4000
}

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
  name                = "master-data"
  resource_group_name = data.azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.masterdata.name
}

resource "azurerm_cosmosdb_sql_container" "collection_metering_points" {
  name                = "metering-points"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/metering_point_id"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }
}

resource "azurerm_cosmosdb_sql_container" "collection_market_roles" {
  name                = "market-roles"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/energy_supplier_id"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }
}

resource "azurerm_cosmosdb_sql_container" "collection_charges" {
  name                = "charges"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/charge_type"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }
}

resource "azurerm_cosmosdb_sql_container" "collection_charge_links" {
  name                = "charge-links"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/charge_id"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }  
}

resource "azurerm_cosmosdb_sql_container" "collection_charge_prices" {
  name                = "charge-prices"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/charge_id"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }  
}

resource "azurerm_cosmosdb_sql_container" "collection_grid_loss_sys_corr" {
  name                = "grid-loss-sys-corr"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/grid_area"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }    
}

resource "azurerm_cosmosdb_sql_container" "collection_es_brp_relations" {
  name                = "es-brp-relations"
  resource_group_name = var.resource_group_name
  account_name        = azurerm_cosmosdb_account.masterdata.name
  database_name       = azurerm_cosmosdb_sql_database.db.name
  partition_key_path  = "/grid_area"
  autoscale_settings {
    max_throughput = local.max_cosmos_throughput
  }    
}
