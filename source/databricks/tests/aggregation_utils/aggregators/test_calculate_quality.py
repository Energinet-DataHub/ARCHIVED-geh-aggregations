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
from datetime import datetime, timedelta
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
from geh_stream.codelists import Quality, MarketEvaluationPointType
from geh_stream.aggregation_utils.aggregators import calculate_quality
import pytest
import pandas as pd

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)

qualities = ["E01", "56", "D01", "QM"]


@pytest.fixture(scope="module")
def schema():
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("MarketEvaluationPointType", StringType()) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("Quality", StringType())


@pytest.fixture(scope="module")
def test_data_factory(spark, schema):
    """
    Factory to generate .....
    """
    def factory(quality_1,
                quality_2,
                quality_3=Quality.estimated.value):
        df_qualities = [quality_1, quality_2, quality_3]
        print(df_qualities[0])
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [],
            "MarketEvaluationPointType": [],
            "time_window": [],
            "Quality": [],
        })
        for i in range(3):
            pandas_df = pandas_df.append({
                "MeteringGridArea_Domain_mRID": str(1),
                "MarketEvaluationPointType": MarketEvaluationPointType.consumption.value,
                "time_window": {
                    "start": default_obs_time,
                    "end": default_obs_time + timedelta(hours=1)},
                "Quality": df_qualities[i]
            }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=schema)
    return factory


def test_set_calculated_quality_to_estimated_when_quality_within_hour_is_estimated_and_read(test_data_factory):
    df = test_data_factory(Quality.estimated.value, Quality.as_read.value)

    result_df = calculate_quality(df)

    assert(result_df.collect()[0].calculated_quality == Quality.estimated.value)


# def test_set_calculated_quality_to_estimated_when_quality_within_hour_is_estimated_and_calculated():


# def test_set_calculated_quality_to_estimated_when_quality_within_hour_is_estimated_calculated_and_read():

    
# def test_set_calculated_quality_to_estimated_when_quality_within_hour_is_estimated_and_quantity_missing():


# def test_set_calculated_quality_to_estimated_when_quality_within_hour_is_read_and_quantity_missing():

    
# def test_set_calculated_quality_to_estimated_when_quality_within_hour_is_calculated_and_quantity_missing():


# def test_set_calculated_quality_to_read_when_quality_within_hour_is_either_read_or_calculated():

