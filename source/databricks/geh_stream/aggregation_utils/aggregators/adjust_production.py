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
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when


# step 11
def adjust_production(hourly_production_result_df: DataFrame, added_grid_loss_result_df: DataFrame, sys_cor_df: DataFrame):

    # select columns from dataframe that contains information about metering points registered as SystemCorrection to use in join.
    sc_df = sys_cor_df.selectExpr(
        "ValidFrom",
        "ValidTo",
        "EnergySupplier_MarketParticipant_mRID as SysCor_EnergySupplier",
        "MeteringGridArea_Domain_mRID as SysCor_GridArea",
        "IsSystemCorrection"
        # "aggregated_quality as sys_cor_aggregated_quality"
    )

    # join result dataframes from previous steps on time window and grid area.
    df = hourly_production_result_df.join(
        added_grid_loss_result_df, ["time_window", "MeteringGridArea_Domain_mRID"], "inner")

    # join information from system correction dataframe on to joined result dataframe with information about which energy supplier,
    # that is responsible for system correction in the given time window from the joined result dataframe.
    df = df.join(
        sc_df,
        when(col("ValidTo").isNotNull(), col("time_window.start") <= col("ValidTo")).otherwise(True)
        & (col("time_window.start") >= col("ValidFrom"))
        & (col("ValidTo").isNull() | (col("time_window.end") <= col("ValidTo")))
        & (col("MeteringGridArea_Domain_mRID") == col("SysCor_GridArea"))
        & (col("IsSystemCorrection")),
        "left")

    # update function that selects the sum of two columns if condition is met, or selects data from a single column if condition is not met.
    update_func = (when(col("EnergySupplier_MarketParticipant_mRID") == col("SysCor_EnergySupplier"),
                        col("sum_quantity") + col("added_system_correction"))
                   .otherwise(col("sum_quantity")))
    # update function that selects quality from grid loss dataframe if condition is met
    # update_quality_func = (when(col("EnergySupplier_MarketParticipant_mRID") == col("SysCor_EnergySupplier"),
    #                             col("sys_cor_aggregated_quality"))
    #                        .otherwise(col("aggregated_quality")))

    result_df = df.withColumn("adjusted_sum_quantity", update_func) \
        .drop("sum_quantity") \
        .withColumnRenamed("adjusted_sum_quantity", "sum_quantity")
    # .withColumn("aggregated_quality", update_quality_func) \

    return result_df.select(
        "MeteringGridArea_Domain_mRID",
        "BalanceResponsibleParty_MarketParticipant_mRID",
        "EnergySupplier_MarketParticipant_mRID",
        "time_window",
        "sum_quantity") \
        .orderBy(
            "MeteringGridArea_Domain_mRID",
            "BalanceResponsibleParty_MarketParticipant_mRID",
            "EnergySupplier_MarketParticipant_mRID",
            "time_window")
