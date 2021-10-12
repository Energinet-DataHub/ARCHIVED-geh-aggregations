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

# .option("checkpointLocation", checkpoint_path) \
# from pyspark.sql import DataFrame
# from pyspark.sql.functions import col, from_json, explode
# from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType, ArrayType, BinaryType, IntegerType
# import json
# import datetime
# from delta.tables import *

# connectionString = "Endpoint=sb://evhnm-aggregation-aggregations-endk-u.servicebus.windows.net/;SharedAccessKeyName=evhar-aggregation-listener;SharedAccessKey=65Pfzom3sMCgStfORF+PlVzbMWxFasZaqXR+uWJCc/Q=;EntityPath=evh-aggregation"
# conf = {}
# conf["eventhubs.connectionString"] = sc._jvm.org.apache.spark.eventhubs.EventHubsUtils.encrypt(connectionString)
