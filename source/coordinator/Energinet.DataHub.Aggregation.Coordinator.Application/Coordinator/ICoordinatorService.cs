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
using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata.Enums;
using NodaTime;

namespace Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator
{
    /// <summary>
    /// This service implements the core aggregation coordination functionality
    /// </summary>
    public interface ICoordinatorService
    {
        /// <summary>
        /// Start an aggregation job
        /// </summary>
        /// <param name="jobId">The unique id provided by calling entity</param>
        /// <param name="snapshotId">The unique snapshotId</param>
        /// <param name="processType"></param>
        /// <param name="isSimulation"></param>
        /// <param name="owner">Owner the job</param>
        /// <param name="resolution">What resolution should we run this calculation with</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        Task StartAggregationJobAsync(
            Guid jobId,
            Guid snapshotId,
            JobProcessTypeEnum processType,
            bool isSimulation,
            string owner,
            ResolutionEnum resolution,
            CancellationToken cancellationToken);

        //TODO: This needs to be refactored to correspond to changes made in #402 /LKI 2021-10-13
        ///// <summary>
        ///// Handles the aggregation results coming back from databricks
        ///// </summary>
        ///// <param name="inputPath"></param>
        ///// <param name="jobId"></param>
        ///// <param name="cancellationToken"></param>
        ///// <returns>Async task</returns>
        //Task HandleResultAsync(string inputPath, string jobId, CancellationToken cancellationToken);

        /// <summary>
        /// Start a wholesale job
        /// </summary>
        /// <param name="jobId">The unique id provided by calling entity</param>
        /// <param name="snapshotId">The unique snapshotId</param>
        /// <param name="processType">Process type of job</param>
        /// <param name="isSimulation">If simulation is true, then it is not considered to be production job</param>
        /// <param name="owner">Owner of the job</param>
        /// <param name="processVariant">Process variant of the job</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Async task</returns>
        Task StartWholesaleJobAsync(
            Guid jobId,
            Guid snapshotId,
            JobProcessTypeEnum processType,
            bool isSimulation,
            string owner,
            JobProcessVariantEnum processVariant,
            CancellationToken cancellationToken);

        /// <summary>
        /// Start a data preparation job
        /// </summary>
        /// <returns>Async task</returns>
        Task StartDataPreparationJobAsync(
            Guid jobId,
            Guid snapshotId,
            Instant fromDate,
            Instant toDate,
            string gridAreas,
            CancellationToken cancellationToken);

        /// <summary>
        /// Update snapshot path
        /// </summary>
        /// <param name="snapshotId"></param>
        /// <param name="path"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateSnapshotPathAsync(
            Guid snapshotId,
            string path);

        /// <summary>
        /// Get JobMetaData object
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns>JobMetaData</returns>
        Task<Job> GetJobAsync(Guid jobId);
    }
}
