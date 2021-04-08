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

# Uncomment the lines below to include modules distributed by wheel
# import sys
# sys.path.append(r'/workspaces/green-energy-hub/src/streaming')

# %% Setup

#setup connection to delta lake
import json
from pyspark.sql.types import *
from pyspark.sql.functions import *
from pyspark import SparkConf
from pyspark.sql import SparkSession
import pandas as pd
from datetime import date, datetime

storage_account_name = "MORTEN_FIKSER" # this must be changed to your storage account name
storage_account_key = "MORTEN_FIKSER"
containerName = "MORTEN_FIKSER"
spark.conf.set(
  "fs.azure.account.key.{0}.dfs.core.windows.net".format(storage_account_name),
  storage_account_key)




# %% Read CSV data

#read csv data containing information about different metering point configurations generated from datahub2 report
test_data_csv_source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/test-data-base/test_data_csv.csv"
print(test_data_csv_source)

csv_df = spark.read.format('csv').options(inferSchema = "true", delimiter=";", header="true").load(test_data_csv_source)
csv_df.display()

# %% Initial data creation

#THIS SHOULD ONLY BE RUN ONCE!!!!!!!
#create dataframe with mock data based on csv file and insert into delta lake for given period
from delta.tables import *
import random
import decimal
from datetime import datetime, timedelta, time
import time

period_from = datetime(2013, 1, 1, 0)
period_to = datetime(2013, 1, 1, 1)

print(period_from)
print(period_to)

output_delta_lake_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"

parquet_schema: StructType = StructType() \
        .add("CreatedDateTime", TimestampType(), False) \
        .add("MarketEvaluationPointType", StringType(), False) \
        .add("SettlementMethod", StringType(), True) \
        .add("MarketEvaluationPoint_mRID", StringType(), False) \
        .add("Quantity", DecimalType(18, 3), True) \
        .add("Quality", StringType(), True) \
        .add("Time", TimestampType(), False) \
        .add("MeterReadingPeriodicity", StringType(), True) \
        .add("MeteringGridArea_Domain_mRID", StringType(), False) \
        .add("ConnectionState", StringType(), False) \
        .add("EnergySupplier_MarketParticipant_mRID", StringType(), True) \
        .add("BalanceResponsibleParty_MarketParticipant_mRID", StringType(), True) \
        .add("InMeteringGridArea_Domain_mRID", StringType(), True) \
        .add("OutMeteringGridArea_Domain_mRID", StringType(), True) \
        .add("Product", StringType(), True) \
        .add("Year", IntegerType(), True) \
        .add("Month", IntegerType(), True) \
        .add("Day", IntegerType(), True) \
  
current_period_from = period_from
csv_count = csv_df.count()
qualities = ["E01", "56", "D01"]
grid_areas = csv_df.select(col("GridArea")).rdd.flatMap(lambda x: x).distinct().collect()

incrementation = 3500

