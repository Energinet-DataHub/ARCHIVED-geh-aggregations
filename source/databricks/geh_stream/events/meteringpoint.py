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

from dataclasses import dataclass
from .broker import Message
from pyspark.sql.types import StructType, StringType, StructField, TimestampType
from dataclasses_json import dataclass_json  # https://pypi.org/project/dataclasses-json/

# Integration event schemas

settlement_method_updated_schema = StructType([
    StructField("metering_point_id", StringType(), False),
    StructField("settlement_method", StringType(), False),
    StructField("effective_date", TimestampType(), False),
])


@dataclass_json
@dataclass
class ConsumptionMeteringPointCreated(Message):
    consumption_metering_point_created_event_schema = StructType([
        StructField("metering_point_id", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("metering_gsrn_number", StringType(), False),
        StructField("metering_grid_area", StringType(), False),
        StructField("settlement_method", StringType(), False),
        StructField("metering_method", StringType(), False),
        StructField("meter_reading_periodicity", StringType(), False),
        StructField("net_settlement_group", StringType(), False),
        StructField("product", StringType(), False),
        StructField("effective_date", TimestampType(), False),
    ])
    metering_point_id: str
    metering_point_type: str
    metering_grid_area: str
    settlement_method: str
    metering_method: str
    meter_reading_periodicity: str
    net_settlement_group: str
    product: str
    effective_date: str

    def get_dataframe(self):
        create_consumption_mp_event = [(
            self.metering_point_id,
            self.metering_point_type,
            self.grid_area,
            self.settlement_method,
            self.metering_method,
            self.meter_reading_periodicity,
            self.net_settlement_group,
            self.product,
            self.effective_date)]
        return self._spark.createDataFrame(create_consumption_mp_event, schema=self.consumption_metering_point_created_event_schema)


@dataclass_json
@dataclass
class SettlementMethodUpdated(Message):
    metering_point_id: str
    settlement_method: str
    effective_date: str
