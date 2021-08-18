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
from geh_stream.codelists import Quality, ResolutionDuration
from pyspark.sql import dataframe
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, StructField
from pyspark.sql.functions import col, to_date
import pytest
import pandas as pd


def test_charges(charges_factory):
    df = charges_factory(from_date=datetime(2020, 1, 1, 0, 0), to_date=datetime(2020, 1, 1, 0, 0))
    df.show()
    assert True


def test_charge_links(charge_links_factory):
    df = charge_links_factory(from_date=datetime(2020, 1, 1, 0, 0), to_date=datetime(2020, 1, 1, 0, 0))
    df.show()
    assert True


def test_charge_prices(charge_prices_factory):
    df = charge_prices_factory(time=datetime(2020, 1, 1, 0, 0))
    df.show()
    assert True
