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
from os import truncate
from time import time
from geh_stream.wholesale_utils.wholesale_initializer import get_charges
from geh_stream.codelists import Colname
from pyspark.sql.functions import to_date
import pytest
import pandas as pd
from pyspark.sql.types import StructType, StringType, TimestampType, DecimalType


@pytest.fixture(scope="module")
def charges_schema():
    return StructType() \
        .add(Colname.charge_id, StringType(), False) \
        .add(Colname.charge_type, StringType(), False) \
        .add(Colname.charge_owner, StringType(), False) \
        .add(Colname.resolution, StringType(), False) \
        .add(Colname.charge_tax, StringType(), False) \
        .add(Colname.currency, StringType(), False) \
        .add(Colname.from_date, TimestampType(), False) \
        .add(Colname.to_date, TimestampType(), False)


@pytest.fixture(scope="module")
def charge_links_schema():
    return StructType() \
        .add(Colname.charge_id, StringType(), False) \
        .add(Colname.metering_point_id, StringType(), False) \
        .add(Colname.from_date, TimestampType(), False) \
        .add(Colname.to_date, TimestampType(), False)


@pytest.fixture(scope="module")
def charge_prices_schema():
    return StructType() \
        .add(Colname.charge_id, StringType(), False) \
        .add(Colname.charge_price, DecimalType(), False) \
        .add(Colname.time, TimestampType(), False) \


@pytest.fixture(scope="module")
def market_roles_schema():
    return StructType() \
        .add(Colname.energy_supplier_id, StringType(), False) \
        .add(Colname.metering_point_id, StringType(), False) \
        .add(Colname.from_date, TimestampType(), False) \
        .add(Colname.to_date, TimestampType(), False)


@pytest.fixture(scope="module")
def metering_points_schema():
    return StructType() \
        .add(Colname.metering_point_id, StringType(), False) \
        .add(Colname.metering_point_type, StringType(), False) \
        .add(Colname.settlement_method, StringType(), False) \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.connection_state, StringType(), False) \
        .add(Colname.resolution, StringType(), False) \
        .add(Colname.in_grid_area, StringType(), False) \
        .add(Colname.out_grid_area, StringType(), False) \
        .add(Colname.metering_method, StringType(), False) \
        .add(Colname.net_settlement_group, StringType(), False) \
        .add(Colname.parent_metering_point_id, StringType(), False) \
        .add(Colname.unit, StringType(), False) \
        .add(Colname.product, StringType(), False) \
        .add(Colname.from_date, TimestampType(), False) \
        .add(Colname.to_date, TimestampType(), False)


@pytest.fixture(scope="module")
def charges_factory(spark, charges_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.charge_id: [],
            Colname.charge_type: [],
            Colname.charge_owner: [],
            Colname.resolution: [],
            Colname.charge_tax: [],
            Colname.currency: [],
            Colname.from_date: [],
            Colname.to_date: [],
        })
        periods = [datetime(2020, 1, 3), datetime(2020, 1, 11),
                   datetime(2020, 1, 8), datetime(2020, 1, 17),
                   datetime(2020, 1, 2), datetime(2020, 1, 21),
                   datetime(2020, 1, 9), datetime(2020, 1, 12),
                   datetime(2020, 1, 1), datetime(2020, 1, 4),
                   datetime(2020, 1, 15), datetime(2020, 1, 19)]
        for i in range(6):
            pandas_df = pandas_df.append([{
                Colname.charge_id: "charge" + str(i),
                Colname.charge_type: "D03",
                Colname.charge_owner: "8500000000502",
                Colname.resolution: "P1D",
                Colname.charge_tax: "FALSE",
                Colname.currency: "DKK",
                Colname.from_date: periods[i + i],
                Colname.to_date: periods[i + i + 1]
            }], ignore_index=True)

        return spark.createDataFrame(pandas_df, schema=charges_schema)
    return factory


@pytest.fixture(scope="module")
def charge_links_factory(spark, charge_links_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.charge_id: [],
            Colname.metering_point_id: [],
            Colname.from_date: [],
            Colname.to_date: [],
        })
        for i in range(20):
            pandas_df = pandas_df.append([{
                Colname.charge_id: "charge" + str((i % 5) + 1),
                Colname.metering_point_id: i,
                Colname.from_date: datetime(2020, 1, 1),
                Colname.to_date: datetime(2020, 1, 21)
            }])
        return spark.createDataFrame(pandas_df, schema=charge_links_schema)
    return factory


@pytest.fixture(scope="module")
def charge_prices_factory(spark, charge_prices_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.charge_id: [],
            Colname.charge_price: [],
            Colname.time: [],
        })
        for i in range(3):
            charge_time = datetime(2020, 1, i + 1)
            for j in range(24):
                pandas_df = pandas_df.append([{
                    Colname.charge_id: "charge" + str(i),
                    Colname.charge_price: DecimalType(i + j),
                    Colname.time: charge_time
                }])
        return spark.createDataFrame(pandas_df, schema=charge_prices_schema)
    return factory


@pytest.fixture(scope="module")
def metering_points_factory(spark, metering_points_schema):
    def factory():
        pandas_df = pd.DataFrame({
        })
        for i in range():
            pandas_df = pandas_df.append([{
            }])
        return spark.createDataFrame(pandas_df, schema=metering_points_schema)
    return factory


@pytest.fixture(scope="module")
def market_roles_factory(spark, market_roles_schema):
    def factory():
        pandas_df = pd.DataFrame({
        })
        for i in range():
            pandas_df = pandas_df.append([{
            }])
        return spark.createDataFrame(pandas_df, schema=market_roles_schema)
    return factory


def test_get_charges(charges_factory, charge_links_factory, charge_prices_factory, metering_points_factory, market_roles_factory):
    charges = charges_factory()
    charges.show()
    charge_links = charge_links_factory()
    charge_links.show()
    charge_prices = charge_prices_factory()
    charge_prices.show(1000, truncate=False)
