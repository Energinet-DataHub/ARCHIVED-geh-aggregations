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

namespace GreenEnergyHub.Aggregation.Application.Services
{
    public class SpecialMeteringPointsService : ISpecialMeteringPointsService
    {
        public string GridLossOwner(string gridArea, NodaTime.Instant validTime)
        {
            // TODO implement
            switch (gridArea)
            {
                case "500":
                    return "8510000000006";
                case "501":
                    return "8510000000013";
                case "502":
                    return "8510000000020";
            }

            return "Unknown";
        }

        public string SystemCorrectionOwner(string gridArea, NodaTime.Instant validTime)
        {
            // TODO implement
            switch (gridArea)
            {
                case "500":
                    return "8510000000006";
                case "501":
                    return "8510000000020";
                case "502":
                    return "8510000000013";
            }

            return "Unknown";
        }
    }
}
