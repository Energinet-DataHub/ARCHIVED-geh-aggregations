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
from pyspark.sql.functions import window, max, greatest
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, FloatType


e_20 = MarketEvaluationPointType.exchange.value
date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime(
    "2020-01-01T00:00:00+0000",
    date_time_formatting_string)
numberOfTestHours = 24

df_template = {
    'MeteringGridArea_Domain_mRID': [],
    'MarketEvaluationPointType': [],
    'InMeteringGridArea_Domain_mRID': [],
    'OutMeteringGridArea_Domain_mRID': [],
    'Quantity': [],
    'Time': [],
    'ConnectionState': [],
    'Quality': []
}


@pytest.fixture(scope='module')
def expected_schema():
    return StructType() \
        .add('InMeteringGridArea_Domain_mRID', StringType()) \
        .add('OutMeteringGridArea_Domain_mRID', StringType()) \
        .add('time_window', StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add('aggregated_quality', StringType(), False) \
        .add('sum_quantity', DecimalType(38))


@pytest.fixture(scope='module')
def time_series_schema():
    return StructType() \
        .add('MeteringGridArea_Domain_mRID', StringType()) \
        .add('MarketEvaluationPointType', StringType()) \
        .add('InMeteringGridArea_Domain_mRID', StringType()) \
        .add('OutMeteringGridArea_Domain_mRID', StringType()) \
        .add('Quantity', DecimalType(38)) \
        .add('Time', TimestampType()) \
        .add('ConnectionState', StringType()) \
        .add('Quality', StringType())


@pytest.fixture(scope='module')
def time_series_with_quality_int_value_schema():
    return StructType() \
        .add('MeteringGridArea_Domain_mRID', StringType()) \
        .add('MarketEvaluationPointType', StringType()) \
        .add('InMeteringGridArea_Domain_mRID', StringType()) \
        .add('OutMeteringGridArea_Domain_mRID', StringType()) \
        .add('Quantity', DecimalType(38)) \
        .add('Time', TimestampType()) \
        .add('ConnectionState', StringType()) \
        .add('Quality', StringType()) \
        .add('Quality_int', FloatType())

@pytest.fixture(scope='module')
def single_hour_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('10'), Quality.as_read.value)
    pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('15'), Quality.as_read.value)
    pandas_df = add_row_of_data(pandas_df, 'A', 'B', 'A', default_obs_time, Decimal('5'), Quality.as_read.value)
    pandas_df = add_row_of_data(pandas_df, 'B', 'B', 'A', default_obs_time, Decimal('10'), Quality.as_read.value)
    pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'C', default_obs_time, Decimal('20'), Quality.as_read.value)
    pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('10'), Quality.as_read.value)
    pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('5'), Quality.as_read.value)
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


@pytest.fixture(scope='module')
def multi_hour_test_data(spark, time_series_schema):
    pandas_df = pd.DataFrame(df_template)
    for i in range(numberOfTestHours):
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time + timedelta(hours=i), Decimal('10'), Quality.as_read.value)
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time + timedelta(hours=i), Decimal('15'), Quality.as_read.value)
        pandas_df = add_row_of_data(pandas_df, 'A', 'B', 'A', default_obs_time + timedelta(hours=i), Decimal('5'), Quality.as_read.value)
        pandas_df = add_row_of_data(pandas_df, 'B', 'B', 'A', default_obs_time + timedelta(hours=i), Decimal('10'), Quality.as_read.value)
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'C', default_obs_time + timedelta(hours=i), Decimal('20'), Quality.as_read.value)
        pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time + timedelta(hours=i), Decimal('10'), Quality.as_read.value)
        pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time + timedelta(hours=i), Decimal('5'), Quality.as_read.value)
    return spark.createDataFrame(pandas_df, schema=time_series_schema)


@pytest.fixture(scope='module')
def single_hour_quality_test_data(spark, time_series_schema):
    def factory(quality):
        pandas_df = pd.DataFrame(df_template)
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('10'), quality)
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('15'), quality)
        pandas_df = add_row_of_data(pandas_df, 'A', 'B', 'A', default_obs_time, Decimal('5'), quality)
        pandas_df = add_row_of_data(pandas_df, 'B', 'B', 'A', default_obs_time, Decimal('10'), quality)
        pandas_df = add_row_of_data(pandas_df, 'A', 'A', 'C', default_obs_time, Decimal('20'), quality)
        pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('10'), quality)
        pandas_df = add_row_of_data(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('5'), quality)
        return spark.createDataFrame(pandas_df, schema=time_series_schema)
    return factory


@pytest.fixture(scope='module')
def single_hour_quality_int_value_data(spark, time_series_with_quality_int_value_schema):
    def factory():
        pandas_df = pd.DataFrame(df_template)
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('10'), Quality.as_read.value, float(1))
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'A', 'A', 'B', default_obs_time, Decimal('15'), Quality.estimated.value, float(2))
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'A', 'B', 'A', default_obs_time, Decimal('5'), Quality.as_read.value, float(1))
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'B', 'B', 'A', default_obs_time, Decimal('10'), Quality.estimated.value, float(2))
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'A', 'A', 'C', default_obs_time, Decimal('20'), Quality.as_read.value, float(1))
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('10'), Quality.quantity_missing.value, float(3))
        pandas_df = add_row_of_data_with_quality_int(pandas_df, 'C', 'C', 'A', default_obs_time, Decimal('5'), Quality.as_read.value, float(1))
        return spark.createDataFrame(pandas_df, schema=time_series_with_quality_int_value_schema)
    return factory


