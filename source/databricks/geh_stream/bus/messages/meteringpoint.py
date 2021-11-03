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
from geh_stream.codelists.colname import Colname
from geh_stream.schemas import metering_point_schema
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
    # Event properties:

    metering_point_id: StringType()
    metering_point_type: StringType()
    gsrn_number: StringType()
    grid_area_code: StringType()
    settlement_method: StringType()
    metering_method: StringType()
    meter_reading_periodicity: StringType()
    net_settlement_group: StringType()
    product: StringType()
    parent_id: StringType()
    connection_state: StringType()
    unit_type: StringType()
    effective_date: StringType()

    # What to do when we want the dataframe for this event
    def get_dataframe(self):
        effective_date = dateutil.parser.parse(self.effective_date)

        create_consumption_mp_event = [(
            self.gsrn_number,
            self.metering_point_type,
            self.settlement_method,
            self.grid_area_code,
            self.connection_state,
            self.meter_reading_periodicity,
            "",#  in_grid_area
            "",#  out_grid_area
            self.metering_method,
            self.net_settlement_group,
            self.parent_id,
            self.unit_type,
            self.product,
            effective_date,
            datetime(9999, 1, 1, 0, 0))]

        return SparkSession.builder.getOrCreate().createDataFrame(create_consumption_mp_event, schema=metering_point_schema)


@dataclass_json
@dataclass
class SettlementMethodUpdated(MeteringPointBase):
    settlement_method_updated_schema = StructType([
        StructField(Colname.metering_point_id, StringType(), False),
        StructField(Colname.settlement_method, StringType(), False),
        StructField(Colname.effective_date, TimestampType(), False),
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
