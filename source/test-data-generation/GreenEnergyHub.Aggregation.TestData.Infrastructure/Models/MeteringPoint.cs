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
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class MeteringPoint : IStoragebleObject
    {
        private string _toDate;
        private string _fromDate;

        [JsonProperty(PropertyName = "id")]
        public static string Id => Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "metering_point_id")]
        public string MeteringPointId { get; set; }

        [JsonProperty(PropertyName = "metering_point_type")]
        public string MeteringPointType { get; set; }

        [JsonProperty(PropertyName = "settlement_method")]
        public string SettlementMethod { get; set; }

        [JsonProperty(PropertyName = "grid_area")]
        public string GridArea { get; set; }

        [JsonProperty(PropertyName = "connection_state")]
        public string ConnectionState { get; set; }

        [JsonProperty(PropertyName = "resolution")]
        public string Resolution { get; set; }

        [JsonProperty(PropertyName = "in_grid_area")]
        public string InGridArea { get; set; }

        [JsonProperty(PropertyName = "out_grid_area")]
        public string OutGridArea { get; set; }

        [JsonProperty(PropertyName = "metering_method")]
        public string MeteringMethod { get; set; }

        [JsonProperty(PropertyName = "net_settlement_group")]
        public string NetSettlementGroup { get; set; }

        [JsonProperty(PropertyName = "parent_metering_point_id")]
        public string ParentMeteringPointId { get; set; }

        [JsonProperty(PropertyName = "unit")]
        public string Unit { get; set; }

        [JsonProperty(PropertyName = "product")]
        public string Product { get; set; }

        [JsonProperty(PropertyName = "from_date")]
        public string FromDate
        {
            get => _fromDate.ToISO8601DateTimeString();
            set => _fromDate = value;
        }

        [JsonProperty(PropertyName = "to_date")]
        public string ToDate
        {
            get => _toDate.ToISO8601DateTimeString();
            set => _toDate = value;
        }
    }
}
