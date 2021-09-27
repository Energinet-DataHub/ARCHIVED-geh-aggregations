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
from decimal import Decimal
from os import truncate
from time import time
from geh_stream.wholesale_utils.wholesale_initializer import get_tariff_charges, \
    join_with_charge_prices, \
    join_with_charge_links, \
    join_with_martket_roles, \
    join_with_metering_points, \
    explode_subscription, \
    filter_on_resolution, \
    group_by_time_series_on_metering_point_id_and_resolution_and_sum_quantity, \
    join_with_grouped_time_series
from geh_stream.codelists import Colname, ChargeType, ResolutionDuration
from geh_stream.schemas import charges_schema, charge_prices_schema, charge_links_schema, metering_point_schema, market_roles_schema
from tests.helpers.test_schemas import charges_with_prices_schema, charges_with_price_and_links_schema, charges_with_price_and_links_and_market_roles_schema
from pyspark.sql.functions import to_date
import pytest
import pandas as pd
from pyspark.sql.types import NullType, StructType, StringType, TimestampType, DecimalType


charges_dataset = [
    ("001-D01-001", "001", ChargeType.tariff, "001", ResolutionDuration.day, "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0)),
    ("001-D01-001", "001", ChargeType.tariff, "001", ResolutionDuration.day, "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0)),
    ("001-D01-001", "001", ChargeType.tariff, "001", ResolutionDuration.hour, "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0)),
    ("001-D01-001", "001", ChargeType.tariff, "001", ResolutionDuration.month, "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))
]


@pytest.mark.parametrize("charges,resolution_duration,expected", [
    (charges_dataset, ResolutionDuration.hour, 1),
    (charges_dataset, ResolutionDuration.day, 2)
])
def test__filter_on_resolution__filters_on_resolution_hour_or_day_only_for_tariff(spark, charges, resolution_duration, expected):
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)

    # Act
    result = filter_on_resolution(charges, resolution_duration)

    # Assert
    assert result.count() == expected


charges_dataset = [("001-D01-001", "001", "D01", "001", "P1D", "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]
charge_prices_dataset = [("001-D01-001", Decimal("200.50"), datetime(2020, 1, 2, 0, 0)),
                         ("001-D01-001", Decimal("100.50"), datetime(2020, 1, 5, 0, 0)),
                         ("001-D01-002", Decimal("100.50"), datetime(2020, 1, 6, 0, 0))]


@pytest.mark.parametrize("charges,charge_prices,charge_type,expected", [
    (charges_dataset, charge_prices_dataset, ChargeType.subscription, 2)
])
def test__join_with_charge_prices__joins_on_charge_key(spark, charges, charge_prices, charge_type, expected):
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)
    charge_prices = spark.createDataFrame(charge_prices, schema=charge_prices_schema)

    # Act
    result = join_with_charge_prices(charges, charge_prices)

    # Assert
    assert result.count() == expected


subscription_charges_with_prices_dataset_1 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 2, 0, 0), Decimal("200.50"))]
subscription_charges_with_prices_dataset_2 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2021, 1, 2, 0, 0), Decimal("200.50"))]
subscription_charges_with_prices_dataset_3 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 2, 0, 0), datetime(2020, 2, 15, 0, 0), Decimal("200.50"))]
subscription_charges_with_prices_dataset_4 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 3, 1, 0, 0), Decimal("200.50"))]


@pytest.mark.parametrize("subscription_charges_with_prices,expected", [
    (subscription_charges_with_prices_dataset_1, 31),
    (subscription_charges_with_prices_dataset_2, 0),
    (subscription_charges_with_prices_dataset_3, 2),
    (subscription_charges_with_prices_dataset_4, 0)
])
def test__explode_subscription__explodes_into_rows_based_on_number_of_days_between_from_and_to_date(spark, subscription_charges_with_prices, expected):
    # Arrange
    subscription_charges_with_prices = spark.createDataFrame(subscription_charges_with_prices, schema=charges_with_prices_schema)

    # Act
    result = explode_subscription(subscription_charges_with_prices)

    # Assert
    assert result.count() == expected


