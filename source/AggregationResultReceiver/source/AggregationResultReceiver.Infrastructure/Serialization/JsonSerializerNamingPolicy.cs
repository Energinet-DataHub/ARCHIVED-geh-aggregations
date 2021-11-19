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

using System;
using System.Text.Json;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Serialization
{
    public class JsonSerializerNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name switch
            {
                "JobId" => "job_id",
                "SnapshotId" => "snapshot_id",
                "ProcessType" => "process_type",
                "ProcessVariant" => "process_variant",
                "Resolution" => "resolution",
                "Results" => "results",
                "FromDate" => "from_date",
                "ToDate" => "to_date",
                "ResultId" => "result_id",
                "ResultPath" => "result_path",
                "Grouping" => "grouping",
                "ResultName" => "result_name",
                "Quality" => "quality",
                "InGridArea" => "in_grid_area",
                "OutGridArea" => "out_grid_area",
                "BalanceResponsibleId" => "balance_responsible_id",
                "EnergySupplierId" => "energy_supplier_id",
                "StartDatetime" => "start_datetime",
                "EndDatetime" => "end_datetime",
                "SumQuantity" => "sum_quantity",
                "MeteringPointType" => "metering_point_type",
                "SettlementMethod" => "settlement_method",
                _ => throw new ArgumentOutOfRangeException(nameof(name), name, "Could not convert property name.")
            };
        }
    }
}
