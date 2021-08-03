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

from argparse import Namespace
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col
import dateutil.parser
from typing import List


def filter_on_date(df: DataFrame, from_date_col: str, to_date_col: str, args: Namespace) -> DataFrame:
    beginning_date_time = dateutil.parser.parse(args.beginning_date_time)
    end_date_time = dateutil.parser.parse(args.end_date_time)

    return df.filter(col(from_date_col).cast("long") >= beginning_date_time.timestamp()).filter(col(to_date_col).cast("long") < end_date_time.timestamp())


def filter_on_grid_areas(df: DataFrame, grid_area_col: str, grid_areas: List[str]) -> DataFrame:
    if grid_areas is not None and len(grid_areas):
        return df.filter(col(grid_area_col).isin(grid_areas))
    return df


def time_series_where_date_condition(args: Namespace) -> str:
    from_date = dateutil.parser.parse(args.beginning_date_time)
    to_date = dateutil.parser.parse(args.end_date_time)

    from_condition = f"Year >= {from_date.year} AND Month >= {from_date.month} AND Day >= {from_date.day}"
    to_condition = f"Year <= {to_date.year} AND Month <= {to_date.month} AND Day <= {to_date.day}"

    return f"{from_condition} AND {to_condition}"
