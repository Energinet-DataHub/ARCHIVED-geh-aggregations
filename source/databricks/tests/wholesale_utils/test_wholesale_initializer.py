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
from geh_stream.codelists.resolution_duration import ResolutionDuration
from time import time
from geh_stream.wholesale_utils.wholesale_initializer import get_tariff_charges, \
    join_charge_prices_with_charges_on_given_charge_type, \
    explode_subscription, join_charge_links_with_charges_with_prices, \
    join_metering_point_with_charges_with_prices_and_links, \
    join_energy_supplier_with_charges
from geh_stream.codelists import Colname, ChargeType
from geh_stream.schemas import charges_schema, charge_prices_schema, charge_links_schema, metering_point_schema, market_roles_schema
from tests.helpers.test_schemas import charges_with_prices_schema, charges_with_price_and_links_schema, charges_with_metering_point_schema
from pyspark.sql.functions import to_date
import pytest
import pandas as pd
from pyspark.sql.types import NullType, StructType, StringType, TimestampType, DecimalType


# TODO: make sure that unit test are added and completed - \lki 23-08-2021 (#269)
# def test_get_tariff_charges(charges_factory, charge_links_factory, charge_prices_factory, market_roles_factory, metering_points_factory):
#     charges = charges_factory()
#     charge_links = charge_links_factory()
#     charge_prices = charge_prices_factory()
#     market_roles = market_roles_factory()
#     metering_points = metering_points_factory()
#     df = get_tariff_charges(charges, charge_links, charge_prices, metering_points, market_roles, ResolutionDuration.day)
#     df.show()


charges_dataset = [("chargea-D01-001", "chargea", "D01", "001", "P1D", "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]
charge_prices_dataset = [("chargea-D01-001", Decimal("200.50"), datetime(2020, 1, 2, 0, 0)),
                         ("chargea-D01-001", Decimal("100.50"), datetime(2020, 1, 5, 0, 0)),
                         ("chargea-D01-002", Decimal("100.50"), datetime(2020, 1, 6, 0, 0))]


@pytest.mark.parametrize("charges,charge_prices,charge_type,expected", [
    (charges_dataset, charge_prices_dataset, ChargeType.subscription, 2)
])
def test__charges_with_prices__join_charge_prices_with_charges_on_given_charge_type(spark, charges, charge_prices, charge_type, expected):
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)
    charge_prices = spark.createDataFrame(charge_prices, schema=charge_prices_schema)

    # Act
    charges_with_prices = join_charge_prices_with_charges_on_given_charge_type(charges, charge_prices, charge_type)

    # Assert
    assert charges_with_prices.count() == expected


charges_with_prices_dataset_1 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 2, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_2 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2021, 1, 2, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_3 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 2, 0, 0), datetime(2020, 2, 15, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_4 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 3, 1, 0, 0), Decimal("200.50"))]


@pytest.mark.parametrize("charges_with_prices,expected", [
    (charges_with_prices_dataset_1, 31),
    (charges_with_prices_dataset_2, 0),
    (charges_with_prices_dataset_3, 2),
    (charges_with_prices_dataset_4, 0)
])
def test__charges_with_prices__explode_subscription(spark, charges_with_prices, expected):
    # Arrange
    charges_with_prices = spark.createDataFrame(charges_with_prices, schema=charges_with_prices_schema)

    # Act
    charges_with_prices = explode_subscription(charges_with_prices)

    # Assert
    assert charges_with_prices.count() == expected


charges_with_prices_dataset_1 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 15, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_2 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2021, 2, 1, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_3 = [("chargea-D01-001", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 1, 0, 0), Decimal("200.50"))]
charges_with_prices_dataset_4 = [("chargea-D01-002", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0), datetime(2020, 1, 15, 0, 0), Decimal("200.50"))]
charge_links_dataset = [("chargea-D01-001", "D01", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges_with_prices,charge_links,expected", [
    (charges_with_prices_dataset_1, charge_links_dataset, 1),
    (charges_with_prices_dataset_2, charge_links_dataset, 0),
    (charges_with_prices_dataset_3, charge_links_dataset, 1),
    (charges_with_prices_dataset_4, charge_links_dataset, 0)
])
def test__charges_with_price_and_links__join_charge_links_with_charges_with_prices(spark, charges_with_prices, charge_links, expected):
    # Arrange
    charges_with_prices = spark.createDataFrame(charges_with_prices, schema=charges_with_prices_schema)
    charge_links = spark.createDataFrame(charge_links, schema=charge_links_schema)

    # Act
    charges_with_price_and_links = join_charge_links_with_charges_with_prices(charges_with_prices, charge_links)

    # Assert
    assert charges_with_price_and_links.count() == expected


