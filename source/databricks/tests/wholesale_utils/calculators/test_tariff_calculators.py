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
from pyspark.sql.types import DecimalType, StructType, StructField, StringType, TimestampType
from geh_stream.schemas import charges_schema
from geh_stream.codelists import Colname, MarketEvaluationPointType, SettlementMethod, ResolutionDuration
from geh_stream.wholesale_utils.wholesale_initializer import filter_on_resolution_and_charge_type_tariff
from calendar import monthrange
import pytest
import pandas as pd


charges_dataset_1 = [("001-D01-001", "001", "D03", "001", "P1D", "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]
charges_dataset_2 = [("001-D01-001", "001", "D03", "001", "PT1H", "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]
charges_dataset_3 = [("001-D01-001", "001", "D01", "001", "P1D", "No", "DDK", datetime(2020, 1, 1, 0, 0), datetime(2020, 2, 1, 0, 0))]


@pytest.mark.parametrize("charges,resolution_duration,expected", [
    (charges_dataset_1, ResolutionDuration.day, 1),  # test charge with resolution P1D (day)
    (charges_dataset_1, ResolutionDuration.hour, 0),  # test charge with resolution P1D (day)
    (charges_dataset_2, ResolutionDuration.day, 0),  # test charge with resolution PT1H (hour)
    (charges_dataset_2, ResolutionDuration.hour, 1),  # test charge with resolution PT1H (hour)
    (charges_dataset_3, ResolutionDuration.day, 0)  # test non tariff charge type
])
def test__filter_on_resolution_and_charge_type_tariff__filters_on_given_resolution_and_charge_type_of_tariff(spark, charges, resolution_duration, expected):
    # Arrange
    charges = spark.createDataFrame(charges, schema=charges_schema)  # subscription_charges and charges_flex_settled_consumption has the same schema

    # Act
    result = filter_on_resolution_and_charge_type_tariff(charges, resolution_duration)

    # Assert
    assert result.count() == expected
