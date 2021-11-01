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

from geh_stream.codelists import Colname
from pyspark.sql.types import DecimalType, IntegerType, StructType, StructField, StringType, TimestampType


aggregation_result_schema = StructType([
    StructField(Colname.job_id, StringType(), False),
    StructField(Colname.snapshot_id, StringType(), False),
    StructField(Colname.result_id, StringType(), False),
    StructField(Colname.result_name, StringType(), False),
    StructField(Colname.grid_area, StringType(), False),
    StructField(Colname.in_grid_area, StringType(), True),
    StructField(Colname.out_grid_area, StringType(), True),
    StructField(Colname.balance_responsible_id, StringType(), True),
    StructField(Colname.energy_supplier_id, StringType(), True),
    StructField(Colname.start_datetime, TimestampType(), False),
    StructField(Colname.end_datetime, TimestampType(), False),
    StructField(Colname.resolution, IntegerType(), False),
    StructField(Colname.sum_quantity, DecimalType(18, 3), False),
    StructField(Colname.quality, IntegerType(), False),
    StructField(Colname.metering_point_type, IntegerType(), False),
    StructField(Colname.settlement_method, IntegerType(), False),
])
