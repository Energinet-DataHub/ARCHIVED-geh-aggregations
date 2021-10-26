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
from geh_stream.bus import MessageDispatcher, messages as m
from delta.tables import DeltaTable
from pyspark.sql.session import SparkSession
from pyspark.sql.functions import col, lit, when


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
        .save(master_data_path)


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):
    # Get master_data_path
    master_data_path = f"{dispatcher.master_data_root_path}{msg.get_master_data_path}"

    consumption_mps_df = SparkSession.builder.getOrCreate().read.format("delta").load(master_data_path).where(f"metering_point_id = '{msg.metering_point_id}'")

    settlement_method_updated_df = msg.get_dataframe()

    # do we match an existing period ? 



    settlement_method_updated_df = settlement_method_updated_df.withColumnRenamed("settlement_method", "updated_settlement_method")
    joined_mps = consumption_mps_df.join(settlement_method_updated_df, "metering_point_id", "inner")

    count = joined_mps.where(f"valid_from == effective_date").count()
    print(count)
    
    # if we have a count of 1 than we've matched an existing period. Otherwise its a new one
    if count == 1:
        updated_mps = joined_mps.withColumn("settlement_method", when(col("valid_from") == col("effective_date"), col("updated_settlement_method")).otherwise(col("settlement_method")))
        result_df = updated_mps.select("metering_point_id", "metering_point_type", "metering_gsrn_number","metering_grid_area","" "parent_id", "resolution", "unit", "product", "settlement_method", "metering_method", "meter_reading_periodicity", "net_settlement_group", "valid_from", "valid_to")

    else:
        # Logic to find and update valid_to on dataframe
        update_func_valid_to = (when((col("valid_from") <= col("effective_date")) & (col("valid_to") > col("effective_date")), col("effective_date"))
                            .otherwise(col("valid_to")))
        update_func_settlement_method = (when((col("valid_from") >= col("effective_date")), col("updated_settlement_method"))
                                     .otherwise(col("settlement_method")))


    print(result_df.show())

    # persist updated mps
    # existing_mps \5
    #     .write \
    #     .format("delta") \
    #     .mode("overwrite") \
    #     .save(master_data_path)

    # deltaTable = DeltaTable.forPath(SparkSession.builder.getOrCreate(), master_data_path)
    # deltaTable.update(f"metering_point_id = '{msg.metering_point_id}' AND effective_date >= '{msg.effective_date}'", {"settlement_method": f"'{msg.settlement_method}'"})
    print("update smethod " + msg.settlement_method + " on id " + msg.metering_point_id)


# -- Dispatcher --------------------------------------------------------------
dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
