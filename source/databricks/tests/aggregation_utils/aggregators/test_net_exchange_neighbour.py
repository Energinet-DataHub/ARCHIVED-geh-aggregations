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
import pytest
from decimal import Decimal
import pandas as pd
from datetime import datetime, timedelta
from geh_stream.aggregation_utils.aggregators import aggregate_net_exchange_per_ga
from geh_stream.codelists import MarketEvaluationPointType, ConnectionState
from pyspark.sql import DataFrame
import pyspark.sql.functions as F
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType


e_20 = MarketEvaluationPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000", date_time_formatting_string)
numberOfTestHours = 24

# Time series schema


@pytest.fixture(scope="module")
def time_series_schema():
    return StructType() \
        .add("MarketEvaluationPointType", StringType(), False) \
        .add("InMeteringGridArea_Domain_mRID", StringType()) \
        .add("OutMeteringGridArea_Domain_mRID", StringType(), False) \
        .add("Quantity", DecimalType(38, 10)) \
        .add("Time", TimestampType()) \
        .add("ConnectionState", StringType()) \


@pytest.fixture(scope="module")
def test_data_time_series_df(spark, time_series_schema):
    """
    Sample Time Series DataFrame
    """
    pandas_df = pd.DataFrame({
        "MarketEvaluationPointType": [],
        "InMeteringGridArea_Domain_mRID": [],
        "OutMeteringGridArea_Domain_mRID": [],
        "Quantity": [],
        "Time": [],
        "ConnectionState": []
    })

    for x in range(numberOfTestHours):
        pandas_df = pandas_df.append({
            "MarketEvaluationPointType": e_20,
            "InMeteringGridArea_Domain_mRID": "B",
            "OutMeteringGridArea_Domain_mRID": "A",
            "Quantity": Decimal("0.5") * x,
            "Time": default_obs_time + timedelta(hours=x),
            "ConnectionState": ConnectionState.connected.value
        }, ignore_index=True)
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


def test_aggregate_net_exchange_per_neighbour_ga(time_series_data_frame):
    """ Check sample data row count"""
    assert time_series_data_frame.count() == (9 * numberOfTestHours)
