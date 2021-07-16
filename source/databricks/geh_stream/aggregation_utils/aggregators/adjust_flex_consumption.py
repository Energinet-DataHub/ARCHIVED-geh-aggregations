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

grid_loss_sys_cor_energy_supplier = "GridLossSysCor_EnergySupplier"
grid_loss_sys_cor_grid_area = "GridLossSysCor_GridArea"
adjusted_sum_quantity = "adjusted_sum_quantity"


# step 10
def adjust_flex_consumption(flex_consumption_result_df: DataFrame, added_grid_loss_result_df: DataFrame, grid_loss_sys_cor_df: DataFrame):
    # select columns from dataframe that contains information about metering points registered as GridLoss or SystemCorrection to use in join.
    glsc_df = grid_loss_sys_cor_df.selectExpr(
        Names.from_date.value,
        Names.to_date.value,
        "{0} as {1}".format(Names.energy_supplier_id.value, grid_loss_sys_cor_energy_supplier),
        "{0} as {1}".format(Names.grid_area.value, grid_loss_sys_cor_grid_area),
        Names.is_grid_loss.value
    )

    # join result dataframes from previous steps on time window and grid area.
    df = flex_consumption_result_df.join(
        added_grid_loss_result_df, [Names.time_window.value, Names.grid_area.value], "inner")
    # join information from grid loss dataframe on to joined result dataframe with information about which energy supplier,
    # that is responsible for grid loss in the given time window from the joined result dataframe.
    df = df.join(
        glsc_df,
        when(col(Names.to_date.value).isNotNull(), col("{0}.start".format(Names.time_window.value)) <= col(Names.to_date.value)).otherwise(True)
        & (col("{0}.start".format(Names.time_window.value)) >= col(Names.from_date.value))
        & (col(Names.to_date.value).isNull() | (col("{0}.end".format(Names.time_window.value)) <= col(Names.to_date.value)))
        & (col(Names.grid_area.value) == col(grid_loss_sys_cor_grid_area))
        & (col(Names.is_grid_loss.value)),
        "left")
    # update function that selects the sum of two columns if condition is met, or selects data from a single column if condition is not met.
    update_func = (when(col(Names.energy_supplier_id.value) == col(grid_loss_sys_cor_energy_supplier),
                        col(Names.sum_quantity.value) + col(Names.added_grid_loss.value))
                   .otherwise(col(Names.sum_quantity.value)))

    result_df = df.withColumn(adjusted_sum_quantity, update_func) \
        .drop(Names.sum_quantity.value) \
        .withColumnRenamed(adjusted_sum_quantity, Names.sum_quantity.value)
    return result_df.select(
        Names.grid_area.value,
        Names.balance_responsible_id.value,
        Names.energy_supplier_id.value,
        Names.time_window.value,
        Names.sum_quantity.value,
        Names.aggregated_quality.value) \
        .orderBy(
            Names.grid_area.value,
            Names.balance_responsible_id.value,
            Names.energy_supplier_id.value,
            Names.time_window.value)
