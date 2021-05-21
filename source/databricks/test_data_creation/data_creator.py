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

storage_account_name = "STORAGE_ACCOUNT_NAME" # this must be changed to your storage account name
storage_account_key = "STORAGE_ACCOUNT_KEY"
containerName = "CONTAINER_NAME"
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
#This block does not rely on performance of spark, so the recommended period is 1 hour, not less and not more

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


# %% Delta table history

#Display delta table history

from delta.tables import *

output_delta_lake_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"

deltaTable = DeltaTable.forPath(spark, output_delta_lake_path)
fullHistoryDF = deltaTable.history()
fullHistoryDF.display()


# %% Query delta table as of timestamp

#Example on how to query data as it was at given timestamp

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
df = spark \
  .read \
  .format("delta") \
  .option("timestampAsOf", "2021-04-09T10:50:37.000+0000") \
  .load(source)

df = df.filter(col("Year") == "2013").filter(col("Month") == "1").filter(col("Day") == "1").orderBy(["MarketEvaluationPoint_mRID", "Time"], ascending=[1, 1]).display()


# %% Query delta table as of version

#Example on how to query data as it was at given version

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"

deltaTable = DeltaTable.forPath(spark, source)
print(deltaTable.history(1))
print(deltaTable.history(1).collect()[0]["version"])

df = spark \
  .read \
  .format("delta") \
  .option("versionAsOf", deltaTable.history(1).collect()[0]["version"]) \
  .load(source)


df = df.filter(col("Year") == "2013").filter(col("Month") == "1").filter(col("Day") == "1").orderBy(["MarketEvaluationPoint_mRID", "Time"], ascending=[1, 1]).display()


# %% Update dataframe

#Update dataframe in delta lake

output_delta_lake_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"

from delta.tables import *
from datetime import datetime, timedelta
import random
import decimal

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source)

df = df.filter(col("Year") == "2013").filter(col("Month") == "1").filter(col("Day") == "1")

print(f"Dataframe count: {df.count()}")

created_datetime = datetime.now()

new_df = df \
    .withColumn("Quantity", lit(decimal.Decimal(str(decimal.Decimal(random.randrange(1, 99))) + "." + str(decimal.Decimal(random.randrange(100, 999))))).cast(DecimalType(18, 3))) \
    .withColumn("Quality", lit("E02")) \
    .withColumn("EnergySupplier_MarketParticipant_mRID", lit("33333333333333")) \
    .withColumn("BalanceResponsibleParty_MarketParticipant_mRID", lit("33333333333333")) \
    .withColumn("SettlementMethod", lit("D02")) \
    .withColumn("Product", lit("33333333333333")) \
    .withColumn("CreatedDateTime", lit(created_datetime).cast(TimestampType())) \


deltaTable = DeltaTable.forPath(spark, output_delta_lake_path)
deltaTable.alias("oldData") \
  .merge( 
    new_df.alias("newData"), \
    "oldData.Year = newData.Year AND oldData.Month = newData.Month AND oldData.Day = newData.Day AND oldData.MarketEvaluationPoint_mRID = newData.MarketEvaluationPoint_mRID AND oldData.Time = newData.Time") \
  .whenMatchedUpdate(set = { 
    "CreatedDateTime" : "newData.CreatedDateTime",
    "Quantity" : "newData.Quantity", 
    "MarketEvaluationPointType" : "newData.MarketEvaluationPointType",
    "Quality" : "newData.Quality",
    "MeterReadingPeriodicity" : "newData.MeterReadingPeriodicity",
    "MeteringGridArea_Domain_mRID" : "newData.MeteringGridArea_Domain_mRID",
    "ConnectionState" : "newData.ConnectionState",
    "EnergySupplier_MarketParticipant_mRID" : "newData.EnergySupplier_MarketParticipant_mRID",
    "BalanceResponsibleParty_MarketParticipant_mRID" : "newData.BalanceResponsibleParty_MarketParticipant_mRID",
    "InMeteringGridArea_Domain_mRID" : "newData.InMeteringGridArea_Domain_mRID",
    "OutMeteringGridArea_Domain_mRID" : "newData.OutMeteringGridArea_Domain_mRID",
    "SettlementMethod" : "newData.SettlementMethod",
    "Product" : "newData.Product"
    }) \
  .whenNotMatchedInsertAll() \
  .execute()


# %% Add data to delta table based on dataframe from latest full hour 

#THE ONE TO RUN
#Inserts X days worth of data based on data for the latest hour in the delta lake

import time
from datetime import datetime, timedelta
import decimal
import random

