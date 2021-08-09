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
from geh_stream.codelists import Colname
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
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.added_system_correction, DecimalType()) \
        .add(Colname.time_window, StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False)


@pytest.fixture(scope="module")
def added_system_correction_result_factory(spark, added_system_correction_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["500", "500"],
            Colname.added_system_correction: [Decimal(6.0), Decimal(6.0)],
            Colname.time_window: [
                {Colname.start: datetime(2019, 1, 1, 0, 0), Colname.end: datetime(2019, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
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
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.added_grid_loss, DecimalType()) \
        .add(Colname.time_window, StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False)


@pytest.fixture(scope="module")
def added_grid_loss_result_factory(spark, added_grid_loss_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["500", "500"],
            Colname.added_grid_loss: [Decimal(6.0), Decimal(6.0)],
            Colname.time_window: [
                {Colname.start: datetime(2019, 1, 1, 0, 0), Colname.end: datetime(2019, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
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
        .add(Colname.metering_point_id, StringType()) \
        .add(Colname.from_date, TimestampType()) \
        .add(Colname.to_date, TimestampType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_method, StringType()) \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.connection_state, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.in_grid_area, StringType()) \
        .add(Colname.out_grid_area, StringType()) \
        .add(Colname.metering_point_type, StringType()) \
        .add(Colname.settlement_method, StringType()) \
        .add(Colname.is_grid_loss, BooleanType()) \
        .add(Colname.is_system_correction, BooleanType())


@pytest.fixture(scope="module")
def grid_loss_sys_cor_master_data_result_factory(spark, grid_loss_sys_cor_master_data_result_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.metering_point_id: ["578710000000000000", "578710000000000000"],
            Colname.from_date: [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            Colname.to_date: [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            Colname.resolution: ["PT1H", "PT1H"],
            Colname.metering_method: ["D03", "D03"],
            Colname.grid_area: ["500", "500"],
            Colname.connection_state: ["E22", "E22"],
            Colname.energy_supplier_id: ["8100000000115", "8100000000115"],
            Colname.balance_responsible_id: ["8100000000214", "8100000000214"],
            Colname.in_grid_area: ["null", "null"],
            Colname.out_grid_area: ["null", "null"],
            Colname.metering_point_type: ["E17", "E17"],
            Colname.settlement_method: ["D01", "D01"],
            Colname.is_grid_loss: [True, False],
            Colname.is_system_correction: [False, True],
        })

        return spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_master_data_result_schema)
    return factory


@pytest.fixture(scope="module")
def expected_combined_data_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add(Colname.grid_area, StringType(), False) \
        .add(Colname.quantity, DecimalType()) \
        .add(Colname.time_window, StructType()
             .add(Colname.start, TimestampType())
             .add(Colname.end, TimestampType()),
             False) \
        .add(Colname.metering_point_id, StringType()) \
        .add(Colname.from_date, TimestampType()) \
        .add(Colname.to_date, TimestampType()) \
        .add(Colname.resolution, StringType()) \
        .add(Colname.metering_method, StringType()) \
        .add(Colname.connection_state, StringType()) \
        .add(Colname.energy_supplier_id, StringType()) \
        .add(Colname.balance_responsible_id, StringType()) \
        .add(Colname.in_grid_area, StringType()) \
        .add(Colname.out_grid_area, StringType()) \
        .add(Colname.metering_point_type, StringType()) \
        .add(Colname.settlement_method, StringType()) \
        .add(Colname.is_grid_loss, BooleanType()) \
        .add(Colname.is_system_correction, BooleanType())


@pytest.fixture(scope="module")
def expected_combined_data_factory(spark, expected_combined_data_schema):
    def factory():
        pandas_df = pd.DataFrame({
            Colname.grid_area: ["500", "500"],
            Colname.added_grid_loss: [Decimal(6.0), Decimal(6.0)],
            Colname.time_window: [
                {Colname.start: datetime(2019, 1, 1, 0, 0), Colname.end: datetime(2019, 1, 1, 1, 0)},
                {Colname.start: datetime(2020, 1, 1, 0, 0), Colname.end: datetime(2020, 1, 1, 1, 0)}
            ],
            Colname.metering_point_id: ["578710000000000000", "578710000000000000"],
            Colname.from_date: [datetime(2018, 12, 31, 23, 0), datetime(2019, 12, 31, 23, 0)],
            Colname.to_date: [datetime(2019, 12, 31, 23, 0), datetime(2020, 12, 31, 23, 0)],
            Colname.resolution: ["PT1H", "PT1H"],
            Colname.metering_method: ["D03", "D03"],
            Colname.connection_state: ["E22", "E22"],
            Colname.energy_supplier_id: ["8100000000115", "8100000000115"],
            Colname.balance_responsible_id: ["8100000000214", "8100000000214"],
            Colname.in_grid_area: ["null", "null"],
            Colname.out_grid_area: ["null", "null"],
            Colname.metering_point_type: ["E17", "E17"],
            Colname.settlement_method: ["D01", "D01"],
            Colname.is_grid_loss: [True, False],
            Colname.is_system_correction: [False, True],
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
