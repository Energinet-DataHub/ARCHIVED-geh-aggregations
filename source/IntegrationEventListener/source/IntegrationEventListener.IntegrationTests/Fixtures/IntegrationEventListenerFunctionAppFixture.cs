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
using System.Linq;
using System.Threading.Tasks;
using Dapper.NodaTime;
using Energinet.DataHub.Aggregations.Application;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.DatabaseMigration;
using Energinet.DataHub.Aggregations.Domain.MeteringPoints;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence;
using Energinet.DataHub.Aggregations.Infrastructure.Persistence.Repositories;
using Energinet.DataHub.Core.FunctionApp.TestCommon;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Azurite;
using Energinet.DataHub.Core.FunctionApp.TestCommon.Configuration;
using Energinet.DataHub.Core.FunctionApp.TestCommon.FunctionAppHost;
using Energinet.DataHub.Core.FunctionApp.TestCommon.ServiceBus.ResourceProvider;
using Energinet.DataHub.IntegrationTest.Core.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ThrowawayDb;
using Xunit;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Fixtures
{
    public class IntegrationEventListenerFunctionAppFixture : FunctionAppFixture
    {
        private readonly MasterDataDatabaseManager _databaseManager;

        public IntegrationEventListenerFunctionAppFixture()
        {
            _databaseManager = new MasterDataDatabaseManager();
            AzuriteManager = new AzuriteManager();
            IntegrationTestConfiguration = new IntegrationTestConfiguration();
            ServiceBusResourceProvider = new ServiceBusResourceProvider(IntegrationTestConfiguration.ServiceBusConnectionString, TestLogger);
            _databaseManager.CreateDatabase();

            //_database = ThrowawayDatabase.FromLocalInstance("(localdb)\\mssqllocaldb");
            //Console.WriteLine($"Created database {_database.Name}");

           // Upgrader.DatabaseUpgrade(_database.ConnectionString);
            MeteringPointRepository = new MeteringPointRepository(_databaseManager.CreateDbContext());
            //DapperNodaTimeSetup.Register();
            MeteringpointCreatedEventToMeteringPointMasterDataTransformer =
                new EventToMasterDataTransformer<MeteringPointCreatedMutator, MeteringPoint>(MeteringPointRepository);
        }

        public EventToMasterDataTransformer<MeteringPointCreatedMutator, MeteringPoint> MeteringpointCreatedEventToMeteringPointMasterDataTransformer { get; set; }

        public MeteringPointRepository MeteringPointRepository { get; set; }

        [NotNull]
        public TopicResource? MPCreatedTopic { get; private set; }

        [NotNull]
        public TopicResource? MPConnectedTopic { get; private set; }

        private AzuriteManager AzuriteManager { get; }

        private IntegrationTestConfiguration IntegrationTestConfiguration { get; }

        private ServiceBusResourceProvider ServiceBusResourceProvider { get; }

        /// <inheritdoc/>
        protected override void OnConfigureHostSettings(FunctionAppHostSettings hostSettings)
        {
            if (hostSettings == null)
                return;

            var buildConfiguration = GetBuildConfiguration();
            hostSettings.FunctionApplicationPath = $"..\\..\\..\\..\\Energinet.DataHub.Aggregations.IntegrationEventListener\\bin\\{buildConfiguration}\\net5.0";

            // The log message we expect in the host log when the host is started and ready to server.
            hostSettings.HostStartedEvent = "Worker process started and initialized";
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

            // => Service Bus
            // Overwrite service bus related settings, so the function app uses the names we have control of in the test
            Environment.SetEnvironmentVariable(EnvironmentSettingNames.IntegrationEventListenerConnectionString, ServiceBusResourceProvider.ConnectionString);

            MPCreatedTopic = await ServiceBusResourceProvider
                .BuildTopic("sbt-mp-created").SetEnvironmentVariableToTopicName(EnvironmentSettingNames.MeteringPointCreatedTopicName)
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MeteringPointCreatedSubscriptionName)
                .CreateAsync().ConfigureAwait(false);

            MPConnectedTopic = await ServiceBusResourceProvider
                .BuildTopic("sbt-mp-connected").SetEnvironmentVariableToTopicName(EnvironmentSettingNames.MeteringPointConnectedTopicName)
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.MeteringPointConnectedSubscriptionName)
                .CreateAsync().ConfigureAwait(false);

            await ServiceBusResourceProvider
                .BuildTopic("sbt-supplier-changed").SetEnvironmentVariableToTopicName(EnvironmentSettingNames.EnergySupplierChangedTopicName)
                .AddSubscription("subscription").SetEnvironmentVariableToSubscriptionName(EnvironmentSettingNames.EnergySupplierChangedSubscriptionName)
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
            AzuriteManager.Dispose();

            // => DB
            await _databaseManager.DeleteDatabaseAsync().ConfigureAwait(false);
        }

        private static string GetBuildConfiguration()
        {
#if DEBUG
            return "Debug";
#else
            return "Release";
#endif
        }
    }
}
