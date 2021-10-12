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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Application.Utilities;
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
        private readonly IPersistedDataService _persistedDataService;
        private readonly IInputProcessor _inputProcessor;
        private readonly IMetaDataDataAccess _metaDataDataAccess;
        private readonly ITriggerBaseArguments _triggerBaseArguments;
        private readonly ICalculationEngine _calculationEngine;

        public CoordinatorService(
            CoordinatorSettings coordinatorSettings,
            ILogger<CoordinatorService> logger,
            IPersistedDataService persistedDataService,
            IInputProcessor inputProcessor,
            IMetaDataDataAccess metaDataDataAccess,
            ITriggerBaseArguments triggerBaseArguments,
            ICalculationEngine calculationEngine)
        {
            _persistedDataService = persistedDataService;
            _inputProcessor = inputProcessor;
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
                var processType = JobProcessTypeEnum.DataPreparation;
                var jobType = JobTypeEnum.Live;
                var owner = "system";

                var parameters = _triggerBaseArguments.GetTriggerDataPreparationArguments(fromDate, toDate, gridAreas, processType, jobId, snapshotId);

                var snapshot = new Snapshot(snapshotId, fromDate, toDate, gridAreas);
                await _metaDataDataAccess.CreateSnapshotAsync(snapshot).ConfigureAwait(false);

                var jobMetadata = new JobMetadata(jobId, snapshotId, jobType, processType, JobStateEnum.Created, owner);
                await _metaDataDataAccess.CreateJobAsync(jobMetadata).ConfigureAwait(false);

                await _calculationEngine.CreateAndRunCalculationJobAsync(jobMetadata, processType, parameters, _coordinatorSettings.DataPreparationPythonFile, cancellationToken).ConfigureAwait(false);
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

        public async Task<JobMetadata> GetJobAsync(Guid jobId)
        {
            return await _metaDataDataAccess.GetJobAsync(jobId);
        }

        public async Task StartAggregationJobAsync(
            Guid jobId,
            Guid snapshotId,
            JobTypeEnum jobType,
            string owner,
            string resolution,
            CancellationToken cancellationToken)
        {
            try
            {
                var processType = JobProcessTypeEnum.Aggregation;

                var parameters = _triggerBaseArguments.GetTriggerAggregationArguments(processType, jobId, snapshotId, resolution);

                var jobMetadata = new JobMetadata(jobId, snapshotId, jobType, processType, JobStateEnum.Created, owner);
                await _metaDataDataAccess.CreateJobAsync(jobMetadata).ConfigureAwait(false);

                await _calculationEngine.CreateAndRunCalculationJobAsync(jobMetadata, processType, parameters, _coordinatorSettings.AggregationPythonFile, cancellationToken).ConfigureAwait(false);
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
            JobTypeEnum jobType,
            string owner,
            string processVariant,
            CancellationToken cancellationToken)
        {
            try
            {
                var processType = JobProcessTypeEnum.Wholesale;

                var parameters = _triggerBaseArguments.GetTriggerWholesaleArguments(processType, jobId, snapshotId);

                var jobMetadata = new JobMetadata(jobId, snapshotId, jobType, processType, JobStateEnum.Created, owner, processVariant);
                await _metaDataDataAccess.CreateJobAsync(jobMetadata).ConfigureAwait(false);

                await _calculationEngine.CreateAndRunCalculationJobAsync(jobMetadata, processType, parameters, _coordinatorSettings.WholesalePythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start wholesale jobMetadata {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task HandleResultAsync(string inputPath, string jobId, CancellationToken cancellationToken)
        {
            if (inputPath == null)
            { throw new ArgumentNullException(nameof(inputPath)); }
            if (jobId == null)
            { throw new ArgumentNullException(nameof(jobId)); }

            try
            {
                _logger.LogInformation("Entered HandleResultAsync with {inputPath} {jobId}", inputPath, jobId);

                var target = InputStringParser.ParseJobPath(inputPath);
                var result = new Result(jobId, target, inputPath);
                await _metaDataDataAccess.CreateResultItemAsync(result).ConfigureAwait(false);

                await using var stream = await _persistedDataService.GetBlobStreamAsync(inputPath, cancellationToken).ConfigureAwait(false);

                result.State = ResultStateEnum.StreamCaptured;
                await _metaDataDataAccess.UpdateResultItemAsync(result).ConfigureAwait(false);

                //await _inputProcessor.ProcessInputAsync(target, stream, processType, startTime, endTime, result, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "We encountered an error while handling result {inputPath} {jobId}", inputPath, jobId);
                throw;
            }

            _logger.LogInformation("Message handled {inputPath} {jobId}", inputPath, jobId);
        }
    }
}
