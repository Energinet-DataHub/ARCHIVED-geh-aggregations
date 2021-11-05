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
from pyspark.sql.functions import col, when, lag, row_number
from geh_stream.codelists import Colname
from pyspark.sql.window import Window


def period_mutations(spark, target_dataframe: DataFrame, event_df: DataFrame, cols_to_change):

    # Update col names to update on event dataframe
    for col_to_change in cols_to_change:
        event_df = event_df.withColumnRenamed(col_to_change, f"updated_{col_to_change}")

    event_df.show()

    # Join event dataframe with target dataframe
    joined = target_dataframe.join(event_df, [Colname.metering_point_id], "inner")
    joined.show()

    # periods that should not be updated
    df_periods_to_keep = joined.filter(col(Colname.to_date) <= col(Colname.effective_date))
    df_periods_to_keep.show()

    # Periods that should be updated
    df_periods_to_update = joined \
        .filter(col(Colname.to_date) > col(Colname.effective_date)) \
        .orderBy(col(Colname.from_date))

    df_periods_to_update.show()

    periods_to_update_count = df_periods_to_update.count()

    update_func_to_date = (when((col(Colname.from_date) < col(Colname.effective_date)) & (col(Colname.to_date) > col(Colname.effective_date)), col(Colname.effective_date))
                           .otherwise(col(Colname.to_date)))

    if periods_to_update_count > 1:

        windowSpec = Window.partitionBy(Colname.metering_point_id).orderBy(Colname.to_date)

        update_func_from_date = (when((col("row_number") > 1), lag(col(Colname.to_date), 1).over(windowSpec))
                                 .otherwise(col(Colname.from_date)))

        df_periods_to_update = df_periods_to_update.withColumn("row_number", row_number().over(windowSpec))

        print("after row number")
        df_periods_to_update.show()

        df_periods_to_update = df_periods_to_update \
            .withColumn(Colname.to_date, update_func_to_date)

        print("after to_date updated")
        df_periods_to_update.show()

        df_periods_to_update = df_periods_to_update \
            .withColumn(Colname.from_date, update_func_from_date)

        print("after from_date updated")
        df_periods_to_update.show()

        df_periods_to_update = df_periods_to_update.drop("row_number")

        print("after drop row_number")
        df_periods_to_update.show()

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

    print("after update")
    df_periods_to_update.show()

    df_periods_to_update = df_periods_to_update.select(target_dataframe.columns)

    result = df_periods_to_keep.select(target_dataframe.columns).union(df_periods_to_update)
    print("result")
    result.show()

    return result
