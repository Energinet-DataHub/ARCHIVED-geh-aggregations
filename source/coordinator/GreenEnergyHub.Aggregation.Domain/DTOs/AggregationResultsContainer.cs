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

using System.Collections.Generic;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class AggregationResultsContainer
    {
        public List<string> FlexConsumption { get; set; }

        public List<string> GridAreaGridLoss { get;  set; }

        public List<string> GridAreaSystemCorrection { get;  set; }

        public List<string> GridLoss { get;  set; }

        public List<string> HourlyConsumption { get;  set; }

        public List<string> HourlyProduction { get;  set; }

        public List<string> HourlySettledConsumption { get;  set; }

        public List<string> NetExchange { get;  set; }

        public List<string> AdjustedFlexConsumption { get;  set; }

        public List<string> AdjustedHourlyProduction { get;  set; }
    }
}
