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
from geh_stream.aggregation_utils.aggregators import combine_added_system_correction_with_master_data, combine_added_grid_loss_with_master_data
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, BooleanType
import pytest
import pandas as pd


@pytest.fixture(scope="module")
def added_system_correction_result_schema():
    """
    Input system correction result schema
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("added_system_correction", DecimalType()) \
        .add("time_window", StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False)


@pytest.fixture(scope="module")
def added_system_correction_result_factory(spark, added_system_correction_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": ["500", "500"],
            "added_system_correction": [Decimal(6.0), Decimal(6.0)],
            "time_window": [
                {"start": datetime(2019, 1, 1, 0, 0), "end": datetime(2019, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
        })

        return spark.createDataFrame(pandas_df, schema=added_system_correction_result_schema)
    return factory


@pytest.fixture(scope="module")
def added_grid_loss_result_schema():
    """
    Input grid loss result schema
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("added_grid_loss", DecimalType()) \
        .add("time_window", StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False)


@pytest.fixture(scope="module")
def added_grid_loss_result_factory(spark, added_grid_loss_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": ["500", "500"],
            "added_grid_loss": [Decimal(6.0), Decimal(6.0)],
            "time_window": [
                {"start": datetime(2019, 1, 1, 0, 0), "end": datetime(2019, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
        })

        return spark.createDataFrame(pandas_df, schema=added_grid_loss_result_schema)
    return factory


@pytest.fixture(scope="module")
def grid_loss_sys_cor_master_data_result_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add("MarketEvaluationPoint_mRID", StringType()) \
        .add("ValidFrom", TimestampType()) \
        .add("ValidTo", TimestampType()) \
        .add("MeterReadingPeriodicity", StringType()) \
        .add("MeteringMethod", StringType()) \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("ConnectionState", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("InMeteringGridArea_Domain_mRID", StringType()) \
        .add("OutMeteringGridArea_Domain_mRID", StringType()) \
        .add("MarketEvaluationPointType", StringType()) \
        .add("SettlementMethod", StringType()) \
        .add("IsGridLoss", BooleanType()) \
        .add("IsSystemCorrection", BooleanType())


@pytest.fixture(scope="module")
def grid_loss_sys_cor_master_data_result_factory(spark, grid_loss_sys_cor_master_data_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MarketEvaluationPoint_mRID": ["578710000000000000", "578710000000000000"],
            "ValidFrom": [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            "ValidTo": [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            "MeterReadingPeriodicity": ["PT1H", "PT1H"],
            "MeteringMethod": ["D03", "D03"],
            "MeteringGridArea_Domain_mRID": ["500", "500"],
            "ConnectionState": ["E22", "E22"],
            "EnergySupplier_MarketParticipant_mRID": ["8100000000115", "8100000000115"],
            "BalanceResponsibleParty_MarketParticipant_mRID": ["8100000000214", "8100000000214"],
            "InMeteringGridArea_Domain_mRID": ["null", "null"],
            "OutMeteringGridArea_Domain_mRID": ["null", "null"],
            "MarketEvaluationPointType": ["E17", "E17"],
            "SettlementMethod": ["D01", "D01"],
            "IsGridLoss": [True, False],
            "IsSystemCorrection": [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_master_data_result_schema)
    return factory


@pytest.fixture(scope="module")
def expected_combined_data_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add("MarketEvaluationPoint_mRID", StringType()) \
        .add("ValidFrom", TimestampType()) \
        .add("ValidTo", TimestampType()) \
        .add("MeterReadingPeriodicity", StringType()) \
        .add("MeteringMethod", StringType()) \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("ConnectionState", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("InMeteringGridArea_Domain_mRID", StringType()) \
        .add("OutMeteringGridArea_Domain_mRID", StringType()) \
        .add("MarketEvaluationPointType", StringType()) \
        .add("SettlementMethod", StringType()) \
        .add("IsGridLoss", BooleanType()) \
        .add("IsSystemCorrection", BooleanType())


@pytest.fixture(scope="module")
def expected_combined_data_factory(spark, expected_combined_data_schema):
    def factory():
        pandas_df = pd.DataFrame({
            "MarketEvaluationPoint_mRID": ["578710000000000000", "578710000000000000"],
            "ValidFrom": [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            "ValidTo": [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            "MeterReadingPeriodicity": ["PT1H", "PT1H"],
            "MeteringMethod": ["D03", "D03"],
            "MeteringGridArea_Domain_mRID": ["500", "500"],
            "ConnectionState": ["E22", "E22"],
            "EnergySupplier_MarketParticipant_mRID": ["8100000000115", "8100000000115"],
            "BalanceResponsibleParty_MarketParticipant_mRID": ["8100000000214", "8100000000214"],
            "InMeteringGridArea_Domain_mRID": ["null", "null"],
            "OutMeteringGridArea_Domain_mRID": ["null", "null"],
            "MarketEvaluationPointType": ["E17", "E17"],
            "SettlementMethod": ["D01", "D01"],
            "IsGridLoss": [True, False],
            "IsSystemCorrection": [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=expected_combined_data_schema)
    return factory


def test_combine_added_system_correction_with_master_data(grid_loss_sys_cor_master_data_result_factory, added_system_correction_result_factory):
    grid_loss_sys_cor_master_data_result_factory = grid_loss_sys_cor_master_data_result_factory()
    added_system_correction_result_factory = added_system_correction_result_factory()

    result = combine_added_system_correction_with_master_data(added_system_correction_result_factory, grid_loss_sys_cor_master_data_result_factory)
    print(result.show())
    assert result.collect()[0]["ConnectionState"] == "E22"

def test_combine_added_grid_loss_with_master_data(grid_loss_sys_cor_master_data_result_factory, added_grid_loss_result_factory):
    grid_loss_sys_cor_master_data_result_factory = grid_loss_sys_cor_master_data_result_factory()
    added_system_correction_result_factory = added_grid_loss_result_factory()

    result = combine_added_grid_loss_with_master_data(added_system_correction_result_factory, grid_loss_sys_cor_master_data_result_factory)

    assert result.collect()[0]["ConnectionState"] == "E22"
