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
from pyspark.sql.functions import col, when, window, count
from geh_stream.codelists import Quality, MarketEvaluationPointType


grid_area = "MeteringGridArea_Domain_mRID"
mp = "MarketEvaluationPointType"
quality = "Quality"
aggregated_quality = "aggregated_quality"
time_window = "time_window"


def aggregate_quality(df: DataFrame):
    df = df.groupBy(grid_area, mp, time_window) \
        .agg(count(when(col(quality) == Quality.estimated.value, 1)).alias("estimated_quality_count"), count(when(col(quality) == Quality.quantity_missing.value, 1)).alias("quantity_missing_quality_count")) \
        .withColumn(aggregated_quality, (when(col("estimated_quality_count") > 0, Quality.estimated.value).when(col("quantity_missing_quality_count") > 0, Quality.estimated.value).otherwise(Quality.as_read.value))) \
        .drop("estimated_quality_count") \
        .drop("quantity_missing_quality_count")
    return df
