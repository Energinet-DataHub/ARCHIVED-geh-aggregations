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
    public class ChargePrices : IStoragebleObject
    {
        private string _time;

        [JsonProperty(PropertyName = "id")]
        public static string Id => Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "charge_id")]
        public string ChargeId { get; set; }

        [JsonProperty(PropertyName = "charge_price")]
        public string ChargePrice { get; set; }

        [JsonProperty(PropertyName = "time")]
        public string Time
        {
            get => _time.ToISO8601DateTimeString();
            set => _time = value;
        }
    }
}
