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
    public class Charge : IStoragebleObject
    {
        private string _fromDate;
        private string _toDate;

        [JsonProperty(PropertyName = "id")]
        public static string Id => Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "charge_id")]
        public string ChargeId { get; set; }

        [JsonProperty(PropertyName = "charge_type")]
        public string ChargeType { get; set; }

        [JsonProperty(PropertyName = "charge_owner")]
        public string ChargeOwner { get; set; }

        [JsonProperty(PropertyName = "resolution")]
        public string Resolution { get; set; }

        [JsonProperty(PropertyName = "charge_tax")]
        public string ChargeTax { get; set; }

        [JsonProperty(PropertyName = "currency")]
        public string Currency { get; set; }

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