add_days = 64

estimated_minutes = add_days * 1.5
estimated_finish_datetime = datetime.now() + timedelta(minutes=estimated_minutes)
print(f"Estimated completion time: {estimated_finish_datetime}")

start = time.time()

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source)

highest_year = df.select(max("Year")).first()["max(Year)"]
print(f"Year: {highest_year}")

highest_month = df.filter(col("Year") == highest_year).select(max("Month")).first()["max(Month)"]
print(f"Month: {highest_month}")

highest_day = df.filter(col("Year") == highest_year).filter(col("Month") == highest_month).select(max("Day")).first()["max(Day)"]
print(f"Day: {highest_day}")

highest_datetime = df.filter(col("Year") == highest_year).filter(col("Month") == highest_month).filter(col("Day") == highest_day).select(max("Time")).first()["max(Time)"]
print(f"Datetime: {highest_datetime}")

from_datetime = datetime(highest_datetime.year, highest_datetime.month, highest_datetime.day, highest_datetime.hour)
to_datetime = from_datetime + timedelta(hours=1)

df = df.filter(col("Time") >= from_datetime).filter(col("Time") < to_datetime)

print(f"Base data from {from_datetime} to {to_datetime} fetched, which contains {df.count()} entries")
print(f"We will add {add_days} days worth of data from {to_datetime} to {to_datetime + timedelta(days=add_days)}")

add_hours = add_days * 24

hours_per_month = 720

print(f"{df.count() * add_hours} entries will be appended to the delta lake")
print(f"Started at {datetime.now()}")

the_df = None

for i in range(1, add_hours + 1):
  
  interval_string = "INTERVAL {hours} HOURS".format(hours = i)
  
  new_df = df \
    .withColumn("Time", col("Time") + expr(interval_string)) \
    .withColumn("Year", year(col("Time"))) \
    .withColumn("Month", month(col("Time"))) \
    .withColumn("Day", dayofmonth(col("Time"))) \
    .withColumn("Quantity", lit(decimal.Decimal(str(decimal.Decimal(random.randrange(1, 99))) + "." + str(decimal.Decimal(random.randrange(100, 999))))).cast(DecimalType(18, 3)))
  
  if the_df is None:
    the_df = new_df
  else:
    the_df = the_df.union(new_df)
    
  if i % hours_per_month == 0:
    min_time, max_time = the_df.select(min("Time"), max("Time")).first()
    print("----------------------------------------------")
    print(f"{datetime.now()}")
    print(f"Saving entries from {min_time} to {max_time}")
    print(f"Number of entries: {the_df.count()}")
  
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
    
    the_df = None
  
min_time, max_time = the_df.select(min("Time"), max("Time")).first()
print("----------------------------------------------")
print(f"{datetime.now()}")
print(f"Saving entries from {min_time} to {max_time}")
print(f"Number of entries: {the_df.count()}")
  
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
print("Total duration: {seconds} seconds".format(seconds = end-start) )


# %% Set auto optimization

#Set property autoOptimize.optimizeWrite and autoOptimize.autoCompact on delta table

spark.sql("ALTER TABLE delta.`abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/` SET TBLPROPERTIES (delta.autoOptimize.optimizeWrite = true, delta.autoOptimize.autoCompact = true)")


# %% Manual optimize delta table

table_name = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
spark.sql("OPTIMIZE delta.`" + table_name + "` WHERE Year = 2013 AND Month = 1 AND Day = 1")


# %% Create dataframe containing metering points registered as grid loss and system correction based on test data

from pyspark.sql.window import Window

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source)

#create dataframe containing metering points to be registered as grid loss metering points
#one metering point from each grid area is selected
gl_window = Window.partitionBy("MeteringGridArea_Domain_mRID").orderBy("MarketEvaluationPointType")
gl_group_df = df.filter(col("MarketEvaluationPointType") == "E17").filter(col("SettlementMethod") == "D01").withColumn("rank", row_number().over(gl_window))
gl_df = gl_group_df.where(col("rank") == 1)


gl_df_selections = gl_df.select("MarketEvaluationPoint_mRID", \
                                lit("2010-01-01").alias("ValidFrom"), \
                                lit("null").alias("ValidTo"), \
                                "MeterReadingPeriodicity", \
                                lit("D03").alias("MeteringMethod"), \
                                "MeteringGridArea_Domain_mRID", \
                                "ConnectionState", \
                                "EnergySupplier_MarketParticipant_mRID", \
                                "BalanceResponsibleParty_MarketParticipant_mRID", \
                                "InMeteringGridArea_Domain_mRID", \
                                "OutMeteringGridArea_Domain_mRID", \
                                "MarketEvaluationPointType", \
                                "SettlementMethod", \
                                lit(False).alias("IsGridLoss"), \
                                lit(True).alias("IsSystemCorrection"))


