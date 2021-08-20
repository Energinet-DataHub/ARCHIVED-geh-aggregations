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
from datetime import date, datetime
from tests.helpers.dataframe_creators.charges_creator import charges_factory, charge_links_factory, charge_prices_factory
from tests.helpers.dataframe_creators.metering_point_creator import metering_point_factory
from tests.helpers.dataframe_creators.market_roles_creator import market_roles_factory
from tests.helpers.dataframe_creators.calculate_daily_subscription_price_creator import calculate_daily_subscription_price_factory
from geh_stream.codelists import Colname
from geh_stream.wholesale_utils.calculators.subscription_calculators import calculate_daily_subscription_price
from calendar import monthrange
import pytest
import pandas as pd


def test_calculate_daily_subscription_price_simple(spark, charges_factory, charge_links_factory, charge_prices_factory, metering_point_factory, market_roles_factory, calculate_daily_subscription_price_factory):
    # Arrange
    from_date = datetime(2020, 1, 1, 0, 0)
    to_date = datetime(2020, 1, 2, 0, 0)
    time = datetime(2020, 1, 1, 0, 0)
    charges_df = charges_factory(from_date, to_date)
    charge_links_df = charge_links_factory(from_date, to_date)
    charge_prices_df = charge_prices_factory(time)
    metering_point_df = metering_point_factory(from_date, to_date)
    market_roles_df = market_roles_factory(from_date, to_date)

    expected_date = datetime(2020, 1, 1, 0, 0)
    expected_price_per_day = Decimal(charge_prices_df.collect()[0][Colname.charge_price] / monthrange(expected_date.year, expected_date.month)[1])

    # Act
    result = calculate_daily_subscription_price(spark, charges_df, charge_links_df, charge_prices_df, metering_point_df, market_roles_df).collect()[0]
    expected = calculate_daily_subscription_price_factory(expected_date, expected_price_per_day, 1, expected_price_per_day).collect()[0]

    # Assert
    assert result == expected
    # assert result[Colname.charge_key] == expected[Colname.charge_key]
    # assert result[Colname.charge_id] == expected[Colname.charge_id]
    # assert result[Colname.charge_type] == expected[Colname.charge_type]
    # assert result[Colname.charge_price] == expected[Colname.charge_price]
    # assert result[Colname.metering_point_type] == expected[Colname.metering_point_type]
    # assert result[Colname.settlement_method] == expected[Colname.settlement_method]
    # assert result[Colname.grid_area] == expected[Colname.grid_area]
    # assert result[Colname.connection_state] == expected[Colname.connection_state]
    # assert result[Colname.energy_supplier_id] == expected[Colname.energy_supplier_id]
    # assert result[Colname.date] == expected[Colname.date]
    # assert result[Colname.price_per_day] == expected[Colname.price_per_day]
    # assert result[Colname.subcription_count] == expected[Colname.subcription_count]
    # assert result[Colname.total_daily_subscription_price] == expected[Colname.total_daily_subscription_price]


def test_calculate_daily_subscription_price_charge_price_change(spark, charges_factory, charge_links_factory, charge_prices_factory, metering_point_factory, market_roles_factory, calculate_daily_subscription_price_factory):
    # Arrange
    from_date = datetime(2020, 1, 15, 0, 0)
    to_date = datetime(2020, 2, 15, 0, 0)
    charges_df = charges_factory(from_date, to_date)
    charge_links_df = charge_links_factory(from_date, to_date)
    metering_point_df = metering_point_factory(from_date, to_date)
    market_roles_df = market_roles_factory(from_date, to_date)

    subscription_1_charge_prices_charge_price = Decimal("3.124544")
    subcription_1_charge_prices_time = datetime(2020, 1, 15, 0, 0)
    subscription_1_charge_prices_df = charge_prices_factory(subcription_1_charge_prices_time, charge_price=subscription_1_charge_prices_charge_price)
    subcription_2_charge_prices_time = datetime(2020, 2, 1, 0, 0)
    subscription_2_charge_prices_df = charge_prices_factory(subcription_2_charge_prices_time)
    charge_prices_df = subscription_1_charge_prices_df.union(subscription_2_charge_prices_df)

    # Act
    result = calculate_daily_subscription_price(spark, charges_df, charge_links_df, charge_prices_df, metering_point_df, market_roles_df).orderBy(Colname.date)
    result.show(1000, False)

    # Assert
    # assert result == expected
    # pytest -vv -s test_subscription_calculators.py::test_calculate_daily_subscription_price_charge_price_change
