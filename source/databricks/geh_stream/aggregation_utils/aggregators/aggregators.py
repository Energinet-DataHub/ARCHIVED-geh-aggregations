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
from pyspark.sql.functions import col, window
from geh_stream.codelists import MarketEvaluationPointType, SettlementMethod, ConnectionState


grid_area = 'MeteringGridArea_Domain_mRID'
brp = 'BalanceResponsibleParty_MarketParticipant_mRID'
es = 'EnergySupplier_MarketParticipant_mRID'
time_window = 'time_window'
mp = "MarketEvaluationPointType"
in_ga = "InMeteringGridArea_Domain_mRID"
out_ga = "OutMeteringGridArea_Domain_mRID"
cs = "ConnectionState"
aggregated_quality = "aggregated_quality"
sum_quantity = "sum_quantity"


# Function to aggregate hourly net exchange per neighbouring grid areas (step 1)
def aggregate_net_exchange_per_neighbour_ga(df: DataFrame):
    exchange_in = df \
        .filter(col(mp) == MarketEvaluationPointType.exchange.value) \
        .filter((col(cs) == ConnectionState.connected.value) | (col(cs) == ConnectionState.disconnected.value)) \
        .groupBy(in_ga, out_ga, window(col("Time"), "1 hour"), aggregated_quality) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", "in_sum") \
        .withColumnRenamed("window", time_window)
    exchange_out = df \
        .filter(col(mp) == MarketEvaluationPointType.exchange.value) \
        .filter((col(cs) == ConnectionState.connected.value) | (col(cs) == ConnectionState.disconnected.value)) \
        .groupBy(in_ga, out_ga, window(col("Time"), "1 hour")) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", "out_sum") \
        .withColumnRenamed("window", time_window)
    exchange = exchange_in.alias("exchange_in").join(
        exchange_out.alias("exchange_out"),
        (col("exchange_in.InMeteringGridArea_Domain_mRID")
         == col("exchange_out.OutMeteringGridArea_Domain_mRID"))
        & (col("exchange_in.OutMeteringGridArea_Domain_mRID")
           == col("exchange_out.InMeteringGridArea_Domain_mRID"))
        & (exchange_in.time_window == exchange_out.time_window)) \
        .select(exchange_in["*"], exchange_out["out_sum"]) \
        .withColumn(
            sum_quantity,
            col("in_sum") - col("out_sum")) \
        .select(
            "InMeteringGridArea_Domain_mRID",
            "OutMeteringGridArea_Domain_mRID",
            "time_window",
            aggregated_quality,
            sum_quantity)
    return exchange


# Function to aggregate hourly net exchange per grid area (step 2)
def aggregate_net_exchange_per_ga(df: DataFrame):
    exchangeIn = df \
        .filter(col(mp) == MarketEvaluationPointType.exchange.value) \
        .filter((col(cs) == ConnectionState.connected.value) | (col(cs) == ConnectionState.disconnected.value))
    exchangeIn = exchangeIn \
        .groupBy(in_ga, window(col("Time"), "1 hour"), aggregated_quality) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", "in_sum") \
        .withColumnRenamed("window", time_window) \
        .withColumnRenamed(in_ga, grid_area)
    exchangeOut = df \
        .filter(col(mp) == MarketEvaluationPointType.exchange.value) \
        .filter((col(cs) == ConnectionState.connected.value) | (col(cs) == ConnectionState.disconnected.value))
    exchangeOut = exchangeOut \
        .groupBy(out_ga, window(col("Time"), "1 hour")) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", "out_sum") \
        .withColumnRenamed("window", time_window) \
        .withColumnRenamed(out_ga, grid_area)
    joined = exchangeIn \
        .join(exchangeOut,
              (exchangeIn.MeteringGridArea_Domain_mRID == exchangeOut.MeteringGridArea_Domain_mRID) & (exchangeIn.time_window == exchangeOut.time_window),
              how="outer") \
        .select(exchangeIn["*"], exchangeOut["out_sum"])
    resultDf = joined.withColumn(
        sum_quantity, joined["in_sum"] - joined["out_sum"]) \
        .select(grid_area, time_window, sum_quantity, aggregated_quality)
    return resultDf


# Function to aggregate hourly consumption per grid area, balance responsible party and energy supplier (step 3)
def aggregate_hourly_consumption(df: DataFrame):
    return aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.consumption, SettlementMethod.non_profiled)


# Function to aggregate flex consumption per grid area, balance responsible party and energy supplier (step 4)
def aggregate_flex_consumption(df: DataFrame):
    return aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.consumption, SettlementMethod.flex_settled)


# Function to aggregate hourly production per grid area, balance responsible party and energy supplier (step 5)
def aggregate_hourly_production(df: DataFrame):
    return aggregate_per_ga_and_brp_and_es(df, MarketEvaluationPointType.production, None)


# Function to aggregate sum per grid area, balance responsible party and energy supplier (step 3, 4 and 5)
def aggregate_per_ga_and_brp_and_es(df: DataFrame, market_evaluation_point_type: MarketEvaluationPointType, settlement_method: SettlementMethod):
    result = df.filter(col(mp) == market_evaluation_point_type.value)
    if settlement_method is not None:
        result = result.filter(col("SettlementMethod") == settlement_method.value)
    result = result.filter((col(cs) == ConnectionState.connected.value) | (col(cs) == ConnectionState.disconnected.value))
    result = result \
        .groupBy(grid_area, brp, es, window(col("Time"), "1 hour"), aggregated_quality) \
        .sum("Quantity") \
        .withColumnRenamed("sum(Quantity)", sum_quantity) \
        .withColumnRenamed("window", time_window)
    return result


# Function to aggregate sum per grid area and energy supplier (step 12, 13 and 14)
def aggregate_per_ga_and_es(df: DataFrame):
    return df \
        .groupBy(grid_area, es, time_window, aggregated_quality) \
        .sum(sum_quantity) \
        .withColumnRenamed('sum(sum_quantity)', sum_quantity)


# Function to aggregate sum per grid area and balance responsible party (step 15, 16 and 17)
def aggregate_per_ga_and_brp(df: DataFrame):
    return df \
        .groupBy(grid_area, brp, time_window, aggregated_quality) \
        .sum(sum_quantity) \
        .withColumnRenamed('sum(sum_quantity)', sum_quantity)


# Function to aggregate sum per grid area (step 18, 19 and 20)
def aggregate_per_ga(df: DataFrame):
    return df \
        .groupBy(grid_area, time_window, aggregated_quality) \
        .sum(sum_quantity) \
        .withColumnRenamed('sum(sum_quantity)', sum_quantity)
