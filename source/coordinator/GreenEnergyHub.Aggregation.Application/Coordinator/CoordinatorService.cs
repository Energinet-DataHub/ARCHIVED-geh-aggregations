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
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly ILogger<CoordinatorService> _logger;
        private readonly IMetaDataDataAccess _metaDataDataAccess;
        private readonly ITriggerBaseArguments _triggerBaseArguments;
        private readonly ICalculationEngine _calculationEngine;

        public CoordinatorService(
            CoordinatorSettings coordinatorSettings,
            ILogger<CoordinatorService> logger,
            IMetaDataDataAccess metaDataDataAccess,
            ITriggerBaseArguments triggerBaseArguments,
            ICalculationEngine calculationEngine)
        {
            _metaDataDataAccess = metaDataDataAccess;
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
                await _metaDataDataAccess.CreateSnapshotAsync(snapshot).ConfigureAwait(false);

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
                await _metaDataDataAccess.UpdateSnapshotPathAsync(snapshotId, path).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to update snapshot path {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task<Job> GetJobAsync(Guid jobId)
        {
            return await _metaDataDataAccess.GetJobAsync(jobId).ConfigureAwait(false);
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
                var jobType = JobTypeEnum.Aggregation;

                var job = await CreateJobAndJobResultsAsync(jobId, snapshotId, jobType, owner, processType, isSimulation, resolution).ConfigureAwait(false);

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
                var jobType = JobTypeEnum.Wholesale;

                var parameters = _triggerBaseArguments.GetTriggerWholesaleArguments(processType, jobId, snapshotId);

                var job = await CreateJobAndJobResultsAsync(jobId, snapshotId, jobType, owner, processType, isSimulation).ConfigureAwait(false);

                await _calculationEngine.CreateAndRunCalculationJobAsync(job, parameters, _coordinatorSettings.WholesalePythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start wholesale job {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        private static string GetPath(Result result, Job job)
        {
            return $"Results/{job.Id}/{result.Id}";
        }

        private async Task<Job> CreateJobAndJobResultsAsync(Guid jobId, Guid snapshotId, JobTypeEnum jobType, string owner, JobProcessTypeEnum? processType = null, bool isSimulation = false, ResolutionEnum? resolution = null)
        {
            var job = new Job(jobId, snapshotId, jobType, JobStateEnum.Pending, owner, resolution, processType, isSimulation);
            await _metaDataDataAccess.CreateJobAsync(job).ConfigureAwait(false);

            var results = await _metaDataDataAccess.GetResultsByTypeAsync(job.Type).ConfigureAwait(false);

            foreach (var result in results)
            {
                var jobResult = new JobResult(job.Id, result.Id, GetPath(result, job), ResultStateEnum.NotCompleted)
                {
                    Result = result,
                };

                await _metaDataDataAccess.CreateJobResultAsync(jobResult).ConfigureAwait(false);
                job.JobResults.Add(jobResult);
            }

            return job;
        }
    }
}