charges_with_prices_dataset_1 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 15, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_2 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2021, 2, 1, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_3 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 1, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_4 = [("001-D01-002", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 15, 0, 0), Decimal("200.50"))]
charge_links_dataset = [("001-D01-001", "D01", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges_with_prices,charge_links,expected", [
    (charges_with_prices_dataset_1, charge_links_dataset, 1),
    (charges_with_prices_dataset_2, charge_links_dataset, 0),
    (charges_with_prices_dataset_3, charge_links_dataset, 1),
    (charges_with_prices_dataset_4, charge_links_dataset, 0)
])
def test__join_with_charge_links__joins_on_charge_key_and_time_is_between_from_and_to_date(spark, charges_with_prices, charge_links, expected):
    # Arrange
    charges_with_prices = spark.createDataFrame(charges_with_prices, schema=charges_with_prices_schema)
    charge_links = spark.createDataFrame(charge_links, schema=charge_links_schema)

    # Act
    result = join_with_charge_links(charges_with_prices, charge_links)

    # Assert
    assert result.count() == expected


charges_with_price_and_links_dataset_1 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "D01")]
charges_with_price_and_links_dataset_2 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 2, 1, 0, 0), Decimal("200.50"), "D01")]
charges_with_price_and_links_dataset_3 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), Decimal("200.50"), "D01")]
charges_with_price_and_links_dataset_4 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "D02")]
market_roles_dataset = [("1", "D01", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges_with_price_and_links,market_roles,expected", [
    (charges_with_price_and_links_dataset_1, market_roles_dataset, 1),
    (charges_with_price_and_links_dataset_2, market_roles_dataset, 0),
    (charges_with_price_and_links_dataset_3, market_roles_dataset, 1),
    (charges_with_price_and_links_dataset_4, market_roles_dataset, 0)
])
def test__join_with_martket_roles__joins_on_metering_point_id_and_time_is_between_from_and_to_date(spark, charges_with_price_and_links, market_roles, expected):
    # Arrange
    charges_with_price_and_links = spark.createDataFrame(charges_with_price_and_links, schema=charges_with_price_and_links_schema)
    market_roles = spark.createDataFrame(market_roles, schema=market_roles_schema)

    # Act
    result = join_with_martket_roles(charges_with_price_and_links, market_roles)

    # Assert
    assert result.count() == expected


charges_with_price_and_links_and_market_roles_dataset_1 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "D01", "1")]
charges_with_price_and_links_and_market_roles_dataset_2 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 2, 1, 0, 0), Decimal("200.50"), "D01", "1")]
charges_with_price_and_links_and_market_roles_dataset_3 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 1, 0, 0), Decimal("200.50"), "D01", "1")]
charges_with_price_and_links_and_market_roles_dataset_4 = [("001-D01-001", "001", "D01", "001", "P1D", "No", datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "D02", "1")]
metering_points_dataset = [("D01", "E17", "D01", "1", "1", "P1D", "2", "1", "1", "1", "1", "1", "1", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges_with_price_and_links_and_market_roles,metering_points,expected", [
    (charges_with_price_and_links_and_market_roles_dataset_1, metering_points_dataset, 1),
    (charges_with_price_and_links_and_market_roles_dataset_2, metering_points_dataset, 0),
    (charges_with_price_and_links_and_market_roles_dataset_3, metering_points_dataset, 1),
    (charges_with_price_and_links_and_market_roles_dataset_4, metering_points_dataset, 0)
])
def test__join_with_metering_points__joins_on_metering_point_id_and_time_is_between_from_and_to_date(spark, charges_with_price_and_links_and_market_roles, metering_points, expected):
    # Arrange
    charges_with_price_and_links_and_market_roles = spark.createDataFrame(charges_with_price_and_links_and_market_roles, schema=charges_with_price_and_links_and_market_roles_schema)
    metering_points = spark.createDataFrame(metering_points, schema=metering_point_schema)

    # Act
    result = join_with_metering_points(charges_with_price_and_links_and_market_roles, metering_points)

    # Assert
    assert result.count() == expected
