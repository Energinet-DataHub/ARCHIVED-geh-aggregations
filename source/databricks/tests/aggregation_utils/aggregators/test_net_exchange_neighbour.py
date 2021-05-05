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
from os import truncate
import pytest
from decimal import Decimal
import pandas as pd
from datetime import datetime, timedelta
from geh_stream.aggregation_utils.aggregators import aggregate_net_exchange_per_neighbour_ga
from geh_stream.codelists import MarketEvaluationPointType, ConnectionState, Quality
from pyspark.sql import DataFrame
import pyspark.sql.functions as F
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType


e_20 = MarketEvaluationPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000",
    date_time_formatting_string)
numberOfTestHours = 24
estimated_quality = Quality.estimated.value

df_template = {
    'MeteringGridArea_Domain_mRID': [],
    'MarketEvaluationPointType': [],
    'InMeteringGridArea_Domain_mRID': [],
    'OutMeteringGridArea_Domain_mRID': [],
    'Quantity': [],
    'Time': [],
    'ConnectionState': [],
    'aggregated_quality': []
}


@pytest.fixture(scope='module')
def time_series_schema():
    return StructType() \
        .add('MeteringGridArea_Domain_mRID', StringType(), False) \
        .add('MarketEvaluationPointType', StringType(), False) \
        .add('InMeteringGridArea_Domain_mRID', StringType()) \
        .add('OutMeteringGridArea_Domain_mRID', StringType(), False) \
        .add('Quantity', DecimalType(38, 10)) \
        .add('Time', TimestampType()) \
        .add('ConnectionState', StringType()) \
        .add('aggregated_quality', StringType())


@pytest.fixture(scope='module')
def single_hour_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('10'))
    pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('15'))
    pandas_df = add_row_of_data(pandas_df, 'A', 'B', 'A', default_obs_time, Decimal('5'))
    pandas_df = add_row_of_data(pandas_df, 'B', 'B', 'A', default_obs_time, Decimal('10'))
    pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'C', default_obs_time, Decimal('20'))
    pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('10'))
    pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('5'))
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


@pytest.fixture(scope='module')
def multi_hour_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    for i in range(numberOfTestHours):
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time + timedelta(hours=i), Decimal('10'))
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time + timedelta(hours=i), Decimal('15'))
        pandas_df = add_row_of_data(pandas_df, 'A', 'B', 'A', default_obs_time + timedelta(hours=i), Decimal('5'))
        pandas_df = add_row_of_data(pandas_df, 'B', 'B', 'A', default_obs_time + timedelta(hours=i), Decimal('10'))
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'C', default_obs_time + timedelta(hours=i), Decimal('20'))
        pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time + timedelta(hours=i), Decimal('10'))
        pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time + timedelta(hours=i), Decimal('5'))
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


def add_row_of_data(pandas_df, domain, in_domain, out_domain, timestamp, quantity):
    new_row = {'MeteringGridArea_Domain_mRID': domain,
               'MarketEvaluationPointType': e_20,
               'InMeteringGridArea_Domain_mRID': in_domain,
               'OutMeteringGridArea_Domain_mRID': out_domain,
               'Quantity': quantity,
               'Time': timestamp,
               'ConnectionState': ConnectionState.connected.value,
               'aggregated_quality': estimated_quality}
    return pandas_df.append(new_row, ignore_index=True)


def test_aggregate_net_exchange_per_neighbour_ga_single_hour(single_hour_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(single_hour_test_data).orderBy(
        "InMeteringGridArea_Domain_mRID",
        "OutMeteringGridArea_Domain_mRID",
        "time_window")
    values = df.collect()
    assert df.count() == 4
    assert values[0][0] == 'A'
    assert values[1][1] == 'C'
    assert values[2][0] == 'B'
    assert values[0][3] == Decimal('-10')
    assert values[1][3] == Decimal('-5')
    assert values[2][3] == Decimal('10')
    assert values[3][3] == Decimal('5')
    validate_exchange_result(values)


def test_aggregate_net_exchange_per_neighbour_ga_multi_hour(multi_hour_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(multi_hour_test_data).orderBy(
        "InMeteringGridArea_Domain_mRID",
        "OutMeteringGridArea_Domain_mRID",
        "time_window")
    print(df.show())
    values = df.collect()
    assert df.count() == 96
    assert values[0][0] == 'A'
    assert values[0][1] == 'B'
    assert values[0][2][0].strftime(date_time_formatting_string) == '2020-01-01T00:00:00'
    assert values[0][2][1].strftime(date_time_formatting_string) == '2020-01-01T01:00:00'
    assert values[0][3] == Decimal('-10')
    assert values[19][0] == 'A'
    assert values[19][1] == 'B'
    assert values[19][2][0].strftime(date_time_formatting_string) == '2020-01-01T19:00:00'
    assert values[19][2][1].strftime(date_time_formatting_string) == '2020-01-01T20:00:00'
    assert values[19][3] == Decimal('-10')
    validate_exchange_result(values)


def validate_exchange_result(values):
    print(values)
    for i in range(len(values[0])):
        for j in range(len(values[1])):
            if (values[i][0] == values[j][1]) & (values[i][1] == values[j][0]):
                assert values[i][3] + values[j][3] == 0
