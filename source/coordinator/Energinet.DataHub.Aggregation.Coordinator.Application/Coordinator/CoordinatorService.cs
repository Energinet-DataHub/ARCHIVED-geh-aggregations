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
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator.Interfaces;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata.Enums;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly ILogger<CoordinatorService> _logger;
        private readonly IMetadataDataAccess _metadataDataAccess;
        private readonly ITriggerBaseArguments _triggerBaseArguments;
        private readonly ICalculationEngine _calculationEngine;

        public CoordinatorService(
            CoordinatorSettings coordinatorSettings,
            ILogger<CoordinatorService> logger,
            IMetadataDataAccess metadataDataAccess,
            ITriggerBaseArguments triggerBaseArguments,
            ICalculationEngine calculationEngine)
        {
            _metadataDataAccess = metadataDataAccess;
            _triggerBaseArguments = triggerBaseArguments;
            _calculationEngine = calculationEngine;
            _coordinatorSettings = coordinatorSettings;
            _logger = logger;
        }

        public async Task StartDataPreparationJobAsync(
            Guid jobId,
            Guid snapshotId,
            Instant fromDate,
            Instant toDate,
            string gridAreas,
            CancellationToken cancellationToken)
        {
            try
            {
                var jobType = JobTypeEnum.Preparation;
                var owner = "system";

                var parameters = _triggerBaseArguments.GetTriggerDataPreparationArguments(fromDate, toDate, gridAreas, jobId, snapshotId);

                var snapshot = new Snapshot(snapshotId, fromDate, toDate, gridAreas);
                await _metadataDataAccess.CreateSnapshotAsync(snapshot).ConfigureAwait(false);

                var job = await CreateJobAndJobResultsAsync(jobId, snapshotId, jobType, owner).ConfigureAwait(false);

                await _calculationEngine.CreateAndRunCalculationJobAsync(job, parameters, _coordinatorSettings.DataPreparationPythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start dataPreparationJob {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task UpdateSnapshotPathAsync(Guid snapshotId, string path)
        {
            try
            {
                await _metadataDataAccess.UpdateSnapshotPathAsync(snapshotId, path).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to update snapshot path {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task<Job> GetJobAsync(Guid jobId)
        {
            return await _metadataDataAccess.GetJobAsync(jobId).ConfigureAwait(false);
        }

        public async Task StartAggregationJobAsync(
            Guid jobId,
            Guid snapshotId,
            JobProcessTypeEnum processType,
            bool isSimulation,
            string owner,
            ResolutionEnum resolution,
            CancellationToken cancellationToken)
        {
            try
            {
                var job = await CreateJobAndJobResultsAsync(jobId, snapshotId, JobTypeEnum.Aggregation, owner, processType, isSimulation, resolution).ConfigureAwait(false);

                var parameters = _triggerBaseArguments.GetTriggerAggregationArguments(job);

                await _calculationEngine.CreateAndRunCalculationJobAsync(job, parameters, _coordinatorSettings.AggregationPythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start aggregation jobMetadata {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task StartWholesaleJobAsync(
            Guid jobId,
            Guid snapshotId,
            JobProcessTypeEnum processType,
            bool isSimulation,
            string owner,
            JobProcessVariantEnum processVariant,
            CancellationToken cancellationToken)
        {
            try
            {
                var parameters = _triggerBaseArguments.GetTriggerWholesaleArguments(processType, jobId, snapshotId);

                var job = await CreateJobAndJobResultsAsync(jobId, snapshotId, JobTypeEnum.Wholesale, owner, processType, isSimulation, null, processVariant).ConfigureAwait(false);

                await _calculationEngine.CreateAndRunCalculationJobAsync(job, parameters, _coordinatorSettings.WholesalePythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start wholesale job {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        private async Task<Job> CreateJobAndJobResultsAsync(
            Guid jobId,
            Guid snapshotId,
            JobTypeEnum jobType,
            string owner,
            JobProcessTypeEnum? processType = null,
            bool isSimulation = false,
            ResolutionEnum? resolution = null,
            JobProcessVariantEnum? processVariant = null)
        {
            var job = new Job(jobId, snapshotId, jobType, JobStateEnum.Pending, owner, resolution, processType, isSimulation, processVariant);
            await _metadataDataAccess.CreateJobAsync(job).ConfigureAwait(false);

            var results = await _metadataDataAccess.GetResultsByTypeAsync(job.Type).ConfigureAwait(false);

            foreach (var result in results)
            {
                var path = $"Results/{job.Id}/{result.Id}";

                var jobResult = new JobResult(job.Id, result.Id, path, ResultStateEnum.NotCompleted)
                {
                    Result = result,
                };

                await _metadataDataAccess.CreateJobResultAsync(jobResult).ConfigureAwait(false);
                job.JobResults.Add(jobResult);
            }

            return job;
        }
    }
}
