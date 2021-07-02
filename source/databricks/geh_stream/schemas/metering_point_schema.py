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

from pyspark.sql.types import StructType, StructField, StringType, TimestampType, IntegerType

metering_point_schema = StructType([
      StructField("id", StringType(), False),
      StructField("metering_point_id", StringType(), False),
      StructField("metering_point_type", IntegerType(), False),
      StructField("settlement_method", IntegerType()),
      StructField("grid_area", StringType(), False),
      StructField("connection_state", IntegerType(), False),
      StructField("resolution", IntegerType(), False),
      StructField("in_grid_area", StringType()),
      StructField("out_grid_area", StringType()),
      StructField("metering_method", IntegerType(), False),
      StructField("net_settlement_group", StringType()),
      StructField("parent_metering_point_id", StringType()),
      StructField("unit", IntegerType(), False),
      StructField("product", StringType()),
      StructField("from_date", TimestampType(), False),
      StructField("to_date", TimestampType(), False)
])
