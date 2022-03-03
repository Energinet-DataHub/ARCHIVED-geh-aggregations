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
data "databricks_node_type" "smallest" {
  local_disk = true
}

data "databricks_spark_version" "latest_lts" {
  long_term_support = true
}

resource "databricks_cluster" "aggregations_autoscaling" {
  cluster_name            = "Aggregations Autoscaling"
  spark_version           = data.databricks_spark_version.latest_lts.id
  node_type_id            = data.databricks_node_type.smallest.id
  autotermination_minutes = 20
  autoscale {
    min_workers = 2
    max_workers = 8
  }

  # library {
  #   pypi {
  #     package = "configargparse==1.2.3"
  #   }
  # }

  # library {
  #   pypi {
  #     package = "azure-storage-blob==12.8.0"
  #   }
  # }

  # library {
  #   maven {
  #     coordinates = "com.azure.cosmos.spark:azure-cosmos-spark_3-1_2-12:4.1.0"
  #   }
  # }

  # library {
  #   whl = "dbfs:/aggregation/geh_stream-x-py3-none-any.whl"
  # }
}