#create dataframe containing metering points to be registered as system correction metering points
#one metering point from each grid area is selected
sc_window = Window.partitionBy("MeteringGridArea_Domain_mRID").orderBy("MarketEvaluationPointType")
sc_group_df = df.filter(col("MarketEvaluationPointType") == "E18").withColumn("rank", row_number().over(sc_window))
sc_df_selections = sc_group_df.where(col("rank") == 1).select("MarketEvaluationPoint_mRID", \
                                                   lit("2010-01-01").alias("ValidFrom"), \
                                                   lit("null").alias("ValidTo"), \
                                                   "MeterReadingPeriodicity", \
                                                   lit("D03").alias("MeteringMethod"), \
                                                   "MeteringGridArea_Domain_mRID", \
                                                   "ConnectionState", \
                                                   "EnergySupplier_MarketParticipant_mRID", \
                                                   "BalanceResponsibleParty_MarketParticipant_mRID", \
                                                   "InMeteringGridArea_Domain_mRID", \
                                                   "OutMeteringGridArea_Domain_mRID", \
                                                   "MarketEvaluationPointType", \
                                                   "SettlementMethod", \
                                                   lit(False).alias("IsGridLoss"), \
                                                   lit(True).alias("IsSystemCorrection"))

#union dataframes for grid loss and system correction to reside in same dataframe
gl_sc_df = gl_df_selections.union(sc_df_selections)

gl_sc_df = gl_sc_df \
  .withColumn("ValidFromDateTime", to_timestamp(col("ValidFrom"))) \
  .withColumn("ValidToDateTime", to_timestamp(col("ValidTo"))) \
  .drop("ValidFrom") \
  .drop("ValidTo") \
  .withColumnRenamed("ValidFromDateTime", "ValidFrom") \
  .withColumnRenamed("ValidToDateTime", "ValidTo")

save_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/grid-loss-sys-cor/"

#save grid loss and system corrections in its own delta lake
gl_sc_df.write.format("delta").mode("overwrite").save(source)

gl_sc_df.display()


# %% Read excel validation data from csv
#read csv data containing information created for excel validation

excel_data_csv_source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/test-data-base/excel-validation-data-with-quantity.csv"
print(excel_data_csv_source)

excel_csv_df = spark.read.format('csv').options(inferSchema = "true", delimiter=";", header="true").load(excel_data_csv_source)
excel_csv_df.display()


# %% Insert one hour of excel test data based on excel test csv data

#Create excel validation dataframe based on csv
#create dataframe with mock data based on csv file and insert into delta lake for given period
#This block does not rely on performance of spark, so the recommended period is 1 hour, not less and not more

from delta.tables import *
import random
import decimal
from datetime import datetime, timedelta, time
import time

period_from = datetime(2020, 1, 1, 0)
period_to = datetime(2020, 1, 1, 1)

print(period_from)
print(period_to)

output_delta_lake_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/excel-test-data/"

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
csv_count = excel_csv_df.count()
qualities = ["E01", "56", "D01"]
grid_areas = excel_csv_df.select(col("GridArea")).rdd.flatMap(lambda x: x).distinct().collect()

incrementation = 10

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
      row = excel_csv_df.collect()[i]      
      
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
              decimal.Decimal(row["Quantity"]), #Quantity
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


# %% Insert days of data based on excel test data

#Insert x number of days of excel test data
#Inserts X days worth of data based on data for the latest hour in the delta lake

import time
from datetime import datetime, timedelta
import decimal
import random

add_days = 7

period_from = datetime(2020, 1, 1, 0)
period_to = datetime(2020, 1, 1, 1)

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/excel-test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source) \
  .where("Year = 2020 AND Month = 1 AND Day = 1")

df = df.filter(col("Time") >= period_from).filter(col("Time") <= period_to)

add_hours = add_days * 24

hours_per_month = 720

print(f"{df.count() * add_hours} entries will be appended to the delta lake")
print(f"Started at {datetime.now()}")

the_df = None

