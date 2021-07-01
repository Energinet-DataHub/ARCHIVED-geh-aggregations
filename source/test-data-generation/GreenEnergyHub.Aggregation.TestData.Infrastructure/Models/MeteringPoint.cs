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
        public string Id => Guid.NewGuid().ToString();

        public string MeteringPointId { get; set; }

        public string MeteringPointType { get; set; }

        public string MeteringGridArea { get; set; }

        public string ToGridArea { get; set; }

        public string FromGridArea { get; set; }

        public string SettlementMethod { get; set; }

        public string MeteringMethod { get; set; }

        public string MeterReadingPeriodicity { get; set; }

        public string ConnectionState { get; set; }

        public string NetSettlementGroup { get; set; }

        public string Product { get; set; }

        public string QuantityUnit { get; set; }

        public string ParentMeteringPointId { get; set; }

        public string FromDate { get; set; }

        public string ToDate { get; set; }
    }
}
