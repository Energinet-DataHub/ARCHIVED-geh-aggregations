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

from pyspark.sql.types import StructType, StructField, StringType, TimestampType, BooleanType

grid_loss_sys_corr_schema = StructType([
      StructField("id", StringType(), False),
      StructField("meteringPointId", StringType(), False),
      StructField("meteringGridArea", StringType(), False),
      StructField("isGridLoss", BooleanType(), False),
      StructField("isSystemCorrection", BooleanType(), False),
      StructField("fromDate", TimestampType(), False),
      StructField("toDate", TimestampType(), False)
])
