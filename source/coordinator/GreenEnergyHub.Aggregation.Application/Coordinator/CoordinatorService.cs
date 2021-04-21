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
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator.Handlers;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Azure.Databricks.Client;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class CoordinatorService : ICoordinatorService
    {
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly Dispatcher _dispatcher;
        private readonly ILogger<CoordinatorService> _logger;
        private readonly HourlyConsumptionHandler _hourlyConsumptionHandler;
        private readonly FlexConsumptionHandler _flexConsumptionHandler;
        private readonly HourlyProductionHandler _hourlyProductionHandler;
        private readonly AdjustedFlexConsumptionHandler _adjustedFlexConsumptionHandler;
        private readonly AdjustedProductionHandler _adjustedProductionHandler;

        public CoordinatorService(
            CoordinatorSettings coordinatorSettings,
            Dispatcher dispatcher,
            ILogger<CoordinatorService> logger,
            HourlyConsumptionHandler hourlyConsumptionHandler,
            FlexConsumptionHandler flexConsumptionHandler,
            HourlyProductionHandler hourlyProductionHandler,
            AdjustedFlexConsumptionHandler adjustedFlexConsumptionHandler,
            AdjustedProductionHandler adjustedProductionHandler)
        {
            _coordinatorSettings = coordinatorSettings;
            _dispatcher = dispatcher;
            _logger = logger;
            _hourlyConsumptionHandler = hourlyConsumptionHandler;
            _flexConsumptionHandler = flexConsumptionHandler;
            _hourlyProductionHandler = hourlyProductionHandler;
            _adjustedFlexConsumptionHandler = adjustedFlexConsumptionHandler;
            _adjustedProductionHandler = adjustedProductionHandler;
        }

        public async Task StartAggregationJobAsync(ProcessType processType, string beginTime, string endTime, string resultId, CancellationToken cancellationToken)
        {
            using var client = DatabricksClient.CreateClient(_coordinatorSettings.ConnectionStringDatabricks, _coordinatorSettings.TokenDatabricks);
            var list = await client.Clusters.List(cancellationToken).ConfigureAwait(false);
            var ourCluster = list.Single(c => c.ClusterName == CoordinatorSettings.ClusterName);
            var jobSettings = JobSettings.GetNewNotebookJobSettings(
                CoordinatorSettings.ClusterJobName,
                null,
                null);
            if (ourCluster.State == ClusterState.TERMINATED)
            {
                await client.Clusters.Start(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
            }

            var timeOut = new TimeSpan(0, _coordinatorSettings.ClusterTimeoutMinutes, 0);
            while (ourCluster.State != ClusterState.RUNNING)
            {
                _logger.LogInformation($"Waiting for cluster {ourCluster.ClusterId} state is {ourCluster.State}");
                Thread.Sleep(5000);
                ourCluster = await client.Clusters.Get(ourCluster.ClusterId, cancellationToken).ConfigureAwait(false);
                timeOut = timeOut.Subtract(new TimeSpan(0, 0, 5));

                if (timeOut < TimeSpan.Zero)
                {
                    throw new Exception($"Could not start cluster within {_coordinatorSettings.ClusterTimeoutMinutes}");
                }
            }

            var parameters = new List<string>
            {
                $"--input-storage-account-name={_coordinatorSettings.InputStorageAccountName}",
                $"--input-storage-account-key={_coordinatorSettings.InputStorageAccountKey}",
                $"--input-storage-container-name={_coordinatorSettings.InputStorageContainerName}",
                $"--beginning-date-time={beginTime}",
                $"--end-date-time={endTime}",
                $"--telemetry-instrumentation-key={_coordinatorSettings.TelemetryInstrumentationKey}",
                $"--process-type={Enum.GetName(typeof(ProcessType), processType)}",
                $"--result-url={_coordinatorSettings.ResultUrl}",
                $"--result-id={resultId}",
            };

            jobSettings.SparkPythonTask = new SparkPythonTask
            {
                PythonFile = _coordinatorSettings.PythonFile,
                Parameters = parameters,
            };
            jobSettings.WithExistingCluster(ourCluster.ClusterId);

            // Create new job
            var jobId = await client.Jobs.Create(jobSettings, cancellationToken).ConfigureAwait(false);

            // Start the job and retrieve the run id.
            var runId = await client.Jobs.RunNow(jobId, null, cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Waiting for run {@RunId}", runId.RunId);

            var run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);

            while (!run.IsCompleted)
            {
                _logger.LogInformation("Waiting for run {runId}", new { runId = runId.RunId });
                Thread.Sleep(2000);
                run = await client.Jobs.RunsGet(runId.RunId, cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task HandleResultAsync(string content, string resultId, string processType, string startTime, string endTime, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Entered HandleResultAsync");
            var results = JsonSerializer.Deserialize<AggregationResultsContainer>(content);

            var pt = (ProcessType)Enum.Parse(typeof(ProcessType), processType, true);

            //Python time formatting of zulu offset needs to be trimmed
            startTime = startTime.Substring(0, startTime.Length - 2);
            endTime = endTime.Substring(0, endTime.Length - 2);
            _logger.LogInformation("starting to dispatch messages");
            try
            {
                await DispatchAsync(_hourlyConsumptionHandler.PrepareMessages(results.HourlyConsumption, pt, startTime, endTime), cancellationToken).ConfigureAwait(false);
                await DispatchAsync(_flexConsumptionHandler.PrepareMessages(results.FlexConsumption, pt, startTime, endTime), cancellationToken).ConfigureAwait(false);
                await DispatchAsync(_hourlyProductionHandler.PrepareMessages(results.HourlyProduction, pt, startTime, endTime), cancellationToken).ConfigureAwait(false);
                await DispatchAsync(_adjustedFlexConsumptionHandler.PrepareMessages(results.AdjustedFlexConsumption, pt, startTime, endTime), cancellationToken).ConfigureAwait(false);
                await DispatchAsync(_adjustedProductionHandler.PrepareMessages(results.AdjustedHourlyProduction, pt, startTime, endTime), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "We encountered an error while dispatching");
            }

            _logger.LogInformation("All messages dispatched");
        }

        private async Task DispatchAsync(IEnumerable<IOutboundMessage> preparedMessages, CancellationToken cancellationToken)
        {
            try
            {
                await _dispatcher.DispatchBulkAsync(preparedMessages, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not dispatch message due to {error}", new { error = e.Message });
            }
        }
    }
}
