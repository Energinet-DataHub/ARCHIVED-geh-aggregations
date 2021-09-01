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
from geh_stream.shared.filters import filter_on_date, filter_on_period
from geh_stream.shared.period import Period
from geh_stream.codelists import Colname
from pyspark.sql.types import StructType, TimestampType
from typing import List
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def period_schema():
    return StructType() \
        .add(Colname.from_date, TimestampType(), False) \
        .add(Colname.to_date, TimestampType(), False)


@pytest.fixture(scope="module")
def timestamps_schema():
    return StructType() \
        .add(Colname.time, TimestampType(), False)


@pytest.fixture(scope="module")
def period_factory(spark, period_schema):
    def factory(period: Period):
        pandas_df = pd.DataFrame({
            Colname.from_date: [],
            Colname.to_date: [],
        })
        pandas_df = pandas_df.append([{Colname.from_date: period.from_date, Colname.to_date: period.to_date}])
        return spark.createDataFrame(pandas_df, schema=period_schema)
    return factory


@pytest.fixture(scope="module")
def periods_factory(spark, period_schema):
    def factory(periods: List[Period]):
        pandas_df = pd.DataFrame({
            Colname.from_date: [],
            Colname.to_date: [],
        })
        for i in range(len(periods)):
            pandas_df = pandas_df.append([{Colname.from_date: periods[i].from_date, Colname.to_date: periods[i].to_date}])
        return spark.createDataFrame(pandas_df, schema=period_schema)
    return factory


@pytest.fixture(scope="module")
def timestamps_factory(spark, timestamps_schema):
    def factory(timestamps: List[datetime]):
        pandas_df = pd.DataFrame({
            Colname.time: [],
        })
        for i in range(len(timestamps)):
            pandas_df = pandas_df.append([{Colname.time: timestamps[i]}])
        return spark.createDataFrame(pandas_df, schema=timestamps_schema)
    return factory


def test_filter_on_period_between(period_factory):
    period = period_factory(Period(datetime(2021, 1, 6), datetime(2021, 1, 8)))
    df = filter_on_period(period, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 1


def test_filter_on_period_overlap_from(period_factory):
    period = period_factory(Period(datetime(2021, 1, 2), datetime(2021, 1, 7)))
    df = filter_on_period(period, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 1


def test_filter_on_period_overlap_to(period_factory):
    period_df = period_factory(Period(datetime(2021, 1, 7), datetime(2021, 1, 15)))
    df = filter_on_period(period_df, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 1


def test_filter_on_period_overlap_from_and_to(period_factory):
    period = period_factory(Period(datetime(2021, 1, 3), datetime(2021, 1, 12)))
    df = filter_on_period(period, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 1


def test_filter_on_period_before(period_factory):
    period = period_factory(Period(datetime(2021, 1, 2), datetime(2021, 1, 4)))
    df = filter_on_period(period, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 0


def test_filter_on_period_after(period_factory):
    period = period_factory(Period(datetime(2021, 1, 12), datetime(2021, 1, 15)))
    df = filter_on_period(period, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 0


def test_filter_on_period_all(periods_factory):
    periods = [Period(datetime(2021, 1, 6), datetime(2021, 1, 8)),
               Period(datetime(2021, 1, 2), datetime(2021, 1, 7)),
               Period(datetime(2021, 1, 7), datetime(2021, 1, 15)),
               Period(datetime(2021, 1, 3), datetime(2021, 1, 12)),
               Period(datetime(2021, 1, 2), datetime(2021, 1, 4)),
               Period(datetime(2021, 1, 12), datetime(2021, 1, 15))]
    periods_df = periods_factory(periods)
    df = filter_on_period(periods_df, Period(datetime(2021, 1, 5), datetime(2021, 1, 10)))
    assert df.count() == 4


def test_filter_on_date(timestamps_factory):
    timestamps = [datetime(2021, 1, 1, 12, 30),
                  datetime(2021, 1, 1, 12, 31),
                  datetime(2021, 1, 1, 12, 32),
                  datetime(2021, 1, 1, 12, 33),
                  datetime(2021, 1, 1, 12, 34),
                  datetime(2021, 1, 1, 12, 35),
                  datetime(2021, 1, 1, 12, 36),
                  datetime(2021, 1, 1, 12, 37),
                  datetime(2021, 1, 1, 12, 38),
                  datetime(2021, 1, 1, 12, 39),
                  datetime(2021, 1, 1, 12, 40)]
    timestamps_df = timestamps_factory(timestamps)
    df = filter_on_date(timestamps_df, Period(datetime(2021, 1, 1, 12, 32), datetime(2021, 1, 1, 12, 38)))
    assert df.count() == 6
