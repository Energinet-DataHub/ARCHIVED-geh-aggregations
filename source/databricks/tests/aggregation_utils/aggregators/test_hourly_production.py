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
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("sum_quantity", DecimalType(20)) \
        .add("aggregated_quality", StringType())


@pytest.fixture(scope="module")
def test_data_factory(spark, agg_production_schema):

    def factory():
        pandas_df = pd.DataFrame({
            'MeteringGridArea_Domain_mRID': [],
            'BalanceResponsibleParty_MarketParticipant_mRID': [],
            'EnergySupplier_MarketParticipant_mRID': [],
            'time_window': [],
            'sum_quantity': [],
            'aggregated_quality': []
        })
        for i in range(3):
            for j in range(5):
                for k in range(10):
                    pandas_df = pandas_df.append({
                        'MeteringGridArea_Domain_mRID': str(i),
                        'BalanceResponsibleParty_MarketParticipant_mRID': str(j),
                        'EnergySupplier_MarketParticipant_mRID': str(k),
                        'time_window': {
                            'start': default_obs_time + timedelta(hours=i),
                            'end': default_obs_time + timedelta(hours=i + 1)},
                        'sum_quantity': Decimal(i + j + k),
                        'aggregated_quality': [Quality.estimated.value]
                    }, ignore_index=True)
        return spark.createDataFrame(pandas_df, schema=agg_production_schema)
    return factory


def test_production_calculation_per_ga_and_es(test_data_factory):
    agg_production = test_data_factory()
    result = aggregate_per_ga_and_es(agg_production).sort('MeteringGridArea_Domain_mRID', 'EnergySupplier_MarketParticipant_mRID')

    assert len(result.columns) == 5
    assert result.collect()[0]['MeteringGridArea_Domain_mRID'] == '0'
    assert result.collect()[9]['EnergySupplier_MarketParticipant_mRID'] == '9'
    assert result.collect()[10]['sum_quantity'] == Decimal('15')
    assert result.collect()[29]['MeteringGridArea_Domain_mRID'] == '2'
    assert result.collect()[29]['EnergySupplier_MarketParticipant_mRID'] == '9'
    assert result.collect()[29]['sum_quantity'] == Decimal('65')


def test_production_calculation_per_ga_and_brp(test_data_factory):
    agg_production = test_data_factory()
    result = aggregate_per_ga_and_brp(agg_production).sort('MeteringGridArea_Domain_mRID', 'BalanceResponsibleParty_MarketParticipant_mRID')

    assert len(result.columns) == 5
    assert result.collect()[0]['sum_quantity'] == Decimal('45')
    assert result.collect()[4]['MeteringGridArea_Domain_mRID'] == '0'
    assert result.collect()[5]['BalanceResponsibleParty_MarketParticipant_mRID'] == '0'
    assert result.collect()[14]['MeteringGridArea_Domain_mRID'] == '2'
    assert result.collect()[14]['BalanceResponsibleParty_MarketParticipant_mRID'] == '4'
    assert result.collect()[14]['sum_quantity'] == Decimal('105')


def test_production_calculation_per_ga(test_data_factory):
    agg_production = test_data_factory()
    result = aggregate_per_ga(agg_production).sort('MeteringGridArea_Domain_mRID')

    assert len(result.columns) == 4
    assert result.collect()[0]['MeteringGridArea_Domain_mRID'] == '0'
    assert result.collect()[1]['sum_quantity'] == Decimal('375')
    assert result.collect()[2]['MeteringGridArea_Domain_mRID'] == '2'
    assert result.collect()[2]['sum_quantity'] == Decimal('425')
