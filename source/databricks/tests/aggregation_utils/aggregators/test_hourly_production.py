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
from geh_stream.codelists import Names
from geh_stream.aggregation_utils.aggregators import aggregate_per_ga, aggregate_per_ga_and_brp, aggregate_per_ga_and_es
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType
import pytest
import pandas as pd
from geh_stream.codelists import Quality

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_obs_time = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)


@pytest.fixture(scope="module")
def agg_production_schema():
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.balance_responsible_id.value, StringType()) \
        .add(Names.energy_supplier_id.value, StringType()) \
        .add(Names.time_window.value,
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add(Names.sum_quantity.value, DecimalType(20)) \
        .add(Names.aggregated_quality.value, StringType())


@pytest.fixture(scope="module")
def test_data_factory(spark, agg_production_schema):

    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: [],
            Names.balance_responsible_id.value: [],
            Names.energy_supplier_id.value: [],
            Names.time_window.value: [],
            Names.sum_quantity.value: [],
            Names.aggregated_quality.value: []
        })
        for i in range(3):
            for j in range(5):
                for k in range(10):
                    pandas_df = pandas_df.append({
                        Names.grid_area.value: str(i),
                        Names.balance_responsible_id.value: str(j),
                        Names.energy_supplier_id.value: str(k),
                        Names.time_window.value: {
                            "start": default_obs_time + timedelta(hours=i),
                            "end": default_obs_time + timedelta(hours=i + 1)},
                        Names.sum_quantity.value: Decimal(i + j + k),
                        Names.aggregated_quality.value: [Quality.estimated.value]
                    }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=agg_production_schema)
    return factory


def test_production_calculation_per_ga_and_es(test_data_factory):
    agg_production = test_data_factory()
    result = aggregate_per_ga_and_es(agg_production).sort(Names.grid_area.value, Names.energy_supplier_id.value)

    assert len(result.columns) == 5
    assert result.collect()[0][Names.grid_area.value] == "0"
    assert result.collect()[9][Names.energy_supplier_id.value] == "9"
    assert result.collect()[10][Names.sum_quantity.value] == Decimal("15")
    assert result.collect()[29][Names.grid_area.value] == "2"
    assert result.collect()[29][Names.energy_supplier_id.value] == "9"
    assert result.collect()[29][Names.sum_quantity.value] == Decimal("65")


def test_production_calculation_per_ga_and_brp(test_data_factory):
    agg_production = test_data_factory()
    result = aggregate_per_ga_and_brp(agg_production).sort(Names.grid_area.value, Names.balance_responsible_id.value)

    assert len(result.columns) == 5
    assert result.collect()[0][Names.sum_quantity.value] == Decimal("45")
    assert result.collect()[4][Names.grid_area.value] == "0"
    assert result.collect()[5][Names.balance_responsible_id.value] == "0"
    assert result.collect()[14][Names.grid_area.value] == "2"
    assert result.collect()[14][Names.balance_responsible_id.value] == "4"
    assert result.collect()[14][Names.sum_quantity.value] == Decimal("105")


def test_production_calculation_per_ga(test_data_factory):
    agg_production = test_data_factory()
    result = aggregate_per_ga(agg_production).sort(Names.grid_area.value)

    assert len(result.columns) == 4
    assert result.collect()[0][Names.grid_area.value] == "0"
    assert result.collect()[1][Names.sum_quantity.value] == Decimal("375")
    assert result.collect()[2][Names.grid_area.value] == "2"
    assert result.collect()[2][Names.sum_quantity.value] == Decimal("425")
