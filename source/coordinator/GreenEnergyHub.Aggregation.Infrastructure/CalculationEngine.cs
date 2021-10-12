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
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Application.Utilities;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    public class CalculationEngine
    {
        private readonly ILogger<CalculationEngine> _logger;
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly IMetaDataDataAccess _metaDataDataAccess;

        public CalculationEngine(ILogger<CalculationEngine> logger, CoordinatorSettings coordinatorSettings, IMetaDataDataAccess metaDataDataAccess)
        {
            _logger = logger;
            _coordinatorSettings = coordinatorSettings;
            _metaDataDataAccess = metaDataDataAccess;
        }

        public async Task CreateAndRunCalculationJobAsync(JobMetadata jobMetadata, JobProcessTypeEnum processType, List<string> parameters, string pythonFileName, CancellationToken cancellationToken)
        {
            jobMetadata.State = JobStateEnum.ClusterStartup;

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

            jobMetadata.ExecutionEndDate = SystemClock.Instance.GetCurrentInstant();

            await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);
        }

        private async Task<RunResultState?> WaitForJobCompletionAsync(
        DatabricksClient client,
        RunIdentifier runId,
        CancellationToken cancellationToken)
        {
            var run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);
            // TODO handle error scenarios (Timeout)
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

            jobMetadata.State = JobStateEnum.Calculating;
            await _metaDataDataAccess.UpdateJobAsync(jobMetadata).ConfigureAwait(false);
            return runId;
        }

        /// <summary>
        /// This will wait for the cluster to be in a running state. If it does not achieve that within the timeoutLenght it throws an exception
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
