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
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator;
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator.Interfaces;
using Energinet.DataHub.Aggregation.Coordinator.Application.Utilities;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata.Enums;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.Logging;
using NodaTime;
using Job = Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata.Job;

namespace Energinet.DataHub.Aggregation.Coordinator.Infrastructure
{
    public class CalculationEngine : ICalculationEngine
    {
        private readonly ILogger<CalculationEngine> _logger;
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly IMetadataDataAccess _metadataDataAccess;

        public CalculationEngine(ILogger<CalculationEngine> logger, CoordinatorSettings coordinatorSettings, IMetadataDataAccess metadataDataAccess)
        {
            _logger = logger;
            _coordinatorSettings = coordinatorSettings;
            _metadataDataAccess = metadataDataAccess;
        }

        public async Task CreateAndRunCalculationJobAsync(Job job, List<string> parameters, string pythonFileName, CancellationToken cancellationToken)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }

            _logger.LogInformation("Cluster starting up for {jobId}", new { jobId = job.Id });

            using var client = DatabricksClient.CreateClient(_coordinatorSettings.ConnectionStringDatabricks, _coordinatorSettings.TokenDatabricks);
            var list = await client.Clusters.List(cancellationToken).ConfigureAwait(false);
            var ourCluster = list.Single(c => c.ClusterName == CoordinatorSettings.ClusterName);
            var jobSettings = JobSettings.GetNewNotebookJobSettings(
                $"{job.Type.GetDescription()} job",
                null,
                null);

            _logger.LogInformation("Cluster created for {jobId}", new { jobId = job.Id });
            if (ourCluster.State == ClusterState.TERMINATED)
            {
                await client.Clusters.Start(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
            }

            await WaitForClusterToBeReadyAsync(ourCluster, client, _coordinatorSettings.ClusterTimeoutMinutes, cancellationToken).ConfigureAwait(false);

            var runId = await CreateAndStartJobAsync(job, parameters, pythonFileName, jobSettings, ourCluster, client, cancellationToken).ConfigureAwait(false);

            var result = await WaitForJobCompletionAsync(client, runId, cancellationToken).ConfigureAwait(false);

            job.State = JobStateEnum.Completed;

            if (result == RunResultState.FAILED)
            {
                job.State = JobStateEnum.Failed;
            }

            _logger.LogInformation("{jobId} completed with state {state}", new { jobId = job.Id, state = job.State });

            job.CompletedDate = SystemClock.Instance.GetCurrentInstant();

            await _metadataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);
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
        /// <param name="job"></param>
        /// <param name="parameters"></param>
        /// <param name="pythonFileName"></param>
        /// <param name="jobSettings"></param>
        /// <param name="ourCluster"></param>
        /// <param name="client"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        private async Task<RunIdentifier> CreateAndStartJobAsync(
            Job job,
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

            job.DatabricksJobId = databricksJobId;
            await _metadataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);

            // Start the jobMetadata and retrieve the run id.
            var runId = await client.Jobs.RunNow(databricksJobId, null, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Waiting for run {@RunId}", runId.RunId);

            var run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);

            _logger.LogInformation("{jobId} started", new { jobId = job.Id });

            job.State = JobStateEnum.Started;

            if (run.StartTime.HasValue)
            {
                job.StartedDate = Instant.FromDateTimeOffset(run.StartTime.Value);
            }

            await _metadataDataAccess.UpdateJobAsync(job).ConfigureAwait(false);
            return runId;
        }

        /// <summary>
        /// This will wait for the cluster to be in a running state. If it does not achieve that within the timeoutLenght it throws an exception
        /// </summary>
        /// <param name="ourCluster"></param>
        /// <param name="client"></param>
        /// <param name="timeOutLength"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Task</returns>
        private async Task WaitForClusterToBeReadyAsync(
            ClusterInfo ourCluster,
            DatabricksClient client,
            int timeOutLength,
            CancellationToken cancellationToken)
        {
            var timeOut = new TimeSpan(0, timeOutLength, 0);

            while (ourCluster.State != ClusterState.RUNNING)
            {
                var clusterState = $"Waiting for cluster {ourCluster.ClusterId} state is {ourCluster.State}";
                _logger.LogInformation(clusterState);

                Thread.Sleep(5000);
                ourCluster = await client.Clusters.Get(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
                timeOut = timeOut.Subtract(new TimeSpan(0, 0, 5));

                if (timeOut >= TimeSpan.Zero)
                {
                    continue;
                }

                var clusterError = $"Could not start cluster within {_coordinatorSettings.ClusterTimeoutMinutes}";
                _logger.LogError(clusterError);
                throw new Exception();
            }
        }
    }
}
