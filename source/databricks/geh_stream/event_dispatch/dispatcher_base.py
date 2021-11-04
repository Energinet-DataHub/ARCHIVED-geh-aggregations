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
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col, when, lag
from geh_stream.codelists import Colname


def period_mutations(spark, target_dataframe: DataFrame, event_df: DataFrame, cols_to_change):

    for col_to_change in cols_to_change:
        event_df = event_df.withColumnRenamed(col_to_change, f"updated_{col_to_change}")

    event_df.show()

    joined = target_dataframe.join(event_df, [Colname.metering_point_id], "inner")

    df_periods_to_keep = joined.filter(col(Colname.to_date) < col(Colname.effective_date))
    df_periods_to_keep.show()

    df_periods_to_update = joined.filter(col(Colname.from_date) <= col(Colname.effective_date)).orderBy(col(Colname.from_date))
    df_periods_to_update.show()

    periods_to_update_count = df_periods_to_update.count()

    update_func_to_date = (when((col(Colname.from_date) <= col(Colname.effective_date)) & (col(Colname.to_date) > col(Colname.effective_date)), col(Colname.effective_date))
                           .otherwise(col(Colname.to_date)))

    if periods_to_update_count > 1:

        update_func_from_date = (when((col(Colname.from_date) > col(Colname.effective_date)) & ((lag(col(Colname.to_date), 1)) == col(Colname.effective_date)), col(Colname.effective_date))
                                 .otherwise(col(Colname.from_date)))

        df_periods_to_update = df_periods_to_update \
            .withColumn(Colname.to_date, update_func_to_date) \
            .withColumn(Colname.from_date, update_func_from_date)

    else:
        df_periods_to_update = df_periods_to_update.withColumn("old_to_date", col(Colname.to_date))
        df_periods_to_update = df_periods_to_update.withColumn(Colname.to_date, update_func_to_date)

        new_row = df_periods_to_update \
            .withColumn(Colname.from_date, col(Colname.effective_date)) \
            .withColumn(Colname.to_date, col("old_to_date"))

        df_periods_to_update = df_periods_to_update.union(new_row)

    for col_to_change in cols_to_change:
        df_periods_to_update = df_periods_to_update \
            .withColumn(col_to_change, col(f"updated_{col_to_change}"))

    df_periods_to_update = df_periods_to_update.select(target_dataframe.columns)

    result = df_periods_to_keep.select(target_dataframe.columns).union(df_periods_to_update)

    return result

    # # Merge the event data onto our existing periods
    # for col_to_change in cols_to_change:
    #     event_df = event_df.withColumnRenamed(col_to_change, f"updated_{col_to_change}")
    # joined_mps = target_dataframe.join(event_df, Colname.metering_point_id, "inner")

    # count = joined_mps.where(f"{Colname.from_date} == {Colname.effective_date}").count()

    # # if we have a count of 1 than we've matched an existing period. Otherwise it's a new one
    # if count == 1:
    #     for col_to_change in cols_to_change:
    #         joined_mps = joined_mps.withColumn(col_to_change, when(col(Colname.from_date) == col(Colname.effective_date), col(f"updated_{col_to_change}")).otherwise(col(col_to_change)))
    #     # return a DF with the same schema as input
    #     result_df = joined_mps.select(target_dataframe.columns)
    # else:
    #     # Logic to find and update to_date on dataframe
    #     update_func_to_date = (when((col(Colname.from_date) < col(Colname.effective_date)) & (col(Colname.to_date) > col(Colname.effective_date)), col(Colname.effective_date))
    #                            .otherwise(col(Colname.to_date)))

    #     # if we need to update all future periods use this  update_func_settlement_method = (when((col(Colname.from_date) >= col(Colname.effective_date) & ), col(f"updated_{col_to_change}")).otherwise(col(col_to_change)))

    #     joined_mps = joined_mps.withColumn(f"old_{Colname.to_date}", col(Colname.to_date))

    #     periods_df = joined_mps.withColumn(Colname.to_date, update_func_to_date)
    #     # if we need to update all future periods use this .withColumn(col_to_change, update_func_settlement_method)

    #     row_to_add = periods_df.filter(col(Colname.to_date) == col(Colname.effective_date)).first()

    #     rdd = spark.sparkContext.parallelize([row_to_add])

    #     dataframe_to_add = spark.createDataFrame(rdd, periods_df.schema)

    #     # Updated dataframe to add
    #     for col_to_change in cols_to_change:
    #         dataframe_to_add = dataframe_to_add.withColumn(col_to_change, col(f"updated_{col_to_change}"))

    #     dataframe_to_add = dataframe_to_add.withColumn(Colname.to_date, col(f"old_{Colname.to_date}")).withColumn(Colname.from_date, col(Colname.effective_date))

    #     resulting_dataframe_period_df = periods_df.union(dataframe_to_add)

    #     result_df = resulting_dataframe_period_df.select(target_dataframe.columns)

    # return result_df
