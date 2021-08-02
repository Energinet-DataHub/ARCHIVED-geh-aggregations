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
from datetime import datetime
from pyspark.sql.dataframe import DataFrame
from pyspark.sql.functions import col


def filter_on_date(df: DataFrame, from_date_col: str, to_date_col: str, args: Namespace) -> DataFrame:
    return df.filter(col(from_date_col).cast("long") >= datetime(args.beginning_date_time).timestamp()).filter(col(to_date_col).cast("long") < datetime(args.end_date_time).timestamp())
