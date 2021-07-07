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
using Newtonsoft.Json;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Models
{
    public class SpecialMeteringPoint : IStoragebleObject
    {
        [JsonProperty(PropertyName = "id")]
        public string Id => Guid.NewGuid().ToString();

        [JsonProperty(PropertyName = "metering_point_id")]
        public string MeteringPointID { get; set; }

        [JsonProperty(PropertyName = "energy_supplier_id")]
        public string EnergySupplier { get; set; }

        [JsonProperty(PropertyName = "grid_area")]
        public string GridArea { get; set; }

        [JsonProperty(PropertyName = "is_grid_loss")]
        public string IsGridLoss { get; set; }

        [JsonProperty(PropertyName = "is_system_correction")]
        public string IsSystemCorrection { get; set; }

        [JsonProperty(PropertyName = "from_date")]
        public string FromDate { get; set; }

        [JsonProperty(PropertyName = "to_date")]
        public string ToDate { get; set; }
    }
}
