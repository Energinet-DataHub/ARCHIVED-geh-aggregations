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
from decimal import Decimal
from datetime import datetime
from geh_stream.codelists import Colname
from geh_stream.wholesale_utils.calculators import subscription_calculators
from geh_stream.schemas import charges_schema, charge_links_schema, charge_prices_schema, metering_point_schema, market_roles_schema
from geh_stream.codelists import Quality, ResolutionDuration
from pyspark.sql import dataframe
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, StructField
from pyspark.sql.functions import col, to_date
import pytest
import pandas as pd


default_metering_point_id: str = "1"
default_metering_point_type: str = "chargea"
default_settlement_method: str = "1"
default_metering_point_type: str = "chargea"
default_metering_point_id: str = "1"
default_metering_point_type: str = "chargea"
default_metering_point_id: str = "1"
default_metering_point_type: str = "chargea"
default_metering_point_id: str = "1"
default_metering_point_type: str = "chargea"
default_metering_point_id: str = "1"
default_metering_point_type: str = "chargea"


@pytest.fixture(scope="module")
def metering_point_factory(spark):
    def factory(
        from_date: datetime,
        to_date: datetime,
        metering_point_id=default_metering_point_id,
        metering_point_type=default_metering_point_type,
        settlement_method=default_settlement_method,
        grid_area=default_grid_area,
        connection_state=default_connection_state,
        resolution=default_resolution,
        in_grid_area=default_in_grid_area,
        out_grid_area=default_out_grid_area,
        metering_method=default_metering_method,
        net_settlement_group=default_net_settlement_group,
        parent_metering_point_id=default_parent_metering_point_id,
        unit=default_unit,
        product=default_product
    ):
        pandas_df = pd.DataFrame().append([{
            Colname.metering_point_id: metering_point_id,
            Colname.metering_point_type: metering_point_type,
            Colname.settlement_method: settlement_method,
            Colname.grid_area: grid_area,
            Colname.connection_state: connection_state,
            Colname.resolution: resolution,
            Colname.in_grid_area: in_grid_area,
            Colname.out_grid_area: out_grid_area,
            Colname.metering_method: metering_method,
            Colname.net_settlement_group: net_settlement_group,
            Colname.parent_metering_point_id: parent_metering_point_id,
            Colname.unit: unit,
            Colname.product: product,
            Colname.from_date: from_date,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charges_schema)
    return factory