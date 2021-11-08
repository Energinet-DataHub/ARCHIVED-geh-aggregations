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
from pyspark.sql.functions import col, when, row_number, lead
from geh_stream.codelists import Colname
from pyspark.sql.window import Window


def period_mutations(spark, target_dataframe: DataFrame, event_df: DataFrame, cols_to_change):

    # Update col names to update on event dataframe
    for col_to_change in cols_to_change:
        event_df = event_df \
            .withColumnRenamed(col_to_change, f"updated_{col_to_change}")

    # Join event dataframe with target dataframe
    joined = target_dataframe \
        .join(event_df, [Colname.metering_point_id], "inner")

    # Periods that should not be updated
    df_periods_to_keep = joined \
        .filter(col(Colname.to_date) <= col(Colname.effective_date))

    # Periods that should be updated
    df_periods_to_update = joined \
        .filter(col(Colname.to_date) > col(Colname.effective_date)) \
        .orderBy(col(Colname.from_date))

    # Count periods to update to be able to select update logic to apply
    periods_to_update_count = df_periods_to_update.count()

    # Generic update function to find out if period to_date should be set to effective_date from event
    update_func_to_date = (
        when(
            (col(Colname.from_date) < col(Colname.effective_date))
            & (col(Colname.to_date) > col(Colname.effective_date)),
            col(Colname.effective_date))
        .otherwise(col(Colname.to_date)))

    # Use this logic to update multiple periods with event data
    if periods_to_update_count > 1:

        # Window function to be used in row_number and lead functions
        windowSpec = Window \
            .partitionBy(Colname.metering_point_id) \
            .orderBy(Colname.to_date)

        # Set row_number on periods to update
        df_periods_to_update = df_periods_to_update \
            .withColumn("row_number", row_number().over(windowSpec))

        # Set to_date based on update_func_to_date logic
        df_periods_to_update = df_periods_to_update \
            .withColumn(Colname.to_date, update_func_to_date)

        # Add column next_from_date with next row's from_date
        df_periods_to_update = df_periods_to_update \
            .withColumn("next_from_date", lead(col(Colname.from_date), 1).over(windowSpec))

        # Select first period in new variable, used to add new period to master dataframe
        new_row = df_periods_to_update \
            .filter(col("row_number") == 1)

        # Set from_date and to_date on new_row
        new_row = new_row \
            .withColumn(Colname.from_date, col(Colname.to_date)) \
            .withColumn(Colname.to_date, col("next_from_date"))

        # Only add new_row if from_date and to_date are not equal
        if(new_row.filter(col(Colname.from_date) != col(Colname.to_date)).count() == 1):
            df_periods_to_update = df_periods_to_update \
                .union(new_row)

        # Drop column row_number as we don't need the column going forward
        df_periods_to_update = df_periods_to_update \
            .drop("row_number")

    # Use this update logic if only one period should be updated and a new period should be added
    else:
        # Set to_date based on update_func_to_date logic and add new column to hold old_to_date
        df_periods_to_update = df_periods_to_update \
            .withColumn("old_to_date", col(Colname.to_date)) \
            .withColumn(Colname.to_date, update_func_to_date)

        # new_row to be added, updated with to_date equal to old_to_date
        new_row = df_periods_to_update \
            .withColumn(Colname.from_date, col(Colname.effective_date)) \
            .withColumn(Colname.to_date, col("old_to_date"))

        # Add new_row to existing periods_to_update
        df_periods_to_update = df_periods_to_update \
            .union(new_row)

    # If period exists that have to_date equal to effective_date, then it's held in new variable (this is added to the periods_to_keep to ensure that no data is updated)
    split_period = df_periods_to_update \
        .filter(col(Colname.to_date) == col(Colname.effective_date)) \
        .select(target_dataframe.columns)

    # Add split_periods to periods_to_keep
    df_periods_to_keep = df_periods_to_keep \
        .select(target_dataframe.columns) \
        .union(split_period)

    # Filter on periods that have to_date after effective_date
    df_periods_to_update = df_periods_to_update \
        .filter(col(Colname.to_date) > col(Colname.effective_date))

    # Update periods_to_update with event data
    for col_to_change in cols_to_change:
        df_periods_to_update = df_periods_to_update \
            .withColumn(col_to_change, col(f"updated_{col_to_change}"))

    # Select only columns that corresponds to columns in target_dataframe
    df_periods_to_update = df_periods_to_update \
        .select(target_dataframe.columns)

    # Union periods_to_keep with updated periods
    result = df_periods_to_keep \
        .select(target_dataframe.columns) \
        .union(df_periods_to_update)

    return result
