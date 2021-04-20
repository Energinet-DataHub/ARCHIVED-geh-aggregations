# # Copyright 2020 Energinet DataHub A/S
# #
# # Licensed under the Apache License, Version 2.0 (the "License2");
# # you may not use this file except in compliance with the License.
# # You may obtain a copy of the License at
# #
# #     http://www.apache.org/licenses/LICENSE-2.0
# #
# # Unless required by applicable law or agreed to in writing, software
# # distributed under the License is distributed on an "AS IS" BASIS,
# # WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# # See the License for the specific language governing permissions and
# # limitations under the License.
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when


def combine_added_system_correction_with_master_data(added_system_correction_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame):
    return combine_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df, "added_system_correction", "IsSystemCorrection")


def combine_added_grid_loss_with_master_data(added_grid_loss_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame):
    return combine_master_data(added_grid_loss_df, grid_loss_sys_cor_master_data_df, "added_grid_loss", "IsGridLoss")


def combine_master_data(timeseries_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame, quantity_column_name, mp_check):
    df = timeseries_df.withColumnRenamed(quantity_column_name, "Quantity")
    mddf = grid_loss_sys_cor_master_data_df.withColumnRenamed("MeteringGridArea_Domain_mRID", "MeteringGridArea_Domain_mRID_drop")
    return df.join(
        mddf,
        when(
            col("ValidTo").isNotNull(),
            col("time_window.start") <= col("ValidTo"),
        ).otherwise(True)
        & (col("time_window.start") >= col("ValidFrom"))
        & (
            col("ValidTo").isNull()
            | (col("time_window.end") <= col("ValidTo"))
        )
        & (
            col("MeteringGridArea_Domain_mRID")
            == col("MeteringGridArea_Domain_mRID_drop")
        )
        & (col(mp_check))
    ).drop("MeteringGridArea_Domain_mRID_drop")
