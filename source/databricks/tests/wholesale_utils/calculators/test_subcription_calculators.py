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
from geh_stream.codelists import Quality
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from pyspark.sql.functions import col, to_date
import pytest
import pandas as pd

default_charge_id: str = "chargea"
default_charge_type: str = "D01"
default_charge_owner: str = "001"
default_charge_key: str = f"{default_charge_id}-{default_charge_type}-{default_charge_owner}"
default_resolution: str = ""
default_charge_tax: str = ""
default_currency: str = ""


@pytest.fixture(scope="module")
def charges_factory(spark, charges_schema):
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key=default_charge_key,
        charge_type=default_charge_type,
        charge_owner=default_charge_owner,
        resolution=default_resolution,
        charge_tax=default_charge_tax,
        currency=default_currency
    ):
        pandas_df = pd.DataFrame({
            Colname.charge_key: [],
            Colname.charge_type: [],
            Colname.charge_owner: [],
            Colname.resolution: [],
            Colname.charge_tax: [],
            Colname.currency: [],
            Colname.from_date: [],
            Colname.to_date: []
        })
        pandas_df = pandas_df.append([{
            Colname.charge_key: charge_key,
            Colname.charge_type: charge_type,
            Colname.charge_owner: charge_owner,
            Colname.from_date: from_date,
            Colname.resolution: resolution,
            Colname.charge_tax: charge_tax,
            Colname.currency: currency,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charges_schema)
    return factory


default_metering_point_id: str = "D01"


@pytest.fixture(scope="module")
def charge_links_factory(spark, charge_links_schema):
    def factory(
        from_date: datetime,
        to_date: datetime,
        charge_key=default_charge_key,
        metering_point_id=default_metering_point_id
    ):
        pandas_df = pd.DataFrame({
            Colname.charge_key: [],
            Colname.metering_point_id: [],
            Colname.from_date: [],
            Colname.to_date: []
        })
        pandas_df = pandas_df.append([{
            Colname.charge_key: charge_key,
            Colname.metering_point_id: metering_point_id,
            Colname.from_date: from_date,
            Colname.to_date: to_date}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charge_links_schema)
    return factory


default_charge_price: Decimal = 1.12345678


@pytest.fixture(scope="module")
def charge_prices_factory(spark, charge_prices_schema):
    def factory(
        time: datetime,
        charge_key=default_charge_key,
        charge_price=default_charge_price
    ):
        pandas_df = pd.DataFrame({
            Colname.charge_key: [],
            Colname.charge_price: [],
            Colname.time: [],
        })
        pandas_df = pandas_df.append([{
            Colname.charge_key: charge_key,
            Colname.charge_price: charge_price,
            Colname.time: time}],
            ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charge_prices_schema)
    return factory