for i in range(1, add_hours + 1):
  
  interval_string = "INTERVAL {hours} HOURS".format(hours = i)
  
  new_df = df \
    .withColumn("Time", col("Time") + expr(interval_string)) \
    .withColumn("Year", year(col("Time"))) \
    .withColumn("Month", month(col("Time"))) \
    .withColumn("Day", dayofmonth(col("Time")))
  
  if the_df is None:
    the_df = new_df
  else:
    the_df = the_df.union(new_df)
    
  if i % hours_per_month == 0:
    min_time, max_time = the_df.select(min("Time"), max("Time")).first()
    print("----------------------------------------------")
    print(f"{datetime.now()}")
    print(f"Saving entries from {min_time} to {max_time}")
    print(f"Number of entries: {the_df.count()}")
  
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
    
    the_df = None
  
min_time, max_time = the_df.select(min("Time"), max("Time")).first()
print("----------------------------------------------")
print(f"{datetime.now()}")
print(f"Saving entries from {min_time} to {max_time}")
print(f"Number of entries: {the_df.count()}")
  
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
print("Total duration: {seconds} seconds".format(seconds = end-start) )


# %% Create grid loss and system correction data in delta lake based on excel test data

#Add grid loss and system correction dataframe for excel test data

from pyspark.sql.window import Window

source = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/excel-test-data/"
df = spark \
  .read \
  .format("delta") \
  .load(source)

#create dataframe containing metering points to be registered as grid loss metering points
#one metering point from each grid area is selected
gl_window = Window.partitionBy("MeteringGridArea_Domain_mRID").orderBy("MarketEvaluationPointType")
gl_group_df = df.filter(col("MarketEvaluationPointType") == "E17").filter(col("SettlementMethod") == "D01").withColumn("rank", row_number().over(gl_window))
gl_df = gl_group_df.where(col("rank") == 1)


gl_df_selections = gl_df.select("MarketEvaluationPoint_mRID", \
                                lit("2010-01-01").alias("ValidFrom"), \
                                lit("null").alias("ValidTo"), \
                                "MeterReadingPeriodicity", \
                                lit("D03").alias("MeteringMethod"), \
                                "MeteringGridArea_Domain_mRID", \
                                "ConnectionState", \
                                "EnergySupplier_MarketParticipant_mRID", \
                                "BalanceResponsibleParty_MarketParticipant_mRID", \
                                "InMeteringGridArea_Domain_mRID", \
                                "OutMeteringGridArea_Domain_mRID", \
                                "MarketEvaluationPointType", \
                                "SettlementMethod", \
                                lit(True).alias("IsGridLoss"), \
                                lit(False).alias("IsSystemCorrection"))


#create dataframe containing metering points to be registered as system correction metering points
#one metering point from each grid area is selected
sc_window = Window.partitionBy("MeteringGridArea_Domain_mRID").orderBy("MarketEvaluationPointType")
sc_group_df = df.filter(col("MarketEvaluationPointType") == "E18").withColumn("rank", row_number().over(sc_window))
sc_df_selections = sc_group_df.where(col("rank") == 1).select("MarketEvaluationPoint_mRID", \
                                                   lit("2010-01-01").alias("ValidFrom"), \
                                                   lit("null").alias("ValidTo"), \
                                                   "MeterReadingPeriodicity", \
                                                   lit("D03").alias("MeteringMethod"), \
                                                   "MeteringGridArea_Domain_mRID", \
                                                   "ConnectionState", \
                                                   "EnergySupplier_MarketParticipant_mRID", \
                                                   "BalanceResponsibleParty_MarketParticipant_mRID", \
                                                   "InMeteringGridArea_Domain_mRID", \
                                                   "OutMeteringGridArea_Domain_mRID", \
                                                   "MarketEvaluationPointType", \
                                                   "SettlementMethod", \
                                                   lit(False).alias("IsGridLoss"), \
                                                   lit(True).alias("IsSystemCorrection"))

#union dataframes for grid loss and system correction to reside in same dataframe
gl_sc_df = gl_df_selections.union(sc_df_selections)

gl_sc_df = gl_sc_df \
  .withColumn("ValidFromDateTime", to_timestamp(col("ValidFrom"))) \
  .withColumn("ValidToDateTime", to_timestamp(col("ValidTo"))) \
  .drop("ValidFrom") \
  .drop("ValidTo") \
  .withColumnRenamed("ValidFromDateTime", "ValidFrom") \
  .withColumnRenamed("ValidToDateTime", "ValidTo")

save_path = "abfss://" + containerName + "@" + storage_account_name + ".dfs.core.windows.net/delta/excel-grid-loss-sys-cor/"

#save grid loss and system corrections in its own delta lake
gl_sc_df.write.format("delta").mode("overwrite").save(save_path)

gl_sc_df.display()