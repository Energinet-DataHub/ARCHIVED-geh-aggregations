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

using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.Types;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// This service implements the core aggregation coordination functionality
    /// </summary>
    public interface ICoordinatorService
    {
        /// <summary>
        /// Start an aggregation job
        /// </summary>
        /// <param name="processType"></param>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="resultId"></param>
        /// <param name="persist">Should we persist the source as a snapshot?</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Async task</returns>
        Task StartAggregationJobAsync(ProcessType processType, Instant beginTime, Instant endTime, bool persist, CancellationToken cancellationToken);

        /// <summary>
        /// Handles the aggregation results coming back from databricks
        /// </summary>
        /// <param name="inputPath"></param>
        /// <param name="resultId"></param>
        /// <param name="processType"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Async task</returns>
        Task HandleResultAsync(string inputPath, string resultId, string processType, Instant startTime, Instant endTime, CancellationToken cancellationToken);
    }
}
