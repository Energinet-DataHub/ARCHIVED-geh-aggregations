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
from pyspark.sql.functions import lit, col


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
        .option("compression", "snappy") \
        .mode("append") \
        .save(master_data_path)


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):
    # Get master_data_path
    master_data_path = f"{dispatcher.master_data_root_path}{msg.get_master_data_path}"

    # update meteringpoint
    deltaTable = DeltaTable.forPath(SparkSession.builder.getOrCreate(), master_data_path)
    deltaTable.update(f"metering_point_id = '{msg.metering_point_id}' AND effective_date >= '{msg.effective_date}'", {"settlement_method": f"'{msg.settlement_method}'"})
    print("update smethod" + msg.settlement_method + " on id " + msg.metering_point_id)


# -- Dispatcher --------------------------------------------------------------
dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
