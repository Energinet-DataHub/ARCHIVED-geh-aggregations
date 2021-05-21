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
from geh_stream.aggregation_utils.aggregators import adjust_flex_consumption
from geh_stream.codelists import Quality
from pyspark.sql.functions import col
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, BooleanType
import pytest
import pandas as pd

# Default values
default_domain = "D1"
default_responsible = "R1"
default_supplier = "S1"
default_sum_quantity = Decimal(1)
default_added_grid_loss = Decimal(3)
default_aggregated_quality = Quality.estimated.value

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_time_window = {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
default_valid_from = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)
default_valid_to = datetime.strptime("2020-01-01T01:00:00+0000", date_time_formatting_string)


@pytest.fixture(scope="module")
def flex_consumption_result_schema():
    """
    Input flex consumption result data frame schema
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("sum_quantity", DecimalType()) \
        .add("time_window", StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("aggregated_quality", StringType())


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
def grid_loss_sys_cor_schema():
    """
    Input grid loss system correction data frame schema
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("ValidFrom", TimestampType()) \
        .add("ValidTo", TimestampType()) \
        .add("IsGridLoss", BooleanType())


@pytest.fixture(scope="module")
def expected_schema():
    """
    Expected aggregation schema
    NOTE: Spark seems to add 10 to the precision of the decimal type on summations.
    Thus, the expected schema should be precision of 20, 10 more than the default of 10.
    If this is an issue we can always cast back to the original decimal precision in the aggregate
    function.
    https://stackoverflow.com/questions/57203383/spark-sum-and-decimaltype-precision
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("time_window",
             StructType()
             .add("start", TimestampType())
             .add("end", TimestampType()),
             False) \
        .add("sum_quantity", DecimalType()) \
        .add("aggregated_quality", StringType())


@pytest.fixture(scope="module")
def flex_consumption_result_row_factory(spark, flex_consumption_result_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                responsible=default_responsible,
                supplier=default_supplier,
                sum_quantity=default_sum_quantity,
                time_window=default_time_window,
                aggregated_quality=default_aggregated_quality):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [domain],
            "BalanceResponsibleParty_MarketParticipant_mRID": [responsible],
            "EnergySupplier_MarketParticipant_mRID": [supplier],
            "sum_quantity": [sum_quantity],
            "time_window": [time_window],
            "aggregated_quality": [aggregated_quality]})
        return spark.createDataFrame(pandas_df, schema=flex_consumption_result_schema)
    return factory


@pytest.fixture(scope="module")
def added_grid_loss_result_row_factory(spark, added_grid_loss_result_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                added_grid_loss=default_added_grid_loss,
                time_window=default_time_window):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [domain],
            "added_grid_loss": [added_grid_loss],
            "time_window": [time_window]})
        return spark.createDataFrame(pandas_df, schema=added_grid_loss_result_schema)
    return factory


@pytest.fixture(scope="module")
def grid_loss_sys_cor_row_factory(spark, grid_loss_sys_cor_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                responsible=default_responsible,
                supplier=default_supplier,
                valid_from=default_valid_from,
                valid_to=default_valid_to,
                is_grid_loss=True):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [domain],
            "BalanceResponsibleParty_MarketParticipant_mRID": [responsible],
            "EnergySupplier_MarketParticipant_mRID": [supplier],
            "ValidFrom": [valid_from],
            "ValidTo": [valid_to],
            "IsGridLoss": [is_grid_loss]})
        return spark.createDataFrame(pandas_df, schema=grid_loss_sys_cor_schema)
    return factory


def test_grid_area_grid_loss_is_added_to_grid_loss_energy_responsible(
        flex_consumption_result_row_factory,
        added_grid_loss_result_row_factory,
        grid_loss_sys_cor_row_factory):

    fc_df = flex_consumption_result_row_factory(supplier="A")

    gagl_df = added_grid_loss_result_row_factory()

    glsc_df = grid_loss_sys_cor_row_factory(supplier="A")

    result_df = adjust_flex_consumption(fc_df, gagl_df, glsc_df)

    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "A").collect()[0].sum_quantity == default_added_grid_loss + default_sum_quantity


def test_grid_area_grid_loss_is_not_added_to_non_grid_loss_energy_responsible(
        flex_consumption_result_row_factory,
        added_grid_loss_result_row_factory,
        grid_loss_sys_cor_row_factory):

    fc_df = flex_consumption_result_row_factory(supplier="A")

    gagl_df = added_grid_loss_result_row_factory()

    glsc_df = grid_loss_sys_cor_row_factory(supplier="B")

    result_df = adjust_flex_consumption(fc_df, gagl_df, glsc_df)

    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "A").collect()[0].sum_quantity == default_sum_quantity


def test_result_dataframe_contains_same_number_of_results_with_same_energy_suppliers_as_flex_consumption_result_dataframe(
        flex_consumption_result_row_factory,
        added_grid_loss_result_row_factory,
        grid_loss_sys_cor_row_factory):

    fc_row_1 = flex_consumption_result_row_factory(supplier="A")
    fc_row_2 = flex_consumption_result_row_factory(supplier="B")
    fc_row_3 = flex_consumption_result_row_factory(supplier="C")

    fc_df = fc_row_1.union(fc_row_2).union(fc_row_3)

    gagl_df = added_grid_loss_result_row_factory()

    glsc_df = grid_loss_sys_cor_row_factory(supplier="C")

    result_df = adjust_flex_consumption(fc_df, gagl_df, glsc_df)

    assert result_df.count() == 3
    assert result_df.collect()[0].EnergySupplier_MarketParticipant_mRID == "A"
    assert result_df.collect()[1].EnergySupplier_MarketParticipant_mRID == "B"
    assert result_df.collect()[2].EnergySupplier_MarketParticipant_mRID == "C"


def test_correct_grid_loss_entry_is_used_to_determine_energy_responsible_for_the_given_time_window_from_flex_consumption_result_dataframe(
        flex_consumption_result_row_factory,
        added_grid_loss_result_row_factory,
        grid_loss_sys_cor_row_factory):

    time_window_1 = {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
    time_window_2 = {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)}
    time_window_3 = {"start": datetime(2020, 1, 1, 2, 0), "end": datetime(2020, 1, 1, 3, 0)}

    fc_row_1 = flex_consumption_result_row_factory(supplier="A", time_window=time_window_1)
    fc_row_2 = flex_consumption_result_row_factory(supplier="B", time_window=time_window_2)
    fc_row_3 = flex_consumption_result_row_factory(supplier="B", time_window=time_window_3)

    fc_df = fc_row_1.union(fc_row_2).union(fc_row_3)

    gagl_result_1 = Decimal(1)
    gagl_result_2 = Decimal(2)
    gagl_result_3 = Decimal(3)

    gagl_row_1 = added_grid_loss_result_row_factory(time_window=time_window_1, added_grid_loss=gagl_result_1)
    gagl_row_2 = added_grid_loss_result_row_factory(time_window=time_window_2, added_grid_loss=gagl_result_2)
    gagl_row_3 = added_grid_loss_result_row_factory(time_window=time_window_3, added_grid_loss=gagl_result_3)

    gagl_df = gagl_row_1.union(gagl_row_2).union(gagl_row_3)

    glsc_row_1 = grid_loss_sys_cor_row_factory(supplier="A", valid_from=time_window_1["start"], valid_to=time_window_1["end"])
    glsc_row_2 = grid_loss_sys_cor_row_factory(supplier="C", valid_from=time_window_2["start"], valid_to=time_window_2["end"])
    glsc_row_3 = grid_loss_sys_cor_row_factory(supplier="B", valid_from=time_window_3["start"], valid_to=None)

    glsc_df = glsc_row_1.union(glsc_row_2).union(glsc_row_3)

    result_df = adjust_flex_consumption(fc_df, gagl_df, glsc_df)

    assert result_df.count() == 3
    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "A").filter(col("time_window.start") == time_window_1["start"]).collect()[0].sum_quantity == default_sum_quantity + gagl_result_1
    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "B").filter(col("time_window.start") == time_window_2["start"]).collect()[0].sum_quantity == default_sum_quantity
    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "B").filter(col("time_window.start") == time_window_3["start"]).collect()[0].sum_quantity == default_sum_quantity + gagl_result_3

    # for i in range(1, 23):
    #     time_windows.append({'start': datetime(2020, 1, 1, i, 0), 'end': datetime(2020, 1, 1, i + 1, 0)})
    #     sup = "A"
    #     if i > 4:
    #         sup = "B"
    #     if i > 8:
    #         sup = "C"
    #     if i > 12:
    #         sup = "D"
    #     if i > 16:
    #         sup = "E"
    #     if i > 20:
    #         sup = "F"
    #     fc_row = flex_consumption_result_row_factory(supplier=sup, time_window=time_windows[i])
    #     fc_df.union(fc_row)
