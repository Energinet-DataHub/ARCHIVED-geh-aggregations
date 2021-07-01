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

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class MeteringPoint
    {
        [Newtonsoft.Json.JsonProperty(PropertyName = "id")]
        public string Id => Guid.NewGuid().ToString();

        [Newtonsoft.Json.JsonProperty(PropertyName = "meteringPointId")]
        public string MeteringPointId { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "meteringPointType")]
        public string MarketEvaluationPointType { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "meteringGridArea")]
        public string GridArea { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "settlementMethod")]
        public string SettlementMethod { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "meterReadingPeriodicity")]
        public string Resolution { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "connectionState")]
        public string ConnectionState { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "fromDate")]
        public string FromDate { get; set; }

        [Newtonsoft.Json.JsonProperty(PropertyName = "toDate")]
        public string ToDate { get; set; }
    }
}
