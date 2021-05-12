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
from geh_stream.aggregation_utils.aggregators import adjust_production
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
default_added_system_correction = Decimal(3)
default_aggregated_quality = Quality.estimated.value

date_time_formatting_string = "%Y-%m-%dT%H:%M:%S%z"
default_time_window = {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
default_valid_from = datetime.strptime("2020-01-01T00:00:00+0000", date_time_formatting_string)
default_valid_to = datetime.strptime("2020-01-01T01:00:00+0000", date_time_formatting_string)


@pytest.fixture(scope="module")
def hourly_production_result_schema():
    """
    Input hourly production result data frame schema
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
             False) \
        .add("aggregated_quality", StringType())


@pytest.fixture(scope="module")
def sys_cor_schema():
    """
    Input system correction data frame schema
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("ValidFrom", TimestampType()) \
        .add("ValidTo", TimestampType()) \
        .add("IsSystemCorrection", BooleanType()) \
        .add("aggregated_quality", StringType())


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
def hourly_production_result_row_factory(spark, hourly_production_result_schema):
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
        return spark.createDataFrame(pandas_df, schema=hourly_production_result_schema)
    return factory


@pytest.fixture(scope="module")
def added_system_correction_result_row_factory(spark, added_system_correction_result_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                added_system_correction=default_added_system_correction,
                time_window=default_time_window,
                sys_cor_aggregated_quality=default_aggregated_quality):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [domain],
            "added_system_correction": [added_system_correction],
            "time_window": [time_window],
            "aggregated_quality": [sys_cor_aggregated_quality]})
        return spark.createDataFrame(pandas_df, schema=added_system_correction_result_schema)
    return factory


@pytest.fixture(scope="module")
def sys_cor_row_factory(spark, sys_cor_schema):
    """
    Factory to generate a single row of  data, with default parameters as specified above.
    """
    def factory(domain=default_domain,
                responsible=default_responsible,
                supplier=default_supplier,
                valid_from=default_valid_from,
                valid_to=default_valid_to,
                is_system_correction=True,
                aggregated_quality=default_aggregated_quality):
        pandas_df = pd.DataFrame({
            "MeteringGridArea_Domain_mRID": [domain],
            "BalanceResponsibleParty_MarketParticipant_mRID": [responsible],
            "EnergySupplier_MarketParticipant_mRID": [supplier],
            "ValidFrom": [valid_from],
            "ValidTo": [valid_to],
            "IsSystemCorrection": [is_system_correction],
            "aggregated_quality": [aggregated_quality]})
        return spark.createDataFrame(pandas_df, schema=sys_cor_schema)
    return factory


def test_grid_area_system_correction_is_added_to_system_correction_energy_responsible(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):

    hp_df = hourly_production_result_row_factory(supplier="A")

    gasc_df = added_system_correction_result_row_factory()

    sc_df = sys_cor_row_factory(supplier="A")

    result_df = adjust_production(hp_df, gasc_df, sc_df)

    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "A").collect()[0].sum_quantity == default_added_system_correction + default_sum_quantity


def test_grid_area_grid_loss_is_not_added_to_non_grid_loss_energy_responsible(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):

    hp_df = hourly_production_result_row_factory(supplier="A")

    gasc_df = added_system_correction_result_row_factory()

    sc_df = sys_cor_row_factory(supplier="B")

    result_df = adjust_production(hp_df, gasc_df, sc_df)

    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "A").collect()[0].sum_quantity == default_sum_quantity


def test_result_dataframe_contains_same_number_of_results_with_same_energy_suppliers_as_flex_consumption_result_dataframe(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):

    hp_row_1 = hourly_production_result_row_factory(supplier="A")
    hp_row_2 = hourly_production_result_row_factory(supplier="B")
    hp_row_3 = hourly_production_result_row_factory(supplier="C")

    hp_df = hp_row_1.union(hp_row_2).union(hp_row_3)

    gasc_df = added_system_correction_result_row_factory()

    sc_df = sys_cor_row_factory(supplier="C")

    result_df = adjust_production(hp_df, gasc_df, sc_df)

    assert result_df.count() == 3
    assert result_df.collect()[0].EnergySupplier_MarketParticipant_mRID == "A"
    assert result_df.collect()[1].EnergySupplier_MarketParticipant_mRID == "B"
    assert result_df.collect()[2].EnergySupplier_MarketParticipant_mRID == "C"


def test_correct_system_correction_entry_is_used_to_determine_energy_responsible_for_the_given_time_window_from_hourly_production_result_dataframe(
        hourly_production_result_row_factory,
        added_system_correction_result_row_factory,
        sys_cor_row_factory):

    time_window_1 = {"start": datetime(2020, 1, 1, 0, 0), "end": datetime(2020, 1, 1, 1, 0)}
    time_window_2 = {"start": datetime(2020, 1, 1, 1, 0), "end": datetime(2020, 1, 1, 2, 0)}
    time_window_3 = {"start": datetime(2020, 1, 1, 2, 0), "end": datetime(2020, 1, 1, 3, 0)}

    hp_row_1 = hourly_production_result_row_factory(supplier="A", time_window=time_window_1)
    hp_row_2 = hourly_production_result_row_factory(supplier="B", time_window=time_window_2)
    hp_row_3 = hourly_production_result_row_factory(supplier="B", time_window=time_window_3)

    hp_df = hp_row_1.union(hp_row_2).union(hp_row_3)

    gasc_result_1 = Decimal(1)
    gasc_result_2 = Decimal(2)
    gasc_result_3 = Decimal(3)

    gasc_row_1 = added_system_correction_result_row_factory(time_window=time_window_1, added_system_correction=gasc_result_1)
    gasc_row_2 = added_system_correction_result_row_factory(time_window=time_window_2, added_system_correction=gasc_result_2)
    gasc_row_3 = added_system_correction_result_row_factory(time_window=time_window_3, added_system_correction=gasc_result_3)

    gasc_df = gasc_row_1.union(gasc_row_2).union(gasc_row_3)

    sc_row_1 = sys_cor_row_factory(supplier="A", valid_from=time_window_1["start"], valid_to=time_window_1["end"])
    sc_row_2 = sys_cor_row_factory(supplier="C", valid_from=time_window_2["start"], valid_to=time_window_2["end"])
    sc_row_3 = sys_cor_row_factory(supplier="B", valid_from=time_window_3["start"], valid_to=None)

    sc_df = sc_row_1.union(sc_row_2).union(sc_row_3)

    result_df = adjust_production(hp_df, gasc_df, sc_df)

    assert result_df.count() == 3
    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "A").filter(col("time_window.start") == time_window_1["start"]).collect()[0].sum_quantity == default_sum_quantity + gasc_result_1
    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "B").filter(col("time_window.start") == time_window_2["start"]).collect()[0].sum_quantity == default_sum_quantity
    assert result_df.filter(col("EnergySupplier_MarketParticipant_mRID") == "B").filter(col("time_window.start") == time_window_3["start"]).collect()[0].sum_quantity == default_sum_quantity + gasc_result_3
