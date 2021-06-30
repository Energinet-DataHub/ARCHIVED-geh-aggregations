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

from pyspark.sql.types import StringType, TimestampType
from pyspark import SparkConf
from pyspark.sql import SparkSession


# %% Setup

storage_account_name = "STORAGE_ACCOUNT_NAME" # this must be changed to your storage account name
storage_account_key = "STORAGE_ACCOUNT_KEY"
container_name = "CONTAINER_NAME"
spark.conf.set(
  "fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),
  storage_account_key)
output_delta_lake_path = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/delta/time_series_test_data/"


# %% Read from timeseries testdata csv

test_data_csv_source = "abfss://" + container_name + "@" + storage_account_name + ".dfs.core.windows.net/test-data-base/time-series-test-data.csv"
print(test_data_csv_source)

schema = StructType() \
      .add("MarketEvaluationPoint_mRID",StringType(),True) \
      .add("Quantity",StringType(),True) \
      .add("Quality",StringType(),True) \
      .add("ObservationTime",TimestampType(),True)

csv_df = spark.read.format('csv').options(inferSchema = "true", delimiter=";", header="true").schema(schema).load(test_data_csv_source)


# %% Filter to get only valid rows. Save data to deltatable (overwrites existing data)

csv_df.filter(col("MarketEvaluationPoint_mRID").isNotNull()) \
  .write.format("delta").mode("overwrite").save(output_delta_lake_path)
