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
from pyspark.sql.types import StructType, StringType, DecimalType, TimestampType, BooleanType
import pytest

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
def grid_loss_sys_cor_master_data_result_schema():
    """
    Input grid loss system correction master result schema
    """
    return StructType() \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("MarketEvaluationPoint_mRID", StringType()) \
        .add("ValidFrom", TimestampType()) \
        .add("ValidTo", TimestampType()) \
        .add("MeterReadingPeriodicity", StringType()) \
        .add("MeteringMethod", StringType()) \
        .add("ConnectionState", StringType()) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType()) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType()) \
        .add("InMeteringGridArea_Domain_mRID", StringType()) \
        .add("OutMeteringGridArea_Domain_mRID", StringType()) \
        .add("MarketEvaluationPointType", StringType()) \
        .add("SettlementMethod", StringType()) \
        .add("IsGridLoss", BooleanType()) \
        .add("IsSystemCorrection", BooleanType())
