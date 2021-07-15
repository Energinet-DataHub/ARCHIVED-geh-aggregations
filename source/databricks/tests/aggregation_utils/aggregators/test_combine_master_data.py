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
from geh_stream.codelists import Names
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
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.added_system_correction.value, DecimalType()) \
        .add(Names.time_window.value, StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False)


@pytest.fixture(scope="module")
def added_system_correction_result_factory(spark, added_system_correction_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: ["500", "500"],
            Names.added_system_correction.value: [Decimal(6.0), Decimal(6.0)],
            Names.time_window.value: [
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
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.added_grid_loss.value, DecimalType()) \
        .add(Names.time_window.value, StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False)


@pytest.fixture(scope="module")
def added_grid_loss_result_factory(spark, added_grid_loss_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: ["500", "500"],
            Names.added_grid_loss.value: [Decimal(6.0), Decimal(6.0)],
            Names.time_window.value: [
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
        .add(Names.metering_point_id.value, StringType()) \
        .add(Names.from_date.value, TimestampType()) \
        .add(Names.to_date.value, TimestampType()) \
        .add(Names.resolution.value, StringType()) \
        .add(Names.metering_method.value, StringType()) \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.connection_state.value, StringType()) \
        .add(Names.energy_supplier_id.value, StringType()) \
        .add(Names.balance_responsible_id.value, StringType()) \
        .add(Names.in_grid_area.value, StringType()) \
        .add(Names.out_grid_area.value, StringType()) \
        .add(Names.metering_point_type.value, StringType()) \
        .add(Names.settlement_method.value, StringType()) \
        .add(Names.is_grid_loss.value, BooleanType()) \
        .add(Names.is_system_correction.value, BooleanType())


@pytest.fixture(scope="module")
def grid_loss_sys_cor_master_data_result_factory(spark, grid_loss_sys_cor_master_data_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.metering_point_id.value: ["578710000000000000", "578710000000000000"],
            Names.from_date.value: [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            Names.to_date.value: [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            Names.resolution.value: ["PT1H", "PT1H"],
            Names.metering_method.value: ["D03", "D03"],
            Names.grid_area.value: ["500", "500"],
            Names.connection_state.value: ["E22", "E22"],
            Names.energy_supplier_id.value: ["8100000000115", "8100000000115"],
            Names.balance_responsible_id.value: ["8100000000214", "8100000000214"],
            Names.in_grid_area.value: ["null", "null"],
            Names.out_grid_area.value: ["null", "null"],
            Names.metering_point_type.value: ["E17", "E17"],
            Names.settlement_method.value: ["D01", "D01"],
            Names.is_grid_loss.value: [True, False],
            Names.is_system_correction.value: [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_master_data_result_schema)
    return factory


@pytest.fixture(scope="module")
def expected_combined_data_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add(Names.grid_area.value, StringType(), False) \
        .add(Names.quantity.value, DecimalType()) \
        .add(Names.time_window.value, StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add(Names.metering_point_id.value, StringType()) \
        .add(Names.from_date.value, TimestampType()) \
        .add(Names.to_date.value, TimestampType()) \
        .add(Names.resolution.value, StringType()) \
        .add(Names.metering_method.value, StringType()) \
        .add(Names.connection_state.value, StringType()) \
        .add(Names.energy_supplier_id.value, StringType()) \
        .add(Names.balance_responsible_id.value, StringType()) \
        .add(Names.in_grid_area.value, StringType()) \
        .add(Names.out_grid_area.value, StringType()) \
        .add(Names.metering_point_type.value, StringType()) \
        .add(Names.settlement_method.value, StringType()) \
        .add(Names.is_grid_loss.value, BooleanType()) \
        .add(Names.is_system_correction.value, BooleanType())


@pytest.fixture(scope="module")
def expected_combined_data_factory(spark, expected_combined_data_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Names.grid_area.value: ["500", "500"],
            Names.added_grid_loss.value: [Decimal(6.0), Decimal(6.0)],
            Names.time_window.value: [
                {"start": datetime(2019, 1, 1, 0, 0), "end": datetime(2019, 1, 1, 1, 0)},
                {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
            ],
            Names.metering_point_id.value: ["578710000000000000", "578710000000000000"],
            Names.from_date.value: [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            Names.to_date.value: [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            Names.resolution.value: ["PT1H", "PT1H"],
            Names.metering_method.value: ["D03", "D03"],
            Names.connection_state.value: ["E22", "E22"],
            Names.energy_supplier_id.value: ["8100000000115", "8100000000115"],
            Names.balance_responsible_id.value: ["8100000000214", "8100000000214"],
            Names.in_grid_area.value: ["null", "null"],
            Names.out_grid_area.value: ["null", "null"],
            Names.metering_point_type.value: ["E17", "E17"],
            Names.settlement_method.value: ["D01", "D01"],
            Names.is_grid_loss.value: [True, False],
            Names.is_system_correction.value: [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=expected_combined_data_schema)
    return factory


def test_combine_added_system_correction_with_master_data(grid_loss_sys_cor_master_data_result_factory, added_system_correction_result_factory, expected_combined_data_factory):
    grid_loss_sys_cor_master_data_result_factory = grid_loss_sys_cor_master_data_result_factory()
    added_system_correction_result_factory = added_system_correction_result_factory()
    expected_combined_data_factory = expected_combined_data_factory()

    result = combine_added_system_correction_with_master_data(added_system_correction_result_factory, grid_loss_sys_cor_master_data_result_factory)

    # expected data for combine_added_grid_loss_with_master_data is at index 1 in expected_combined_data_factory
    assert result.collect()[0] == expected_combined_data_factory.collect()[1]


def test_combine_added_grid_loss_with_master_data(grid_loss_sys_cor_master_data_result_factory, added_grid_loss_result_factory, expected_combined_data_factory):
    grid_loss_sys_cor_master_data_result_factory = grid_loss_sys_cor_master_data_result_factory()
    added_grid_loss_result_factory = added_grid_loss_result_factory()
    expected_combined_data_factory = expected_combined_data_factory()

    result = combine_added_grid_loss_with_master_data(added_grid_loss_result_factory, grid_loss_sys_cor_master_data_result_factory)

    # expected data for combine_added_grid_loss_with_master_data is at index 0 in expected_combined_data_factory
    assert result.collect()[0] == expected_combined_data_factory.collect()[0]
