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


def combine_master_data(timeseries_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame):
    df = timeseries_df.alias("tsdf").join(
        grid_loss_sys_cor_master_data_df.alias("mddf"),
        when(col("mddf.ValidTo").isNotNull(), col("tsdf.time_window.start") <= col("mddf.ValidTo")).otherwise(True)
        & (col("tsdf.time_window.start") >= col("mddf.ValidFrom"))
        & (col("mddf.ValidTo").isNull() | (col("tsdf.time_window.end") <= col("mddf.ValidTo")))
        & (col("mddf.MeteringGridArea_Domain_mRID") == col("tsdf.MeteringGridArea_Domain_mRID"))
        & (col("mddf.IsSystemCorrection")),
        "left")

    print(df.show(1000, False))
