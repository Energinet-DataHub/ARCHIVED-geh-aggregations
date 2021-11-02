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
from geh_stream.bus import MessageDispatcher, messages as m
from delta.tables import DeltaTable
from pyspark.sql.session import SparkSession
from pyspark.sql.functions import col, lit, when
from pyspark.sql.types import StructType, StringType, StructField, TimestampType


def on_consumption_metering_point_created(msg: m.ConsumptionMeteringPointCreated):
    # Event --> Dataframe
    df = msg.get_dataframe()
    print(df.show())

    # Get master_data_path
    master_data_path = f"{dispatcher.master_data_root_path}{msg.get_master_data_path}"
    # Save Dataframe to that path
    df \
        .write \
        .format("delta") \
        .mode("append") \
        .partitionBy("metering_point_id") \
        .save(master_data_path)


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):
    # TODO right now this below is super draft, it will be refined later on

    spark = SparkSession.builder.getOrCreate()
    # Get master_data_path
    master_data_path = f"{dispatcher.master_data_root_path}{msg.get_master_data_path}"

    # Get all existing metering point periods
    consumption_mps_df = spark.read.format("delta").load(master_data_path).where(f"metering_point_id = '{msg.metering_point_id}'")

    # Get the event data frame
    settlement_method_updated_df = msg.get_dataframe()

    result_df = handle_update(spark, consumption_mps_df, settlement_method_updated_df, ["settlement_method"])

    # persist updated mps
    result_df \
        .write \
        .format("delta") \
        .mode("overwrite") \
        .partitionBy("metering_point_id") \
        .option("replaceWhere", f"metering_point_id == '{msg.metering_point_id}'") \
        .save(master_data_path)

    # deltaTable = DeltaTable.forPath(SparkSession.builder.getOrCreate(), master_data_path)
    # deltaTable.update(f"metering_point_id = '{msg.metering_point_id}' AND effective_date >= '{msg.effective_date}'", {"settlement_method": f"'{msg.settlement_method}'"})
    print("update smethod " + msg.settlement_method + " on id " + msg.metering_point_id)


def handle_update(spark, target_dataframe: DataFrame, event_df: DataFrame, cols_to_change):

    #Merge the event data onto our existing periods
    for col_to_change in cols_to_change:
        event_df = event_df.withColumnRenamed(col_to_change, f"updated_{col_to_change}")
    joined_mps = target_dataframe.join(event_df, "metering_point_id", "inner")

    count = joined_mps.where("valid_from == effective_date").count()

    # if we have a count of 1 than we've matched an existing period. Otherwise it's a new one
    if count == 1:
        for col_to_change in cols_to_change:
            joined_mps = joined_mps.withColumn(col_to_change, when(col("valid_from") == col("effective_date"), col(f"updated_{col_to_change}")).otherwise(col(col_to_change)))
        # return a DF with the same schema as input
        result_df = joined_mps.select(target_dataframe.columns)
    else:
        # Logic to find and update valid_to on dataframe
        update_func_valid_to = (when((col("valid_from") < col("effective_date")) & (col("valid_to") > col("effective_date")), col("effective_date"))
                                .otherwise(col("valid_to")))

        # update_func_settlement_method = (when((col("valid_from") >= col("effective_date") & ), col(f"updated_{col_to_change}")).otherwise(col(col_to_change)))

        joined_mps = joined_mps.withColumn("old_valid_to", col("valid_to"))

        periods_df = joined_mps.withColumn("valid_to", update_func_valid_to) # \
                                        # .withColumn(col_to_change, update_func_settlement_method)

        row_to_add = periods_df \
        .filter(col("valid_to") == col("effective_date")) \
        .first()

        rdd = spark.sparkContext.parallelize([row_to_add])

        dataframe_to_add = spark.createDataFrame(rdd, periods_df.schema)

        # Updated dataframe to add
        for col_to_change in cols_to_change:
            dataframe_to_add = dataframe_to_add.withColumn(col_to_change, col(f"updated_{col_to_change}"))
        
        dataframe_to_add = dataframe_to_add.withColumn("valid_to", col("old_valid_to")).withColumn("valid_from", col("effective_date"))

        resulting_dataframe_period_df = periods_df.union(dataframe_to_add)
        
        result_df = resulting_dataframe_period_df.select(target_dataframe.columns)

    return result_df


# -- Dispatcher --------------------------------------------------------------
dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
