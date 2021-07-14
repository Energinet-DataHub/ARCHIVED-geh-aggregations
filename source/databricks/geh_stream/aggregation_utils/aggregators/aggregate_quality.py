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

from geh_stream.codelists import Names
from pyspark.sql import DataFrame
from pyspark.sql.functions import col, when, window, count, year, month, dayofmonth, hour
from geh_stream.codelists import Quality


grid_area = "MeteringGridArea_Domain_mRID"
quality = "Quality"
mp = "MarketEvaluationPointType"
time_window = "time_window"
aggregated_quality = "aggregated_quality"
temp_estimated_quality_count = "temp_estimated_quality_count"
temp_quantity_missing_quality_count = "temp_quantity_missing_quality_count"

aggregated_production_quality = "aggregated_production_quality"
aggregated_net_exchange_quality = "aggregated_net_exchange_quality"


def aggregate_quality(time_series_df: DataFrame):
    agg_df = time_series_df.groupBy(Names.grid_area.value, Names.metering_point_type.value, window(Names.time.value, "1 hour")) \
        .agg(
            # Count entries where quality is estimated (Quality=56)
            count(when(col(Names.quality.value) == Quality.estimated.value, 1)).alias(temp_estimated_quality_count),
            # Count entries where quality is quantity missing (Quality=QM)
            count(when(col(Names.quality.value) == Quality.quantity_missing.value, 1)).alias(temp_quantity_missing_quality_count)
            ) \
        .withColumn(
                    Names.aggregated_quality.value,
                    (
                    # Set quality to as read (Quality=E01) if no entries where quality is estimated or quantity missing
                    when(col(temp_estimated_quality_count) > 0, Quality.estimated.value)
                    .when(col(temp_quantity_missing_quality_count) > 0, Quality.estimated.value)
                    .otherwise(Quality.as_read.value)
                    )
                    ) \
        .drop(temp_estimated_quality_count) \
        .drop(temp_quantity_missing_quality_count) \
        .withColumn(Names.time.value, col("window").start) \
        .withColumnRenamed("window", Names.time_window.value)

    joined_df = time_series_df \
        .join(agg_df,
              (year(time_series_df[Names.time.value]) == year(agg_df[Names.time.value]))
              & (month(time_series_df[Names.time.value]) == month(agg_df[Names.time.value]))
              & (dayofmonth(time_series_df[Names.time.value]) == dayofmonth(agg_df[Names.time.value]))
              & (hour(time_series_df[Names.time.value]) == hour(agg_df[Names.time.value]))
              & (time_series_df[Names.metering_point_type.value] == agg_df[Names.metering_point_type.value])
              & (time_series_df[Names.grid_area.value] == agg_df[Names.grid_area.value])) \
        .select(time_series_df["*"], agg_df[Names.aggregated_quality.value])
    return joined_df


def aggregate_total_consumption_quality(df: DataFrame):
    df = df.groupBy(grid_area, time_window, "sum_quantity") \
        .agg(
            # Count entries where quality is estimated (Quality=56)
            count(
                when(col(aggregated_production_quality) == Quality.estimated.value, 1) \
                .when(col(aggregated_net_exchange_quality) == Quality.estimated.value, 1)) \
            .alias(temp_estimated_quality_count),
            # Count entries where quality is quantity missing (Quality=QM)
            count(
                when(col(aggregated_production_quality) == Quality.quantity_missing.value, 1) \
                .when(col(aggregated_net_exchange_quality) == Quality.quantity_missing.value, 1)) \
            .alias(temp_quantity_missing_quality_count)
            ) \
        .withColumn(
                    aggregated_quality,
                    (
                        # Set quality to as read (Quality=E01) if no entries where quality is estimated or quantity missing
                        when(col(temp_estimated_quality_count) > 0, Quality.estimated.value)
                        .when(col(temp_quantity_missing_quality_count) > 0, Quality.estimated.value)
                        .otherwise(Quality.as_read.value)
                    )
                   )
    return df
