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
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.BlobStorage;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.Logging;
using NodaTime;
using Job = GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Job;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly ILogger<CoordinatorService> _logger;
        private readonly IBlobService _blobService;
        private readonly IInputProcessor _inputProcessor;
        private readonly IMetaDataDataAccess _metaDataDataAccess;
        private readonly ITriggerBaseArguments _triggerBaseArguments;

        public CoordinatorService(
            CoordinatorSettings coordinatorSettings,
            ILogger<CoordinatorService> logger,
            IBlobService blobService,
            IInputProcessor inputProcessor,
            IMetaDataDataAccess metaDataDataAccess,
            ITriggerBaseArguments triggerBaseArguments)
        {
            _blobService = blobService;
            _inputProcessor = inputProcessor;
            _metaDataDataAccess = metaDataDataAccess;
            _triggerBaseArguments = triggerBaseArguments;
            _coordinatorSettings = coordinatorSettings;
            _logger = logger;
        }

        public async Task StartAggregationJobAsync(string processType, Instant beginTime, Instant endTime, string resultId, bool persist, string resolution, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = _triggerBaseArguments.GetTriggerBaseArguments(beginTime, endTime, processType, persist);
                parameters.Add($"--resolution={resolution}");
                parameters.Add($"--result-id={resultId}");

                await CreateAndRunDatabricksJobAsync(processType, CoordinatorSettings.ClusterAggregationJobName, parameters, _coordinatorSettings.AggregationPythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start aggregation job {message} {stack}", e.Message, e.StackTrace);
                throw;
            }
        }

        public async Task StartWholesaleJobAsync(string processType, Instant beginTime, Instant endTime, bool persist, CancellationToken cancellationToken)
        {
            try
            {
                var parameters = _triggerBaseArguments.GetTriggerBaseArguments(beginTime, endTime, processType, persist);
                parameters.Add($"--cosmos-container-charges={_coordinatorSettings.CosmosContainerCharges}");
                parameters.Add($"--cosmos-container-charge-links={_coordinatorSettings.CosmosContainerChargeLinks}");
                parameters.Add($"--cosmos-container-charge-prices={_coordinatorSettings.CosmosContainerChargePrices}");

                await CreateAndRunDatabricksJobAsync(processType, CoordinatorSettings.ClusterWholesaleJobName, parameters, _coordinatorSettings.WholesalePythonFile, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Exception when trying to start wholesale job {message} {stack}", e.Message, e.StackTrace);
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

                await using var stream = await _blobService.GetBlobStreamAsync(inputPath, cancellationToken).ConfigureAwait(false);

                result.State = "Stream captured";
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

        private async Task CreateAndRunDatabricksJobAsync(string processType, string jobName, List<string> parameters, string pythonFileName, CancellationToken cancellationToken)
        {
            // TODO: #199 beginTime, endTime, resultId and persist should stored when creating job metadata (add to parameters list)
            var job = await CreateJobAsync(processType).ConfigureAwait(false);

            using var client = DatabricksClient.CreateClient(_coordinatorSettings.ConnectionStringDatabricks, _coordinatorSettings.TokenDatabricks);
            var list = await client.Clusters.List(cancellationToken).ConfigureAwait(false);
            var ourCluster = list.Single(c => c.ClusterName == CoordinatorSettings.ClusterName);
            var jobSettings = JobSettings.GetNewNotebookJobSettings(
                jobName,
                null,
                null);

            job.State = "Checking cluster";
            await _metaDataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);
            if (ourCluster.State == ClusterState.TERMINATED)
            {
                await client.Clusters.Start(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
            }

            var timeOut = new TimeSpan(0, _coordinatorSettings.ClusterTimeoutMinutes, 0);
            while (ourCluster.State != ClusterState.RUNNING)
            {
                var clusterState = $"Waiting for cluster {ourCluster.ClusterId} state is {ourCluster.State}";
                _logger.LogInformation(clusterState);
                job.State = clusterState;
                await _metaDataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);

                Thread.Sleep(5000);
                ourCluster = await client.Clusters.Get(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
                timeOut = timeOut.Subtract(new TimeSpan(0, 0, 5));

                if (timeOut >= TimeSpan.Zero)
                {
                    continue;
                }

                var clusterError = $"Could not start cluster within {_coordinatorSettings.ClusterTimeoutMinutes}";
                _logger.LogError(clusterError);
                job.State = clusterError;
                await _metaDataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);
                throw new Exception();
            }

            jobSettings.SparkPythonTask = new SparkPythonTask
            {
                PythonFile = pythonFileName,
                Parameters = parameters,
            };
            jobSettings.WithExistingCluster(ourCluster.ClusterId);

            // Create new job
            var databricksJobId = await client.Jobs.Create(jobSettings, cancellationToken).ConfigureAwait(false);

            job.DatabricksJobId = databricksJobId;
            await _metaDataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);

            // Start the job and retrieve the run id.
            var runId = await client.Jobs.RunNow(databricksJobId, null, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Waiting for run {@RunId}", runId.RunId);

            var run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);

            job.ClusterId = ourCluster.ClusterId;
            job.RunId = runId.RunId;
            await _metaDataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);

            // TODO refactor this to run as a time triggered function or something similar.
            while (!run.IsCompleted)
            {
                job.State = "Waiting for databricks job to complete";
                await _metaDataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);

                _logger.LogInformation("Waiting for run {runId}", new { runId = runId.RunId });
                Thread.Sleep(2000);
                run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);
            }
        }

        private async Task<Job> CreateJobAsync(string processType)
        {
            var job = new Domain.DTOs.MetaData.Job(processType);
            await _metaDataDataAccess.CreateJobAsync(job).ConfigureAwait(false);
            return job;
        }
    }
}
