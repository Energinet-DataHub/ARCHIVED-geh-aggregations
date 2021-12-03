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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Assets;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Microsoft.Extensions.Configuration;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.IntegrationTests.Fixtures
{
    public class AggregationsFunctionAppFixture : FunctionAppFixture
    {
        private const string AggregationResultsContainerName = "aggregation-results";
        private const string ConvertedMessagesContainerName = "converted-messages";

        public AggregationsFunctionAppFixture()
        {
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
            BlobServiceClient = new BlobServiceClient("UseDevelopmentStorage=true");
        }

        [NotNull]
        public TopicResource? JobCompletedTopic { get; private set; }

        public BlobServiceClient BlobServiceClient { get; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
                return;

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\AggregationResultReceiver.ResultListeners\\bin\\{buildConfiguration}\\net5.0";
        }

        /// <inheritdoc/>
        protected override void OnConfigureEnvironment()
        {
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
            Environment.SetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY", IntegrationTestConfiguration.ApplicationInsightsInstrumentationKey);
        }

        /// <inheritdoc/>
        protected override async Task OnInitializeFunctionAppDependenciesAsync(IConfiguration localSettingsSnapshot)
        {
            // => Storage
            AzuriteManager.StartAzurite();
            await InitializeAggregationResultsAsync().ConfigureAwait(false);

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            Environment.SetEnvironmentVariable("AGGREGATIONS_SERVICE_BUS_CONNECTION_STRING", ServiceBusResourceProvider.ConnectionString);

            JobCompletedTopic = await ServiceBusResourceProvider
                .BuildTopic("sbt-job-completed").SetEnvironmentVariableToTopicName("AGGREGATION_JOB_COMPLETED_TOPIC_NAME")
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName("AGGREGATION_JOB_COMPLETED_SUBSCRIPTION_NAME")
                .CreateAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        protected override Task OnFunctionAppHostFailedAsync(IReadOnlyList<string> hostLogSnapshot, Exception exception)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            return base.OnFunctionAppHostFailedAsync(hostLogSnapshot, exception);
        }

        /// <inheritdoc/>
        protected override async Task OnDisposeFunctionAppDependenciesAsync()
        {
            // => Service Bus
            await ServiceBusResourceProvider.DisposeAsync().ConfigureAwait(false);

            // => Storage
            await BlobServiceClient.DeleteBlobContainerAsync(AggregationResultsContainerName).ConfigureAwait(false);
            await BlobServiceClient.DeleteBlobContainerAsync(ConvertedMessagesContainerName).ConfigureAwait(false);
            AzuriteManager.Dispose();
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }

        private async Task InitializeAggregationResultsAsync()
        {
            var testDocuments = new TestDocuments();
            var blobContainerClient = await CreateBlobContainerClientAsync(AggregationResultsContainerName).ConfigureAwait(false);
            await CreateBlobContainerClientAsync(ConvertedMessagesContainerName).ConfigureAwait(false);

            await blobContainerClient
                .UploadBlobAsync(nameof(testDocuments.NetExchangePerGridArea), testDocuments.NetExchangePerGridArea)
                .ConfigureAwait(false);
            await blobContainerClient
                .UploadBlobAsync(nameof(testDocuments.HourlyConsumptionPerGridArea), testDocuments.HourlyConsumptionPerGridArea)
                .ConfigureAwait(false);
            await blobContainerClient
                .UploadBlobAsync(nameof(testDocuments.FlexConsumptionPerGridArea), testDocuments.FlexConsumptionPerGridArea)
                .ConfigureAwait(false);
            await blobContainerClient
                .UploadBlobAsync(nameof(testDocuments.ProductionPerGridArea), testDocuments.ProductionPerGridArea)
                .ConfigureAwait(false);
            await blobContainerClient
                .UploadBlobAsync(nameof(testDocuments.TotalConsumptionPerGridArea), testDocuments.TotalConsumptionPerGridArea)
                .ConfigureAwait(false);
        }

        private async Task<BlobContainerClient> CreateBlobContainerClientAsync(string containerName)
        {
            var container = BlobServiceClient.GetBlobContainerClient(containerName);
            await container.DeleteIfExistsAsync().ConfigureAwait(false);
            await container.CreateIfNotExistsAsync().ConfigureAwait(false);
            return container;
        }
    }
}