while current_period_from < period_to:
  start = time.time()
  current_period_to = current_period_from + timedelta(hours=1)
  metering_point_id = 578710000000000000
  start_index = 0
  end_index = incrementation
  while start_index < csv_count:
    print("Index: " + str(start_index))
    start_index_span = time.time()
    
    entries = []
    
    for i in range(start_index, end_index):
      row = csv_df.collect()[i]      
      
      for j in range(int(row["Count"])):
        metering_point_id = metering_point_id + 1
        
        resolution = row["Reading_Occurrence"]
        
        minutes_to_increment = 60
        
        if resolution == "P1M":
          minutes_to_increment = 1
        if resolution == "PT15M":
          minutes_to_increment = 15
        
        current_datetime = current_period_from
        quality = random.choice(qualities)
        
        while current_datetime < current_period_to:
          entry = (
              datetime.now(), #CreatedDateTime
              row["Type_Of_MP"], #MarketEvaluationPointType
              row["Settlement_Method"], #SettlementMethod
              str(metering_point_id), #MarketEvaluationPoint_mRID
              decimal.Decimal(str(decimal.Decimal(random.randrange(1, 99))) + "." + str(decimal.Decimal(random.randrange(100, 999)))), #Quantity
              quality, #Quality
              current_datetime, #Time
              resolution, #MeterReadingPeriodicity
              row["GridArea"], #MeteringGridArea_Domain_mRID
              row["Physical_Status"], #ConnectionState
              row["Supplier"], #EnergySupplier_MarketParticipant_mRID
              row["BRP"], #BalanceResponsibleParty_MarketParticipant_mRID
              row["FromGridArea"], #InMeteringGridArea_Domain_mRID
              row["ToGridArea"], #OutMeteringGridArea_Domain_mRID
              "8716867000030", #Product
              current_datetime.year,
              current_datetime.month,
              current_datetime.day,
                  )
          entries.append(entry)
        
          current_datetime = current_datetime + timedelta(minutes=minutes_to_increment)
    
    print("Number of entries: " + str(len(entries)))
    df = spark.createDataFrame( \
        spark.sparkContext.parallelize(entries), \
        parquet_schema)
    
    print("Data frame created")

    df.select(col("MarketEvaluationPoint_mRID"),
                    col("CreatedDateTime"),
                    col("Time"),
                    col("Quantity"),
                    col("MarketEvaluationPointType"),
                    col("Quality"),
                    col("MeterReadingPeriodicity"),
                    col("MeteringGridArea_Domain_mRID"),
                    col("ConnectionState"),
                    col("EnergySupplier_MarketParticipant_mRID"),
                    col("BalanceResponsibleParty_MarketParticipant_mRID"),
                    col("InMeteringGridArea_Domain_mRID"),
                    col("OutMeteringGridArea_Domain_mRID"),
                    col("SettlementMethod"),
                    col("Product"),
                    col("Year"),
                    col("Month"),
                    col("Day")) \
            .write \
            .partitionBy("Year", "Month", "Day") \
            .format("delta") \
            .mode("append") \
            .save(output_delta_lake_path)
    
    print("Data frame saved")
    
    end_index_span = time.time()
    
    print("Index span " + str(start_index) + " - " + str(end_index) + " took {} seconds".format(end_index_span-start_index_span))
  
    if (start_index + incrementation) < csv_count:
      start_index = end_index
      end_index = end_index + incrementation
      if end_index >= csv_count:
        end_index = csv_count - 1
    else:
      start_index = csv_count
  
  end = time.time()
  print("Done with " + str(current_period_from) + ", it took {} seconds".format(end-start))
  current_period_from = current_period_from + timedelta(hours=1)


# %% Update dataframe

#this section should be updated to be able to update data within a given period

#output_delta_lake_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/meter-data/"
#
#from delta.tables import *
#
#deltaTable = DeltaTable.forPath(spark, output_delta_lake_path)
#deltaTable.alias("oldData") \
#  .merge( 
#    df.alias("newData"), \
#    "oldData.Year = newData.Year AND oldData.Month = newData.Month AND oldData.Day = newData.Day AND oldData.MarketEvaluationPoint_mRID = newData.MarketEvaluationPoint_mRID AND oldData.Time = newData.Time") \
#  .whenMatchedUpdate(set = { 
#    "CreatedDateTime" : "newData.CreatedDateTime",
#    "Quantity" : "newData.Quantity", 
#    "MarketEvaluationPointType" : "newData.MarketEvaluationPointType",
#    "Quality" : "newData.Quality",
#    "MeterReadingPeriodicity" : "newData.MeterReadingPeriodicity",
#    "MeteringGridArea_Domain_mRID" : "newData.MeteringGridArea_Domain_mRID",
#    "ConnectionState" : "newData.ConnectionState",
#    "EnergySupplier_MarketParticipant_mRID" : "newData.EnergySupplier_MarketParticipant_mRID",
#    "BalanceResponsibleParty_MarketParticipant_mRID" : "newData.BalanceResponsibleParty_MarketParticipant_mRID",
#    "InMeteringGridArea_Domain_mRID" : "newData.InMeteringGridArea_Domain_mRID",
#    "OutMeteringGridArea_Domain_mRID" : "newData.OutMeteringGridArea_Domain_mRID",
#    "SettlementMethod" : "newData.SettlementMethod"
#    }) \
#  .whenNotMatchedInsertAll() \
#  .execute()

# %% Test data delta table history

#display delta table history

from delta.tables import *

output_delta_lake_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"

deltaTable = DeltaTable.forPath(spark, output_delta_lake_path)
fullHistoryDF = deltaTable.history()
fullHistoryDF.display()

# %% Display dataframe for latest full hour

from datetime import datetime, timedelta
from pyspark.sql.window import Window

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source)

highest_year = df.groupBy("Year").max("Year").sort("max(Year)", ascending=False).take(1)[0]["max(Year)"]
print(f"Year: {highest_year}")

highest_month = df.filter(col("Year") == highest_year).groupBy("Month").max("Month").sort("max(Month)", ascending=False).take(1)[0]["max(Month)"]
print(f"Month: {highest_month}")

highest_day = df.filter(col("Year") == highest_year).filter(col("Month") == highest_month).groupBy("Day").max("Day").sort("max(Day)", ascending=False).take(1)[0]["max(Day)"]
print(f"Day: {highest_day}")

