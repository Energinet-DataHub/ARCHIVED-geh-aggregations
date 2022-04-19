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
using System.IO;
using Dapper.NodaTime;
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator;
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator.Interfaces;
using Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction.Common;
using Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction.Configuration;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure.BlobStorage;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure.Registration;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure.ServiceBusProtobuf;
using Energinet.DataHub.Core.App.Common.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Diagnostics.HealthChecks;
using Energinet.DataHub.Core.App.FunctionApp.Middleware;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Messaging;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using IPersistedDataService = Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator.Interfaces.IPersistedDataService;

namespace Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction
{
#pragma warning disable CA1812
    public static class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is main")]
        public static void Main(string[] args)
        {
            // Assemblies containing the stuff we want to wire up by convention
            var applicationAssembly = typeof(CoordinatorService).Assembly;
            var infrastructureAssembly = typeof(PersistedDataService).Assembly;

            // wire up configuration
            var host = new HostBuilder().ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddCommandLine(args);
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("local.settings.json", true, true);
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults(builder =>
                {
                    builder.UseMiddleware<JwtTokenMiddleware>();
                });

            //wire up DI
            var buildHost = host.ConfigureServices((context, services) =>
                 {
                     // extract config values
                     ParseAndSetupConfiguration(
                         out var connectionStringDatabase,
                         out var instrumentationKey,
                         out var coordinatorSettings);

                     services.AddJwtTokenSecurity();

                     // Setup Serilog
                     using var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                     telemetryConfiguration.InstrumentationKey = instrumentationKey;
                     var logger = new LoggerConfiguration()
                         .WriteTo.Console()
                         .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                         .CreateLogger();

                     services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
                     services.AddSingleton(coordinatorSettings);
                     services.AddSingleton<IJsonSerializer>(x => new JsonSerializerWithOption());
                     services.AddSingleton<IPersistedDataService, PersistedDataService>();

                     services.AddSingleton<PostOfficeDispatcher>();
                     services.AddSingleton<IMessageDispatcher, TimeSeriesDispatcher>();
                     services.SendProtobuf<Document>();
                     services.AddSingleton<IMetadataDataAccess>(x => new MetadataDataAccess(connectionStringDatabase));
                     services.AddSingleton<ICoordinatorService, CoordinatorService>();
                     services.AddSingleton<ITriggerBaseArguments, TriggerArguments>();
                     services.AddSingleton<ICalculationEngine, CalculationEngine>();

                     // Wire up all services in application
                     services.AddSingletonsByConvention(applicationAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

                     // Wire up all services in infrastructure
                     services.AddSingletonsByConvention(infrastructureAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

                     // wire up all dispatch strategies.
                     services.RegisterAllTypes<IDispatchStrategy>(new[] { applicationAssembly }, ServiceLifetime.Singleton);

                     // Health check
                     services.AddScoped<IHealthCheckEndpointHandler, HealthCheckEndpointHandler>();
                     services.AddHealthChecks()
                         .AddLiveCheck()
                         .AddSqlServer(
                             name: "CoordinatorDb",
                             connectionString: EnvironmentHelper.GetEnv(EnvironmentSettingNames.CoordinatorDbConnectionString));
                 }).Build();

            DapperNodaTimeSetup.Register();

            buildHost.Run();
        }

        private static void ParseAndSetupConfiguration(
            out string connectionStringDatabase,
            out string instrumentationKey,
            out CoordinatorSettings coordinatorSettings)
        {
            // Configuration
            instrumentationKey = EnvironmentHelper.GetEnv(EnvironmentSettingNames.AppInsightsInstrumentationKey);

            /*
             * JWT Token authentication
             */
            var b2cTenantId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.B2CTenantId);
            var backendServiceAppId = EnvironmentHelper.GetEnv(EnvironmentSettingNames.BackendServiceAppId);

            /*
             * Databricks related configuration settings
             */
            if (!int.TryParse(EnvironmentHelper.GetEnv(EnvironmentSettingNames.ClusterTimeoutMinutes), out var clusterTimeoutMinutes))
            {
                throw new Exception($"Could not parse cluster timeout minutes in {nameof(ParseAndSetupConfiguration)}");
            }

            var connectionStringDatabricks = EnvironmentHelper.GetEnv(EnvironmentSettingNames.ConnectionStringDatabricks);
            var tokenDatabricks = EnvironmentHelper.GetEnv(EnvironmentSettingNames.TokenDatabricks);

            /*
            * Path to python files in Databricks file system
            */
            var dataPreparationPythonFile = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataPreparationPythonFile);
            var aggregationPythonFile = EnvironmentHelper.GetEnv(EnvironmentSettingNames.AggregationPythonFile);
            var wholesalePythonFile = EnvironmentHelper.GetEnv(EnvironmentSettingNames.WholesalePythonFile);

            /*
            * Database connections strings
            */
            connectionStringDatabase = EnvironmentHelper.GetEnv(EnvironmentSettingNames.CoordinatorDbConnectionString);
            var masterDataDatabaseConnectionString = EnvironmentHelper.GetEnv(EnvironmentSettingNames.MasterDataDbConnectionString);

            /*
            * Endpoints used by jobs in Databricks
            */
            var resultUrl = new Uri(EnvironmentHelper.GetEnv(EnvironmentSettingNames.ResultReceiverUrl));
            var snapshotNotifyUrl = new Uri(EnvironmentHelper.GetEnv(EnvironmentSettingNames.SnapshotReceiverUrl));

            /*
            * Storage account configuration settings
            */
            var sharedStorageAccountKey = EnvironmentHelper.GetEnv(EnvironmentSettingNames.SharedStorageAccountKey);
            var sharedStorageAccountName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.SharedStorageAccountName);
            var dataStorageAccountKey = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataStorageAccountKey);
            var dataStorageAccountName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataStorageAccountName);
            var dataStorageContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.DataStorageContainerName);
            var sharedStorageAggregationsContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.SharedStorageAggregationsContainerName);
            var sharedStorageTimeSeriesContainerName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.SharedStorageTimeSeriesContainerName);
            var timeSeriesPointsDeltaTableName = EnvironmentHelper.GetEnv(EnvironmentSettingNames.TimeSeriesPointsDeltaTableName);

            var snapshotsBasePath = EnvironmentHelper.GetEnv(EnvironmentSettingNames.SnapshotsBasePath);

            var gridLossSystemCorrectionPath = EnvironmentHelper.GetEnv(EnvironmentSettingNames.GridLossSystemCorrectionPath);

            coordinatorSettings = new CoordinatorSettings
            {
                ConnectionStringDatabricks = connectionStringDatabricks,
                TokenDatabricks = tokenDatabricks,
                DataStorageContainerName = dataStorageContainerName,
                DataStorageAccountKey = dataStorageAccountKey,
                DataStorageAccountName = dataStorageAccountName,
                SharedStorageAggregationsContainerName = sharedStorageAggregationsContainerName,
                SharedStorageTimeSeriesContainerName = sharedStorageTimeSeriesContainerName,
                SharedStorageAccountKey = sharedStorageAccountKey,
                SharedStorageAccountName = sharedStorageAccountName,
                TimeSeriesPointsDeltaTableName = timeSeriesPointsDeltaTableName,
                MasterDataDatabaseConnectionString = masterDataDatabaseConnectionString,
                GridLossSystemCorrectionPath = gridLossSystemCorrectionPath,
                SnapshotsBasePath = snapshotsBasePath,
                ResultUrl = resultUrl,
                SnapshotNotifyUrl = snapshotNotifyUrl,
                AggregationPythonFile = aggregationPythonFile,
                WholesalePythonFile = wholesalePythonFile,
                DataPreparationPythonFile = dataPreparationPythonFile,
                ClusterTimeoutMinutes = clusterTimeoutMinutes,
                B2CTenantId = b2cTenantId,
                BackendServiceAppId = backendServiceAppId,
            };
        }
    }
#pragma warning restore CA1812
}
