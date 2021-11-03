
from geh_stream.bus import MessageDispatcher, messages as m
from pyspark.sql.session import SparkSession
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
        .partitionBy("metering_point_id") \
        .save(master_data_path)


def on_settlement_method_updated(msg: m.SettlementMethodUpdated):

    spark = SparkSession.builder.getOrCreate()
    # Get master_data_path
    master_data_path = f"{dispatcher.master_data_root_path}{msg.get_master_data_path}"

    # Get all existing metering point periods
    consumption_mps_df = spark.read.format("delta").load(master_data_path).where(f"metering_point_id = '{msg.metering_point_id}'")

    # Get the event data frame
    settlement_method_updated_df = msg.get_dataframe()

    result_df = period_mutations(spark, consumption_mps_df, settlement_method_updated_df, ["settlement_method"])

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




# -- Dispatcher --------------------------------------------------------------
dispatcher = MessageDispatcher({
    m.ConsumptionMeteringPointCreated: on_consumption_metering_point_created,
    m.SettlementMethodUpdated: on_settlement_method_updated,
})