highest_datetime = df.filter(col("Year") == highest_year).filter(col("Month") == highest_month).filter(col("Day") == highest_day).sort("Time", ascending=False).take(1)[0]["Time"]
print(f"Datetime: {highest_datetime}")

from_datetime = datetime(highest_datetime.year, highest_datetime.month, highest_datetime.day, highest_datetime.hour)
to_datetime = from_datetime + timedelta(hours=1)

df = df.filter(col("Time") >= from_datetime).filter(col("Time") < to_datetime)
df..filter(col("MarketEvaluationPointType") == "E20")display()

#csv_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/csv-output/"

#df.write.format("csv").save(csv_path)

#window = Window.partitionBy(df['MeteringGridArea_Domain_mRID']).orderBy(df['MarketEvaluationPoint_mRID'].desc())
#
#df.select('*', rank().over(window).alias('rank')) \
#  .filter(col('rank') < 2) \
#  .filter(col("MarketEvaluationPointType") == "E17") \
#  .display() 
  
#df
#highest_datetime = df.orderBy(["Year", "Month", "Day", "Time"], ascending=[0, 0, 0, 0]).take(1)[0]["Time"]
#print(f"{highest_datetime}")
#from_datetime = datetime(highest_datetime.year, highest_datetime.month, highest_datetime.day, highest_datetime.hour)
#to_datetime = from_datetime + timedelta(hours=1)

#df = df.filter(col("Time") >= from_datetime).filter(col("Time") < to_datetime)



# %% Add data to delta table based on dataframe from latest full hour 

#THE ONE TO RUN
import time
from datetime import datetime, timedelta

add_days = 1

start = time.time()

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source)

highest_year = df.groupBy("Year").max("Year").sort("max(Year)", ascending=False).take(1)[0]["max(Year)"]
print(f"Year: {highest_year}")

highest_month = df.filter(col("Year") == highest_year).groupBy("Month").max("Month").sort("max(Month)", ascending=False).take(1)[0]["max(Month)"]
print(f"Month: {highest_month}")

highest_day = df.filter(col("Year") == highest_year).filter(col("Month") == highest_month).groupBy("Day").max("Day").sort("max(Day)", ascending=False).take(1)[0]["max(Day)"]
print(f"Day: {highest_day}")

highest_datetime = df.filter(col("Year") == highest_year).filter(col("Month") == highest_month).filter(col("Day") == highest_day).sort("Time", ascending=False).take(1)[0]["Time"]
print(f"Datetime: {highest_datetime}")

from_datetime = datetime(highest_datetime.year, highest_datetime.month, highest_datetime.day, highest_datetime.hour)
to_datetime = from_datetime + timedelta(hours=1)

df = df.filter(col("Time") >= from_datetime).filter(col("Time") < to_datetime)

print(f"Base data from {from_datetime} to {to_datetime} fetched, which contains {df.count()} entries")
print(f"We will add {add_days} days worth of data from {to_datetime} to {to_datetime + timedelta(days=add_days)}")

add_hours = add_days * 24

print(f"{df.count() * add_hours} entries will be appended to the delta lake")

the_df = None

for i in range(1, add_hours + 1):
  
  interval_string = "INTERVAL {hours} HOURS".format(hours = i)
  
  new_df = df.withColumn("Time", col("Time") + expr(interval_string)).withColumn("Year", year(col("Time"))).withColumn("Month", month(col("Time"))).withColumn("Day", dayofmonth(col("Time")))
  
  if the_df is None:
    the_df = new_df
  else:
    the_df = the_df.union(new_df)
  
the_df.select(col("MarketEvaluationPoint_mRID"),
              col("CreatedDateTime"),
              col("Time"),
              col("Quantity"),
              col("MarketEvaluationPointType"),
              col("Quality"),
              col("MeterReadingPeriodicity"),
              col("MeteringGridArea_Domain_mRID"),
              col("ConnectionState"),
              col("EnergySupplier_MarketParticipant_mRID"),
              col("BalanceResponsibleParty_MarketParticipant_mRID"),
              col("InMeteringGridArea_Domain_mRID"),
              col("OutMeteringGridArea_Domain_mRID"),
              col("SettlementMethod"),
              col("Product"),
              col("Year"),
              col("Month"),
              col("Day")) \
      .write \
      .partitionBy("Year", "Month", "Day") \
      .format("delta") \
      .mode("append") \
      .save(source)
  
end = time.time()
print("Duration: {seconds} seconds".format(seconds = end-start) )
