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
    /// <summary>
    /// This service provides information on grid loss and system correction metering point in a grid area
    /// </summary>
    public interface ISpecialMeteringPointsService
    {
        /// <summary>
        /// Return the supplier id that owns the grid loss metering point in the provided grid area
        /// </summary>
        /// <param name="gridArea"></param>
        /// <param name="when">For what time would we like the metering point</param>
        /// <returns>Supplier id</returns>
        public string GridLossOwner(string gridArea, NodaTime.Instant when);

        /// <summary>
        /// Return the supplier id that owns the system correction metering point in the provided grid area
        /// </summary>
        /// <param name="gridArea"></param>
        /// <param name="when">For what time would we like the metering point</param>
        /// <returns>Supplier id</returns>
        public string SystemCorrectionOwner(string gridArea, NodaTime.Instant when);
    }
}