charges_with_price_and_links_dataset_1 = [("chargea-D01-001", "D01", "chargea", "D01", "001", Decimal("200.50"), datetime(2020, 1, 15, 0, 0))]
charges_with_price_and_links_dataset_2 = [("chargea-D01-001", "D01", "chargea", "D01", "001", Decimal("200.50"), datetime(2020, 2, 1, 0, 0))]
charges_with_price_and_links_dataset_3 = [("chargea-D01-001", "D01", "chargea", "D01", "001", Decimal("200.50"), datetime(2020, 1, 1, 0, 0))]
charges_with_price_and_links_dataset_4 = [("chargea-D01-001", "D02", "chargea", "D01", "001", Decimal("200.50"), datetime(2020, 1, 15, 0, 0))]
metering_points_dataset = [("D01", "E17", "D01", "1", "1", "P1D", "2", "1", "1", "1", "1", "1", "1", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges_with_price_and_links,metering_points,expected", [
    (charges_with_price_and_links_dataset_1, metering_points_dataset, 1),
    (charges_with_price_and_links_dataset_2, metering_points_dataset, 0),
    (charges_with_price_and_links_dataset_3, metering_points_dataset, 1),
    (charges_with_price_and_links_dataset_4, metering_points_dataset, 0)
])
def test__charges_with_metering_point__join_metering_point_with_charges_with_prices_and_links(spark, charges_with_price_and_links, metering_points, expected):
    # Arrange
    charges_with_price_and_links = spark.createDataFrame(charges_with_price_and_links, schema=charges_with_price_and_links_schema)
    metering_points = spark.createDataFrame(metering_points, schema=metering_point_schema)

    # Act
    charges_with_metering_point = join_metering_point_with_charges_with_prices_and_links(charges_with_price_and_links, metering_points)

    # Assert
    assert charges_with_metering_point.count() == expected


charges_with_metering_point_dataset_1 = [("chargea-D01-001", "D01", "chargea", "D01", "001", datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "1", "1", "1", "1")]
charges_with_metering_point_dataset_2 = [("chargea-D01-001", "D01", "chargea", "D01", "001", datetime(2020, 2, 1, 0, 0), Decimal("200.50"), "1", "1", "1", "1")]
charges_with_metering_point_dataset_3 = [("chargea-D01-001", "D01", "chargea", "D01", "001", datetime(2020, 1, 1, 0, 0), Decimal("200.50"), "1", "1", "1", "1")]
charges_with_metering_point_dataset_4 = [("chargea-D01-001", "D02", "chargea", "D01", "001", datetime(2020, 1, 15, 0, 0), Decimal("200.50"), "1", "1", "1", "1")]
market_roles_dataset = [("1", "D01", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges_with_metering_point,market_roles,expected", [
    (charges_with_metering_point_dataset_1, market_roles_dataset, 1),
    (charges_with_metering_point_dataset_2, market_roles_dataset, 0),
    (charges_with_metering_point_dataset_3, market_roles_dataset, 1),
    (charges_with_metering_point_dataset_4, market_roles_dataset, 0)
])
def test__charges_with_metering_point_and_energy_supplier__join_energy_supplier_with_charges(spark, charges_with_metering_point, market_roles, expected):
    # Arrange
    charges_with_metering_point = spark.createDataFrame(charges_with_metering_point, schema=charges_with_metering_point_schema)
    market_roles = spark.createDataFrame(market_roles, schema=market_roles_schema)

    # Act
    charges_with_metering_point_and_energy_supplier = join_energy_supplier_with_charges(charges_with_metering_point, market_roles)

    # Assert
    assert charges_with_metering_point_and_energy_supplier.count() == expected
