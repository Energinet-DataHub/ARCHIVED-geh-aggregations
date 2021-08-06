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
from .aggregate_quality import aggregate_total_consumption_quality

grid_area = "MeteringGridArea_Domain_mRID"
time_window = "time_window"


# Function used to calculate grid loss (step 6)
def calculate_grid_loss(agg_net_exchange: DataFrame, agg_hourly_consumption: DataFrame, agg_flex_consumption: DataFrame, agg_production: DataFrame):
    agg_net_exchange_result = agg_net_exchange.selectExpr(grid_area, "sum_quantity as net_exchange_result", "time_window")
    agg_hourly_consumption_result = agg_hourly_consumption \
        .selectExpr(grid_area, "sum_quantity as hourly_result", "time_window") \
        .groupBy(grid_area, "time_window") \
        .sum("hourly_result") \
        .withColumnRenamed("sum(hourly_result)", "hourly_result")
    agg_flex_consumption_result = agg_flex_consumption \
        .selectExpr(grid_area, "sum_quantity as flex_result", "time_window") \
        .groupBy(grid_area, "time_window") \
        .sum("flex_result") \
        .withColumnRenamed("sum(flex_result)", "flex_result")
    agg_production_result = agg_production \
        .selectExpr(grid_area, "sum_quantity as prod_result", "time_window") \
        .groupBy(grid_area, "time_window") \
        .sum("prod_result") \
        .withColumnRenamed("sum(prod_result)", "prod_result")

    result = agg_net_exchange_result \
        .join(agg_production_result, [grid_area, time_window], "left") \
        .join(agg_hourly_consumption_result.join(agg_flex_consumption_result, [grid_area, time_window], "left"), [grid_area, time_window], "left") \
        .orderBy(grid_area, time_window) \
        .na.fill(value=0)
    result = result\
        .withColumn("grid_loss", result.net_exchange_result + result.prod_result - (result.hourly_result + result.flex_result))
    # Quality is always calculated for grid loss entries
    return result.select(grid_area, time_window, "grid_loss")


# Function to calculate system correction to be added (step 8)
def calculate_added_system_correction(df: DataFrame):
    result = df.withColumn("added_system_correction", when(col("grid_loss") < 0, (col("grid_loss")) * (-1)).otherwise(0))
    return result.select(grid_area, time_window, "added_system_correction")


# Function to calculate grid loss to be added (step 9)
def calculate_added_grid_loss(df: DataFrame):
    result = df.withColumn("added_grid_loss", when(col("grid_loss") > 0, col("grid_loss")).otherwise(0))
    return result.select(grid_area, time_window, "added_grid_loss")


# Function to calculate total consumption (step 21)
def calculate_total_consumption(agg_net_exchange: DataFrame, agg_production: DataFrame):
    grid_area = "MeteringGridArea_Domain_mRID"

    result_production = agg_production.selectExpr(grid_area, "time_window", "sum_quantity", "aggregated_quality") \
        .groupBy(grid_area, "time_window", "aggregated_quality").sum("sum_quantity") \
        .withColumnRenamed("sum(sum_quantity)", "production_sum_quantity") \
        .withColumnRenamed("aggregated_quality", "aggregated_production_quality")

    result_net_exchange = agg_net_exchange.selectExpr(grid_area, "time_window", "sum_quantity", "aggregated_quality") \
        .groupBy(grid_area, "time_window", "aggregated_quality").sum("sum_quantity") \
        .withColumnRenamed("sum(sum_quantity)", "exchange_sum_quantity") \
        .withColumnRenamed("aggregated_quality", "aggregated_net_exchange_quality")

    result = result_production.join(result_net_exchange, [grid_area, "time_window"]) \
        .withColumn("sum_quantity", col("production_sum_quantity") + col("exchange_sum_quantity"))

    result = aggregate_total_consumption_quality(result).orderBy(grid_area, "time_window")

    result = result.select(grid_area, "time_window", "aggregated_quality", "sum_quantity")
    return result
