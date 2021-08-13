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
from pyspark.sql.functions import col, expr, year, month, lit, last_day, dayofmonth, explode
from pyspark.sql.types import IntegerType
from geh_stream.codelists import Colname
from geh_stream.shared.data_exporter import export_to_csv
from calendar import monthrange


def calculate_daily_subscription_price(charges: DataFrame, charge_links: DataFrame, charge_prices: DataFrame):
    # Rename
    # charges = charges.withColumnRenamed(col(Colname.from_date), "charge_from_date") \
    #     .withColumnRenamed(col(Colname.to_date), "charge_to_date")      
    # charge_links = charge_links.withColumnRenamed(col(Colname.from_date), "charge_link_from_date") \
    #     .withColumnRenamed(col(Colname.to_date), "charge_link_to_date")

    # Only look at subcriptions D01
    subscription_charge_type = "D01"
    subscription_charges = charges.filter(col(Colname.charge_type) == subscription_charge_type)
    # subscription_charges.show()
    # charge_links.show()
    # charge_prices.show()

    # Join charges and charge_prices
    charges_with_prices = charge_prices \
        .join(subscription_charges, ["charge_key"])
    # join charges_with_charge_prices with charge_links
    # charges_with_prices_and_charge_links = charges_with_prices \
    #     .join(charge_links, ["charge_id"], "left")

    df = charges_with_prices.withColumn("price_per_day", (col(Colname.charge_price)/dayofmonth(last_day(col(Colname.time)))))

    new_df = df.withColumn("date", explode(expr('sequence(from_date, to_date, interval 1 day)')))
    new_df.show(100, False)
    charge_links_exploded = charge_links.withColumn("date", explode(expr('sequence(from_date, to_date, interval 1 day)')))
    charge_links_exploded.show(100, False)
    df_with_link_ex = new_df.join(charge_links_exploded, ["charge_key", "date"])
    df_with_link_ex.show(1000, False)

# STEP 1
    # df = charges_with_prices.withColumn("days_in_month", lit(f"{monthrange(year(col(Colname.time)), month(col(Colname.time)))[1]}").cast(IntegerType()))
    # df = charges_with_prices.select((col(Colname.charge_price)/monthrange(year(col(Colname.time)), month(col(Colname.time)))[1]).alias("price_per_day"))
    # df = charges_with_prices.withColumn("test", (monthrange(year(col("time")), month(col("time")))[1]))
    # charges_with_prices.show()
    # number_of_days = monthrange(2012, 2)[1]
    # print(number_of_days)

    # time_series_with_metering_point_and_charges = time_series_with_metering_point \
    #     .join(charges_with_prices_and_links, ["metering_point_id", "from_date", "to_date"])
