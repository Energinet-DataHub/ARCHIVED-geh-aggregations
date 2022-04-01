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
from enum import Enum


# Keep aligned with the equivalent class in C#:
# https://github.com/Energinet-DataHub/geh-aggregations/blob/main/source/coordinator/Energinet.DataHub.Aggregation.Coordinator.Domain/Types/Quality.cs
class Quality(Enum):
    calculated = "D01"
    revised = "36"
    estimated = "56"
    as_read = "E01"
    quantity_missing = "QM"
