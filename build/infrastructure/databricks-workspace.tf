resource "azurerm_databricks_workspace" "databricksworkspace" {
  name                = "dbw-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  sku                 = "standard"
  tags                = data.azurerm_resource_group.main.tags
}

