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
using NodaTime;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class Charge : IStoragebleObject
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string Id => Guid.NewGuid().ToString();

        public string ChargeId { get; set; }

        public string ChargeType { get; set; }

        public string ChargeOwner { get; set; }

        public string Resolution { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "TaxIndicator")]
        public string ChargeTax { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "StartDate")]
        public string FromDate { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "EndDate")]
        public string ToDate { get; set; }
    }
}
