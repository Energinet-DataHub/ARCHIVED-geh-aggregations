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
from geh_stream.codelists import MarketEvaluationPointType, SettlementMethod, ConnectionState, Names


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
        .filter(col(Names.metering_point_type.value) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Names.connection_state.value) == ConnectionState.connected.value) | (col(Names.connection_state.value) == ConnectionState.disconnected.value)) \
        .groupBy(Names.in_grid_area.value, Names.out_grid_area.value, window(col(Names.time.value), "1 hour"), Names.aggregated_quality.value) \
        .sum(Names.quantity.value) \
        .withColumnRenamed("sum({0})".format(Names.quantity.value), "in_sum") \
        .withColumnRenamed("window", Names.time_window.value) \
        .withColumnRenamed(Names.in_grid_area.value, "ExIn_InMeteringGridArea_Domain_mRID") \
        .withColumnRenamed(Names.out_grid_area.value, "ExIn_OutMeteringGridArea_Domain_mRID")
    exchange_out = df \
        .filter(col(Names.metering_point_type.value) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Names.connection_state.value) == ConnectionState.connected.value) | (col(Names.connection_state.value) == ConnectionState.disconnected.value)) \
        .groupBy(Names.in_grid_area.value, Names.out_grid_area.value, window(col(Names.time.value), "1 hour")) \
        .sum(Names.quantity.value) \
        .withColumnRenamed("sum({0})".format(Names.quantity.value), "out_sum") \
        .withColumnRenamed("window", Names.time_window.value) \
        .withColumnRenamed(Names.in_grid_area.value, "ExOut_InMeteringGridArea_Domain_mRID") \
        .withColumnRenamed(Names.out_grid_area.value, "ExOut_OutMeteringGridArea_Domain_mRID")

    exchange = exchange_in.join(
        exchange_out, [Names.time_window.value]) \
        .filter(exchange_in.ExIn_InMeteringGridArea_Domain_mRID == exchange_out.ExOut_OutMeteringGridArea_Domain_mRID) \
        .filter(exchange_in.ExIn_OutMeteringGridArea_Domain_mRID == exchange_out.ExOut_InMeteringGridArea_Domain_mRID) \
        .select(exchange_in["*"], exchange_out["out_sum"]) \
        .withColumn(
            Names.sum_quantity.value,
            col("in_sum") - col("out_sum")) \
        .withColumnRenamed("ExIn_InMeteringGridArea_Domain_mRID", Names.in_grid_area.value) \
        .withColumnRenamed("ExIn_OutMeteringGridArea_Domain_mRID", Names.out_grid_area.value) \
        .select(
            Names.in_grid_area.value,
            Names.out_grid_area.value,
            Names.time_window.value,
            Names.aggregated_quality.value,
            Names.sum_quantity.value)
    return exchange


# Function to aggregate hourly net exchange per grid area (step 2)
def aggregate_net_exchange_per_ga(df: DataFrame):
    exchangeIn = df \
        .filter(col(Names.metering_point_type.value) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Names.connection_state.value) == ConnectionState.connected.value) | (col(Names.connection_state.value) == ConnectionState.disconnected.value))
    exchangeIn = exchangeIn \
        .groupBy(Names.in_grid_area.value, window(col(Names.time.value), "1 hour"), Names.aggregated_quality.value) \
        .sum(Names.quantity.value) \
        .withColumnRenamed("sum({0})".format(Names.quantity.value), "in_sum") \
        .withColumnRenamed("window", Names.time_window.value) \
        .withColumnRenamed(Names.in_grid_area.value, Names.grid_area.value)
    exchangeOut = df \
        .filter(col(Names.metering_point_type.value) == MarketEvaluationPointType.exchange.value) \
        .filter((col(Names.connection_state.value) == ConnectionState.connected.value) | (col(Names.connection_state.value) == ConnectionState.disconnected.value))
    exchangeOut = exchangeOut \
        .groupBy(Names.out_grid_area.value, window(col(Names.time.value), "1 hour")) \
        .sum(Names.quantity.value) \
        .withColumnRenamed("sum({0})".format(Names.quantity.value), "out_sum") \
        .withColumnRenamed("window", Names.time_window.value) \
        .withColumnRenamed(Names.out_grid_area.value, Names.grid_area.value)
    joined = exchangeIn \
        .join(exchangeOut,
              (exchangeIn[Names.grid_area.value] == exchangeOut[Names.grid_area.value]) & (exchangeIn[Names.time_window.value] == exchangeOut[Names.time_window.value]),
              how="outer") \
        .select(exchangeIn["*"], exchangeOut["out_sum"])
    joined.show()
    resultDf = joined.withColumn(
        Names.sum_quantity.value, joined["in_sum"] - joined["out_sum"]) \
        .select(Names.grid_area.value, Names.time_window.value, Names.sum_quantity.value, Names.aggregated_quality.value)
    resultDf.show()
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
    result = df.filter(col(Names.metering_point_type.value) == market_evaluation_point_type.value)
    if settlement_method is not None:
        result = result.filter(col(Names.settlement_method.value) == settlement_method.value)
    result = result.filter((col(Names.connection_state.value) == ConnectionState.connected.value) | (col(Names.connection_state.value) == ConnectionState.disconnected.value))
    result = result \
        .groupBy(Names.grid_area.value, Names.balance_responsible_id.value, Names.energy_supplier_id.value, window(col(Names.time.value), "1 hour"), Names.aggregated_quality.value) \
        .sum(Names.quantity.value) \
        .withColumnRenamed("sum({0})".format(Names.quantity.value), Names.sum_quantity.value) \
        .withColumnRenamed("window", Names.time_window.value)
    return result


# Function to aggregate sum per grid area and energy supplier (step 12, 13 and 14)
def aggregate_per_ga_and_es(df: DataFrame):
    return df \
        .groupBy(Names.grid_area.value, Names.energy_supplier_id.value, Names.time_window.value, Names.aggregated_quality.value) \
        .sum(Names.sum_quantity.value) \
        .withColumnRenamed('sum({0})'.format(Names.sum_quantity.value), Names.sum_quantity.value)


# Function to aggregate sum per grid area and balance responsible party (step 15, 16 and 17)
def aggregate_per_ga_and_brp(df: DataFrame):
    return df \
        .groupBy(Names.grid_area.value, Names.balance_responsible_id.value, Names.time_window.value, Names.aggregated_quality.value) \
        .sum(Names.sum_quantity.value) \
        .withColumnRenamed('sum({0})'.format(Names.sum_quantity.value), Names.sum_quantity.value)


# Function to aggregate sum per grid area (step 18, 19 and 20)
def aggregate_per_ga(df: DataFrame):
    return df \
        .groupBy(Names.grid_area.value, Names.time_window.value, Names.aggregated_quality.value) \
        .sum(Names.sum_quantity.value) \
        .withColumnRenamed('sum({0})'.format(Names.sum_quantity.value), Names.sum_quantity.value)
