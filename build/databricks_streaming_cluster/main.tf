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

resource "databricks_job" "streaming_job" {
  name = "StreamingJob"
  max_retries = 2
  max_concurrent_runs = 1   
  always_running = true

  new_cluster { 
    spark_version           = "8.4.x-scala2.12"
    node_type_id            = "Standard_DS3_v2"
    num_workers    = 1
  }
	
  library {
    maven {
      coordinates = "com.microsoft.azure:azure-eventhubs-spark_2.12:2.3.17"
    }
  }

  library {
    pypi {
      package = "configargparse==1.2.3"
    }
  }

  library {
    pypi {
      package = "azure-storage-blob==12.7.1"
    }
  }

  library {
    pypi {
      package = "dataclasses-json==0.5.6"
    }
  }

  library {
    whl = "geh_stream-x-py3-none-any.whl"
  } 

  spark_python_task {
    python_file = "dbfs:/streaming/streaming.py"
    parameters  = [
       "--event-hub-connection-key=${data.azurerm_key_vault_secret.evh_aggregation_listen_connection_string.value}",
       "--data-storage-account-key=${data.azurerm_key_vault_secret.st_data_lake_primary_access_key.value}",
       "--data-storage-account-name=${data.azurerm_key_vault_secret.st_data_lake_name.value}",
       "--delta-lake-container-name=${data.azurerm_key_vault_secret.st_data_lake_data_container_name.value}",
       "--events-data-blob-name=${data.azurerm_key_vault_secret.st_data_lake_events_blob_name.value}",
       "--master-data-blob-name=${data.azurerm_key_vault_secret.st_data_lake_masterdata_blob_name.value}",
    ]
  }

  email_notifications {
    no_alert_for_skipped_runs = true
  }
}