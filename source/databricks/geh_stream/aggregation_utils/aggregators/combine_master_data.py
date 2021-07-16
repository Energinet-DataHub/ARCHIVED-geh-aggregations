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
from geh_stream.codelists import Names
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when


metering_grid_area_domain_mrid_drop = "MeteringGridArea_Domain_mRID_drop"


def combine_added_system_correction_with_master_data(added_system_correction_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame):
    return combine_master_data(added_system_correction_df, grid_loss_sys_cor_master_data_df, Names.added_system_correction.value, Names.is_system_correction.value)


def combine_added_grid_loss_with_master_data(added_grid_loss_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame):
    return combine_master_data(added_grid_loss_df, grid_loss_sys_cor_master_data_df, Names.added_grid_loss.value, Names.is_grid_loss.value)


def combine_master_data(timeseries_df: DataFrame, grid_loss_sys_cor_master_data_df: DataFrame, quantity_column_name, mp_check):
    df = timeseries_df.withColumnRenamed(quantity_column_name, Names.quantity.value)
    mddf = grid_loss_sys_cor_master_data_df.withColumnRenamed(Names.grid_area.value, metering_grid_area_domain_mrid_drop)
    return df.join(
        mddf,
        when(
            col(Names.to_date.value).isNotNull(),
            col("{0}.start".format(Names.time_window.value)) <= col(Names.to_date.value),
        ).otherwise(True)
        & (col("{0}.start".format(Names.time_window.value)) >= col(Names.from_date.value))
        & (
            col(Names.to_date.value).isNull()
            | (col("{0}.end".format(Names.time_window.value)) <= col(Names.to_date.value))
        )
        & (
            col(Names.grid_area.value)
            == col(metering_grid_area_domain_mrid_drop)
        )
        & (col(mp_check))
    ).drop(metering_grid_area_domain_mrid_drop)
