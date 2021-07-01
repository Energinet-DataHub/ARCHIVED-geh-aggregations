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
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb
{
    /// <summary>
    /// An interface to the location where the master data is stored
    /// </summary>
    public interface IMasterDataStorage
    {
        /// <summary>
        /// Write a meteringPoint to storage
        /// </summary>
        /// <param name="meteringPoint"></param>
        /// <returns>Task</returns>
        Task WriteMeteringPointAsync(MeteringPoint meteringPoint);

        /// <summary>
        /// Write multiple meteringPoints to the storage
        /// </summary>
        /// <param name="records"></param>
        /// <returns>Task</returns>
        Task WriteMeteringPointsAsync(IAsyncEnumerable<MeteringPoint> records);
    }
}
