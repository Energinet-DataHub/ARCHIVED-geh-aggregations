resource "azurerm_databricks_workspace" "databricksworkspace" {
  name                = "dbw-${var.project}-${var.organisation}-${var.environment}"
  resource_group_name = data.azurerm_resource_group.main.name
  location            = data.azurerm_resource_group.main.location
  sku                 = "standard"
  tags                = data.azurerm_resource_group.main.tags
}

provider "databricks" {
  alias = "created_workspace" 
  azure_workspace_resource_id = azurerm_databricks_workspace.databricksworkspace.id
}

# // create PAT token to provision entities within workspace
# resource "databricks_token" "pat" {
#   provider = databricks.created_workspace
# }

# data "databricks_current_user" "me" {
#   depends_on = [azurerm_databricks_workspace.databricksworkspace]
# }

