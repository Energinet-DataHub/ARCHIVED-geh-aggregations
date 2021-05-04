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

using System.Text.Json.Serialization;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class NetExchange
    {
        [JsonPropertyName("MeteringGridArea_Domain_mRID")]
        public string MeteringGridAreaDomainmRID { get; set; }

        [JsonPropertyName("time_window")]
        public TimeWindow TimeWindow { get; set; }

        [JsonPropertyName("in_sum")]
        public double InSum { get; set; }

        [JsonPropertyName("out_sum")]
        public double OutSum { get; set; }

        [JsonPropertyName("result")]
        public double Result { get; set; }
    }
}
