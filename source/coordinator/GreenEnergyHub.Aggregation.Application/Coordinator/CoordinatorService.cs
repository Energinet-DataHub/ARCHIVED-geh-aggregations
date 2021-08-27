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
using Microsoft.Azure.Databricks.Client;
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

        public CoordinatorService(
            CoordinatorSettings coordinatorSettings,
            ILogger<CoordinatorService> logger,
            IPersistedDataService persistedDataService,
            IInputProcessor inputProcessor,
            IMetaDataDataAccess metaDataDataAccess,
            ITriggerBaseArguments triggerBaseArguments)
        {
            _persistedDataService = persistedDataService;
            _inputProcessor = inputProcessor;
            _metaDataDataAccess = metaDataDataAccess;
            _triggerBaseArguments = triggerBaseArguments;
            _coordinatorSettings = coordinatorSettings;
            _logger = logger;
        }

        public async Task StartAggregationJobAsync(
            Guid jobId,
            JobTypeEnum jobType,
            string jobOwner,
            Instant beginTime,
            Instant endTime,
            bool persist,
            string resolution,
            string gridArea,
            CancellationToken cancellationToken)
        {
            try
            {
                var processType = JobProcessTypeEnum.Aggregation;

                var parameters = _triggerBaseArguments.GetTriggerBaseArguments(beginTime, endTime, gridArea, processType, persist, jobId);
                parameters.Add($"--resolution={resolution}");

                var jobMetadata = new JobMetadata(processType, jobId, new Interval(beginTime, endTime), jobType, jobOwner, gridArea);
                await CreateAndRunDatabricksJobAsync(jobMetadata, processType, parameters, _coordinatorSettings.AggregationPythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start aggregation jobMetadata {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task StartWholesaleJobAsync(
            Guid jobId,
            JobTypeEnum jobType,
            string jobOwner,
            Instant beginTime,
            Instant endTime,
            bool persist,
            string resolution,
            string gridArea,
            string processVariant,
            CancellationToken cancellationToken)
        {
            try
            {
                var processType = JobProcessTypeEnum.Wholesale;

                var parameters = _triggerBaseArguments.GetTriggerBaseArguments(beginTime, endTime, gridArea, processType, persist, jobId);
                parameters.Add($"--cosmos-container-charges={_coordinatorSettings.CosmosContainerCharges}");
                parameters.Add($"--cosmos-container-charge-links={_coordinatorSettings.CosmosContainerChargeLinks}");
                parameters.Add($"--cosmos-container-charge-prices={_coordinatorSettings.CosmosContainerChargePrices}");

                var jobMetadata = new JobMetadata(processType, jobId, new Interval(beginTime, endTime), jobType, jobOwner, gridArea, processVariant);
                await CreateAndRunDatabricksJobAsync(jobMetadata, processType, parameters, _coordinatorSettings.WholesalePythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start wholesale jobMetadata {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task HandleResultAsync(string inputPath, string resultId, string processType, Instant startTime, Instant endTime, CancellationToken cancellationToken)
        {
            if (inputPath == null)
            { throw new ArgumentNullException(nameof(inputPath)); }
            if (resultId == null)
            { throw new ArgumentNullException(nameof(resultId)); }
            if (processType == null)
            { throw new ArgumentNullException(nameof(processType)); }
            if (startTime == null)
            { throw new ArgumentNullException(nameof(startTime)); }
            if (endTime == null)
            { throw new ArgumentNullException(nameof(endTime)); }

            try
            {
                _logger.LogInformation("Entered HandleResultAsync with {inputPath} {resultId} {processType} {startTime} {endTime}", inputPath, resultId, processType, startTime, endTime);

                var target = InputStringParser.ParseJobPath(inputPath);
                var result = new Result(resultId, target, inputPath);
                await _metaDataDataAccess.CreateResultItemAsync(result).ConfigureAwait(false);

                await using var stream = await _persistedDataService.GetBlobStreamAsync(inputPath, cancellationToken).ConfigureAwait(false);

                result.State = ResultStateEnum.StreamCaptured;
                await _metaDataDataAccess.UpdateResultItemAsync(result).ConfigureAwait(false);

                await _inputProcessor.ProcessInputAsync(target, stream, processType, startTime, endTime, result, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "We encountered an error while handling result {inputPath} {resultId} {processType} {startTime} {endTime}", inputPath, resultId, processType, startTime, endTime);
                throw;
            }

            _logger.LogInformation("Message handled {inputPath} {resultId} {processType} {startTime} {endTime}", inputPath, resultId, processType, startTime, endTime);
        }

        private async Task CreateAndRunDatabricksJobAsync(JobMetadata jobMetadata, JobProcessTypeEnum processType, List<string> parameters, string pythonFileName, CancellationToken cancellationToken)
        {
            jobMetadata.State = JobStateEnum.ClusterStartup;
            await _metaDataDataAccess.CreateJobAsync(jobMetadata).ConfigureAwait(false);

            using var client = DatabricksClient.CreateClient(_coordinatorSettings.ConnectionStringDatabricks, _coordinatorSettings.TokenDatabricks);
            var list = await client.Clusters.List(cancellationToken).ConfigureAwait(false);
            var ourCluster = list.Single(c => c.ClusterName == CoordinatorSettings.ClusterName);
            var jobSettings = JobSettings.GetNewNotebookJobSettings(
                $"{processType.GetDescription()} job",
                null,
                null);

            jobMetadata.State = JobStateEnum.ClusterCreated;
            await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);
            if (ourCluster.State == ClusterState.TERMINATED)
            {
                await client.Clusters.Start(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
            }

            await WaitForClusterToBeReadyAsync(ourCluster, jobMetadata, client, _coordinatorSettings.ClusterTimeoutMinutes, cancellationToken).ConfigureAwait(false);

            var runId = await CreateAndStartJobAsync(jobMetadata, parameters, pythonFileName, jobSettings, ourCluster, client, cancellationToken).ConfigureAwait(false);

            var result = await WaitForJobCompletionAsync(client, runId, cancellationToken).ConfigureAwait(false);

            jobMetadata.State = JobStateEnum.Completed;

            if (result == RunResultState.FAILED)
            {
                jobMetadata.State = JobStateEnum.CompletedWithFail;
            }

            jobMetadata.ExecutionEnd = SystemClock.Instance.GetCurrentInstant();

            await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);
        }

        private async Task<RunResultState?> WaitForJobCompletionAsync(
            DatabricksClient client,
            RunIdentifier runId,
            CancellationToken cancellationToken)
        {
            var run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);
            //TODO handle error scenarios (Timeout)
            while (!run.IsCompleted)
            {
                _logger.LogInformation("Waiting for run {runId}", new { runId = runId.RunId });
                Thread.Sleep(2000);
                run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);
            }

            return run.State.ResultState;
        }

        /// <summary>
        /// Starts the job on the running cluster
        /// </summary>
        /// <param name="jobMetadata"></param>
        /// <param name="parameters"></param>
        /// <param name="pythonFileName"></param>
        /// <param name="jobSettings"></param>
        /// <param name="ourCluster"></param>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        private async Task<RunIdentifier> CreateAndStartJobAsync(
            JobMetadata jobMetadata,
            List<string> parameters,
            string pythonFileName,
            JobSettings jobSettings,
            ClusterInfo ourCluster,
            DatabricksClient client,
            CancellationToken cancellationToken)
        {
            jobSettings.SparkPythonTask = new SparkPythonTask
            {
                PythonFile = pythonFileName,
                Parameters = parameters,
            };
            jobSettings.WithExistingCluster(ourCluster.ClusterId);

            // Create new job
            var databricksJobId = await client.Jobs.Create(jobSettings, cancellationToken).ConfigureAwait(false);

            jobMetadata.DatabricksJobId = databricksJobId;
            await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);

            // Start the jobMetadata and retrieve the run id.
            var runId = await client.Jobs.RunNow(databricksJobId, null, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Waiting for run {@RunId}", runId.RunId);

            var run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);

            jobMetadata.ClusterId = ourCluster.ClusterId;
            jobMetadata.RunId = runId.RunId;
            jobMetadata.State = JobStateEnum.Calculating;
            await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);
            return runId;
        }

        /// <summary>This will wait for the cluster to be in a running state. If it does not achieve that within the timeoutLenght it throws an exception
        /// </summary>
        /// <param name="ourCluster"></param>
        /// <param name="jobMetadata"></param>
        /// <param name="client"></param>
        /// <param name="timeOutLength"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        private async Task WaitForClusterToBeReadyAsync(
            ClusterInfo ourCluster,
            JobMetadata jobMetadata,
            DatabricksClient client,
            int timeOutLength,
            CancellationToken cancellationToken)
        {
            var timeOut = new TimeSpan(0, timeOutLength, 0);

            while (ourCluster.State != ClusterState.RUNNING)
            {
                var clusterState = $"Waiting for cluster {ourCluster.ClusterId} state is {ourCluster.State}";
                _logger.LogInformation(clusterState);
                jobMetadata.State = JobStateEnum.ClusterWarmingUp;
                await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);

                Thread.Sleep(5000);
                ourCluster = await client.Clusters.Get(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
                timeOut = timeOut.Subtract(new TimeSpan(0, 0, 5));

                if (timeOut >= TimeSpan.Zero)
                {
                    continue;
                }

                var clusterError = $"Could not start cluster within {_coordinatorSettings.ClusterTimeoutMinutes}";
                _logger.LogError(clusterError);
                jobMetadata.State = JobStateEnum.ClusterFailed;
                await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);
                throw new Exception();
            }
        }
    }
}
