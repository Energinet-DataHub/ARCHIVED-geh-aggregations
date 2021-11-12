// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Newtonsoft.Json;

namespace Energinet.DataHub.ResultReceiver.Domain
{
#pragma warning disable SA1313
    public record ResultData(
        [property:JsonProperty("job_id")]string JobId,
        [property:JsonProperty("snapshot_id")]string SnapshotId,
        [property:JsonProperty("result_id")]string ResultId,
        [property:JsonProperty("result_name")]string ResultName,
        [property:JsonProperty("grid_area")]string GridArea,
        [property:JsonProperty("in_grid_area")]string InGridArea,
        [property:JsonProperty("out_grid_area")]string OutGridArea,
        [property:JsonProperty("balance_responsible_id")]string BalanceResponsibleId,
        [property:JsonProperty("energy_supplier_id")]string EnergySupplierId,
        [property:JsonProperty("start_datetime")]string StartDateTime,
        [property:JsonProperty("end_datetime")]string EndDateTime,
        [property:JsonProperty("resolution")]string Resolution,
        [property:JsonProperty("sum_quantity")]decimal SumQuantity,
        [property:JsonProperty("quality")]string Quality,
        [property:JsonProperty("metering_point_type")]string MeteringPointType,
        [property:JsonProperty("settlement_method")]string SettlementMethod);
#pragma warning restore SA1313
}