def add_row_of_data_with_quality_int(pandas_df, domain, in_domain, out_domain, timestamp, quantity, quality, quality_int):
    new_row = {'MeteringGridArea_Domain_mRID': domain,
               'MarketEvaluationPointType': e_20,
               'InMeteringGridArea_Domain_mRID': in_domain,
               'OutMeteringGridArea_Domain_mRID': out_domain,
               'Quantity': quantity,
               'Time': timestamp,
               'ConnectionState': ConnectionState.connected.value,
               'Quality': quality,
               'Quality_int': quality_int}
    return pandas_df.append(new_row, ignore_index=True)


def add_row_of_data(pandas_df, domain, in_domain, out_domain, timestamp, quantity, quality):
    new_row = {'MeteringGridArea_Domain_mRID': domain,
               'MarketEvaluationPointType': e_20,
               'InMeteringGridArea_Domain_mRID': in_domain,
               'OutMeteringGridArea_Domain_mRID': out_domain,
               'Quantity': quantity,
               'Time': timestamp,
               'ConnectionState': ConnectionState.connected.value,
               'Quality': quality}
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
    assert values[0][4] == Decimal('10')
    assert values[1][4] == Decimal('5')
    assert values[2][4] == Decimal('-10')
    assert values[3][4] == Decimal('-5')


def test_aggregate_net_exchange_per_neighbour_ga_multi_hour(multi_hour_test_data):
    df = aggregate_net_exchange_per_neighbour_ga(multi_hour_test_data).orderBy(
        "InMeteringGridArea_Domain_mRID",
        "OutMeteringGridArea_Domain_mRID",
        "time_window")
    values = df.collect()
    assert df.count() == 96
    assert values[0][0] == 'A'
    assert values[0][1] == 'B'
    assert values[0][2][0].strftime(date_time_formatting_string) == '2020-01-01T00:00:00'
    assert values[0][2][1].strftime(date_time_formatting_string) == '2020-01-01T01:00:00'
    assert values[0][4] == Decimal('10')
    assert values[19][0] == 'A'
    assert values[19][1] == 'B'
    assert values[19][2][0].strftime(date_time_formatting_string) == '2020-01-01T19:00:00'
    assert values[19][2][1].strftime(date_time_formatting_string) == '2020-01-01T20:00:00'
    assert values[19][4] == Decimal('10')


def test_expected_schema(single_hour_test_data, expected_schema):
    df = aggregate_net_exchange_per_neighbour_ga(single_hour_test_data).orderBy(
        "InMeteringGridArea_Domain_mRID",
        "OutMeteringGridArea_Domain_mRID",
        "time_window")
    assert df.schema == expected_schema


@pytest.mark.parametrize("quality",[ \
    (Quality.estimated.value), \
    (Quality.quantity_missing.value), \
    (Quality.as_read.value) \
    ])
def test_aggregated_quality(single_hour_quality_test_data, quality):
    df = single_hour_quality_test_data(quality)
    result_df = aggregate_net_exchange_per_neighbour_ga(df).orderBy("InMeteringGridArea_Domain_mRID", "OutMeteringGridArea_Domain_mRID")
    values = result_df.collect()

    estimated_quality = Quality.estimated.value

    if quality is Quality.as_read.value:
        estimated_quality = Quality.as_read.value

    assert values[0]["aggregated_quality"] == estimated_quality
    assert values[1]["aggregated_quality"] == estimated_quality
    assert values[2]["aggregated_quality"] == estimated_quality


def test_max_aggregation_on_quality(single_hour_quality_int_value_data):
    df = single_hour_quality_int_value_data()

    result_df_1 = df.groupBy(window("Time", "1 hour")).max("Quality_int")
    assert result_df_1.collect()[0]["max(Quality_int)"] == float(3)

    result_df_2 = df.groupBy('InMeteringGridArea_Domain_mRID', window("Time", "1 hour")).max("Quality_int").orderBy('InMeteringGridArea_Domain_mRID')
    assert result_df_2.collect()[0]["max(Quality_int)"] == float(2)
    assert result_df_2.collect()[1]["max(Quality_int)"] == float(2)
    assert result_df_2.collect()[2]["max(Quality_int)"] == float(3)

    result_df_3 = df.groupBy('OutMeteringGridArea_Domain_mRID', window("Time", "1 hour")).max("Quality_int").orderBy('OutMeteringGridArea_Domain_mRID')
    assert result_df_3.collect()[0]["max(Quality_int)"] == float(3)
    assert result_df_3.collect()[1]["max(Quality_int)"] == float(2)
    assert result_df_3.collect()[2]["max(Quality_int)"] == float(1)

    result_df_2 = result_df_2.withColumnRenamed("max(Quality_int)", "in_max_quality")
    result_df_3 = result_df_3.withColumnRenamed("max(Quality_int)", "out_max_quality")

    result_df = result_df_2.join(result_df_3, "window").withColumn("aggregated_quality", greatest("in_max_quality", "out_max_quality"))

    print(result_df.show())