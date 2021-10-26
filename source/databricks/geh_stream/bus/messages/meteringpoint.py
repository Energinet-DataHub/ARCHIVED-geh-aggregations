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

from datetime import datetime
from dataclasses import dataclass
from geh_stream.bus.broker import Message
from pyspark.sql.session import SparkSession
from pyspark.sql.types import StructType, StringType, StructField, TimestampType
from dataclasses_json import dataclass_json  # https://pypi.org/project/dataclasses-json/
import dateutil.parser
from pyspark.sql.functions import lit


class MeteringPointBase(Message):

    @property
    def get_master_data_path(self):
        return "/meteringpoint"


@dataclass_json
@dataclass
class ConsumptionMeteringPointCreated(MeteringPointBase):
    # master data schema
    consumption_metering_point_created_event_schema = StructType([
        StructField("metering_point_id", StringType(), False),
        StructField("metering_point_type", StringType(), False),
        StructField("parent_id", StringType(), False),
        StructField("resolution", StringType(), False),
        StructField("unit", StringType(), False),
        StructField("metering_gsrn_number", StringType(), False),
        StructField("metering_grid_area", StringType(), False),
        StructField("settlement_method", StringType(), False),
        StructField("metering_method", StringType(), False),
        StructField("meter_reading_periodicity", StringType(), False),
        StructField("net_settlement_group", StringType(), False),
        StructField("product", StringType(), False),
        StructField("valid_from", TimestampType(), False),
        StructField("valid_to", TimestampType(), False),
    ])
    # Event properties:
    metering_point_id: StringType()
    metering_point_type: StringType()
    parent_id: StringType()
    resolution: StringType()
    unit: StringType()
    metering_gsrn_number: StringType()
    metering_grid_area: StringType()
    settlement_method: StringType()
    metering_method: StringType()
    meter_reading_periodicity: StringType()
    net_settlement_group: StringType()
    product: StringType()
    effective_date: StringType()

    # What to do when we want the dataframe for this event
    def get_dataframe(self):
        effective_date = dateutil.parser.parse(self.effective_date)

        create_consumption_mp_event = [(
            self.metering_point_id,
            self.metering_point_type,
            self.parent_id,
            self.resolution,
            self.unit,
            self.metering_gsrn_number,
            self.metering_grid_area,
            self.settlement_method,
            self.metering_method,
            self.meter_reading_periodicity,
            self.net_settlement_group,
            self.product,
            effective_date,
            datetime(9999, 1, 1, 0, 0))]
        return SparkSession.builder.getOrCreate().createDataFrame(create_consumption_mp_event, schema=self.consumption_metering_point_created_event_schema)


@dataclass_json
@dataclass
class SettlementMethodUpdated(MeteringPointBase):
    settlement_method_updated_schema = StructType([
        StructField("metering_point_id", StringType(), False),
        StructField("settlement_method", StringType(), False),
        StructField("effective_date", TimestampType(), False),
    ])

    metering_point_id: StringType()
    settlement_method: StringType()
    effective_date: TimestampType()

    def get_dataframe(self):
        effective_date = dateutil.parser.parse(self.effective_date)

        settlement_method_updated_event = [(
            self.metering_point_id,
            self.settlement_method,
            effective_date)]
        return SparkSession.builder.getOrCreate().createDataFrame(settlement_method_updated_event, schema=self.settlement_method_updated_schema)
