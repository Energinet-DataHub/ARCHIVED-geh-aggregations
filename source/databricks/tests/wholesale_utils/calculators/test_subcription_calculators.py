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
from tests.helpers.dataframe_creators.charges_creator import charges_factory, charge_links_factory, charge_prices_factory
from tests.helpers.dataframe_creators.metering_point_creator import metering_point_factory
from tests.helpers.dataframe_creators.market_roles_creator import market_roles_factory
from geh_stream.codelists import Colname
from geh_stream.wholesale_utils.calculators import subscription_calculators
from geh_stream.codelists import Quality, ResolutionDuration
from pyspark.sql import dataframe
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, StructField
from pyspark.sql.functions import col, to_date
import pytest
import pandas as pd


def test_charges(charges_factory, charge_links_factory, charge_prices_factory, metering_point_factory, market_roles_factory):
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    time = datetime(2020, 1, 1, 0, 0)
    charges_df = charges_factory(from_date, to_date)
    charges_df2 = charges_factory(from_date=datetime(2020, 1, 2, 0, 0), to_date=datetime(2020, 1, 3, 0, 0))
    charges_df = charges_df.union(charges_df2)
    charge_links_df = charge_links_factory(from_date, to_date)
    charge_prices_df = charge_prices_factory(time)
    metering_point_df = metering_point_factory(from_date, to_date)
    market_roles_df = market_roles_factory(from_date, to_date)
    charges_df.show()
    charge_links_df.show()
    charge_prices_df.show()
    metering_point_df.show()
    market_roles_df.show()
    df = subscription_calculators.calculate_daily_subscription_price(charges_df, charge_links_df, charge_prices_df, metering_point_df, market_roles_df)
    df.show()
    assert True
