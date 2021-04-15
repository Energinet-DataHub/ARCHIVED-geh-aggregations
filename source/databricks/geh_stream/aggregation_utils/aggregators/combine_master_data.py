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
    df = added_system_correction_df.alias("ascdf").join(
        grid_loss_sys_cor_master_data_df.alias("mddf"),
        when(col("mddf.ValidTo").isNotNull(), col("ascdf.time_window.start") <= col("mddf.ValidTo")).otherwise(True)
        & (col("ascdf.time_window.start") >= col("mddf.ValidFrom"))
        & (col("mddf.ValidTo").isNull() | (col("ascdf.time_window.end") <= col("mddf.ValidTo")))
        & (col("mddf.MeteringGridArea_Domain_mRID") == col("ascdf.MeteringGridArea_Domain_mRID"))
        & (col("mddf.IsSystemCorrection")),
        "left")

    return df


def combine_added_grid_loss_with_master_data(added_grid_loss_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame):
    df = added_grid_loss_df.alias("agldf").join(
        grid_loss_sys_cor_master_data_df.alias("mddf"),
        when(col("mddf.ValidTo").isNotNull(), col("agldf.time_window.start") <= col("mddf.ValidTo")).otherwise(True)
        & (col("agldf.time_window.start") >= col("mddf.ValidFrom"))
        & (col("mddf.ValidTo").isNull() | (col("agldf.time_window.end") <= col("mddf.ValidTo")))
        & (col("mddf.MeteringGridArea_Domain_mRID") == col("agldf.MeteringGridArea_Domain_mRID"))
        & (col("mddf.IsGridLoss")),
        "left")

    return df
