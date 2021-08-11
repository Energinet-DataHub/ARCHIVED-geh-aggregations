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
using System.Text.Json.Serialization;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class CombinedGridLossDto : AggregationResultDto
    {
        [JsonPropertyName("ConnectionState")]
        public string ConnectionState { get; set; } = string.Empty;

        [JsonPropertyName("IsGridLoss")]
        public bool IsGridLoss { get; set; } = false;

        [JsonPropertyName("IsSystemCorrection")]
        public bool IsSystemCorrection { get; set; } = false;

        [JsonPropertyName("MarketEvaluationPointType")]
        public string MarketEvaluationPointType { get; set; } = string.Empty;

        [JsonPropertyName("MarketEvaluationPoint_mRID")]
        public string MarketEvaluationPointmRID { get; set; } = string.Empty;

        [JsonPropertyName("MeterReadingPeriodicity")]
        public string MeterReadingPeriodicity { get; set; } = string.Empty;

        [JsonPropertyName("MeteringMethod")]
        public string MeteringMethod { get; set; } = string.Empty;

        [JsonPropertyName("SettlementMethod")]
        public string SettlementMethod { get; set; } = string.Empty;

        [JsonPropertyName("ValidFrom")]
        public DateTimeOffset ValidFrom { get; set; } = DateTimeOffset.Now;

        [JsonPropertyName("added_system_correction")]
        public double AddedSystemCorrection { get; set; } = default;
    }
}
