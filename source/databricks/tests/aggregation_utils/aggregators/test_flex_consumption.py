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
from datetime import datetime, timedelta
from geh_stream.codelists import Colname
from geh_stream.aggregation_utils.aggregators import aggregate_per_ga_and_es, aggregate_per_ga_and_brp, aggregate_per_ga
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd
from geh_stream.codelists import Quality

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)


@pytest.fixture(scope="module")
def agg_flex_consumption_schema():
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.time_window,
             StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.sum_quantity, DecimalType(20)) \
        .add(Colname.aggregated_quality, StringType())


@pytest.fixture(scope="module")
def test_data_factory(spark, agg_flex_consumption_schema):

    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: [],
            Colname.balance_responsible_id: [],
            Colname.energy_supplier_id: [],
            Colname.time_window: [],
            Colname.sum_quantity: [],
        })
        for i in range(3):
            for j in range(5):
                for k in range(10):
                    pandas_df = pandas_df.append({
                        Colname.grid_area: str(i),
                        Colname.balance_responsible_id: str(j),
                        Colname.energy_supplier_id: str(k),
                        Colname.time_window: {
                            Colname.start: default_obs_time + timedelta(hours=i),
                            Colname.end: default_obs_time + timedelta(hours=i + 1)},
                        Colname.sum_quantity: Decimal(i + j + k),
                        Colname.aggregated_quality: Quality.estimated.value
                    }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=agg_flex_consumption_schema)
    return factory


def test_flex_consumption_calculation_per_ga_and_es(test_data_factory):
    agg_flex_consumption = test_data_factory()
    result = aggregate_per_ga_and_es(agg_flex_consumption).sort(Colname.grid_area, Colname.energy_supplier_id, Colname.time_window)
    assert len(result.columns) == 5
    assert result.collect()[0][Colname.grid_area] == "0"
    assert result.collect()[9][Colname.energy_supplier_id] == "9"
    assert result.collect()[10][Colname.sum_quantity] == Decimal("15")
    assert result.collect()[29][Colname.grid_area] == "2"
    assert result.collect()[29][Colname.energy_supplier_id] == "9"
    assert result.collect()[29][Colname.sum_quantity] == Decimal("65")


def test_flex_consumption_calculation_per_ga_and_brp(test_data_factory):
    agg_flex_consumption = test_data_factory()
    result = aggregate_per_ga_and_brp(agg_flex_consumption).sort(Colname.grid_area, Colname.balance_responsible_id, Colname.time_window)
    assert len(result.columns) == 5
    assert result.collect()[0][Colname.sum_quantity] == Decimal("45")
    assert result.collect()[4][Colname.grid_area] == "0"
    assert result.collect()[5][Colname.balance_responsible_id] == "0"
    assert result.collect()[14][Colname.grid_area] == "2"
    assert result.collect()[14][Colname.balance_responsible_id] == "4"
    assert result.collect()[14][Colname.sum_quantity] == Decimal("105")


def test_flex_consumption_calculation_per_ga(test_data_factory):
    agg_flex_consumption = test_data_factory()
    result = aggregate_per_ga(agg_flex_consumption).sort(Colname.grid_area, Colname.time_window)
    assert len(result.columns) == 4
    assert result.collect()[0][Colname.grid_area] == "0"
    assert result.collect()[1][Colname.sum_quantity] == Decimal("375")
    assert result.collect()[2][Colname.grid_area] == "2"
    assert result.collect()[2][Colname.sum_quantity] == Decimal("425")
