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
                        StructField("meteringPointId", StringType(), False),
                        StructField("meteringPointType", IntegerType(), False),
                        StructField("meteringGridArea", StringType(), False),
                        StructField("toGridArea", StringType()),
                        StructField("fromGridArea", StringType()),
                        StructField("settlementMethod", IntegerType()),
                        StructField("meteringMethod", IntegerType(), False),
                        StructField("meterReadingPeriodicity", IntegerType(), False),
                        StructField("connectionState", IntegerType(), False),
                        StructField("netSettlementGroup", StringType()),
                        StructField("product", StringType()),
                        StructField("quantityUnit", IntegerType(), False),
                        StructField("parentMeteringPointId", StringType()),
                        StructField("fromDate", TimestampType(), False),
                        StructField("toDate", TimestampType(), False)
])
