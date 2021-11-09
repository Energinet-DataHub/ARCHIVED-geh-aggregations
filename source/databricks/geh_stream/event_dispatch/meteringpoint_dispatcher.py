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
from pyspark.sql.session import SparkSession
from geh_stream.codelists.colname import Colname
from geh_stream.event_dispatch.dispatcher_base import period_mutations


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
        .partitionBy(Colname.metering_point_id) \
        .save(master_data_path)


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):

    spark = SparkSession.builder.getOrCreate()
    # Get master_data_path
    master_data_path = f"{dispatcher.master_data_root_path}{msg.get_master_data_path}"

    # Get all existing metering point periods
    consumption_mps_df = spark.read.format("delta").load(master_data_path).where(f"{Colname.metering_point_id} = '{msg.metering_point_id}'")

    # Get the event data frame
    settlement_method_updated_df = msg.get_dataframe()

    result_df = period_mutations(consumption_mps_df, settlement_method_updated_df, [Colname.settlement_method])

    # persist updated mps
    result_df \
        .write \
        .format("delta") \
        .mode("overwrite") \
        .partitionBy(Colname.metering_point_id) \
        .option("replaceWhere", f"{Colname.metering_point_id} == '{msg.metering_point_id}'") \
        .save(master_data_path)

    print("update smethod " + msg.settlement_method + " on id " + msg.metering_point_id)


# -- Dispatcher --------------------------------------------------------------
dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
