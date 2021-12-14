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
output databricks_workspace_url {
  description = "URL of the created Databricks workspace"
  value       = azurerm_databricks_workspace.dbw_aggregations.workspace_url
  sensitive   = false
}

output kv_aggregations_name {
  description = "Name of the key vault in the Aggregations domain"
  value       = module.kv_aggregations.name
  sensitive   = false
}