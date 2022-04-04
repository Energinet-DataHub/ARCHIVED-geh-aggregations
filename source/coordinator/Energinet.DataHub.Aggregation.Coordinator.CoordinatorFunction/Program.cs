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
using Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction.Configuration;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure.BlobStorage;
using Energinet.DataHub.Aggregation.Coordinator.Infrastructure.ServiceBusProtobuf;
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
                         context.Configuration,
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
                 }).Build();

            DapperNodaTimeSetup.Register();

            buildHost.Run();
        }

        private static void ParseAndSetupConfiguration(
            IConfiguration config,
            out string connectionStringDatabase,
            out string instrumentationKey,
            out CoordinatorSettings coordinatorSettings)
        {
            // Configuration
            var connectionStringDatabricks = StartupConfig.GetConfigurationVariable(config, "CONNECTION_STRING_DATABRICKS");
            var tokenDatabricks = StartupConfig.GetConfigurationVariable(config, "TOKEN_DATABRICKS");
            var dataStorageContainerName = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_CONTAINER_NAME");
            var dataStorageAccountName = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_ACCOUNT_NAME");
            var dataStorageAccountKey = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_ACCOUNT_KEY");
            var sharedStorageAggregationsContainerName = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_AGGREGATIONS_CONTAINER_NAME");
            var sharedStorageTimeSeriesContainerName = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_TIME_SERIES_CONTAINER_NAME");
            var sharedStorageAccountName = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_ACCOUNT_NAME");
            var sharedStorageAccountKey = StartupConfig.GetConfigurationVariable(config, "SHARED_STORAGE_ACCOUNT_KEY");
            var timeSeriesPath = StartupConfig.GetConfigurationVariable(config, "TIME_SERIES_PATH");
            var masterDataDatabaseConnectionString =
                StartupConfig.GetConfigurationVariable(config, "MASTER_DATA_DATABASE_CONNECTION_STRING");
            var gridLossSystemCorrectionPath = StartupConfig.GetConfigurationVariable(config, "GRID_LOSS_SYSTEM_CORRECTION_PATH");
            var snapshotsBasePath = StartupConfig.GetConfigurationVariable(config, "SNAPSHOTS_BASE_PATH");
            var resultUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "RESULT_URL"));
            var snapshotNotifyUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "SNAPSHOT_NOTIFY_URL"));
            var aggregationPythonFile = StartupConfig.GetConfigurationVariable(config, "AGGREGATION_PYTHON_FILE");
            var wholesalePythonFile = StartupConfig.GetConfigurationVariable(config, "WHOLESALE_PYTHON_FILE");
            var dataPreparationPythonFile = StartupConfig.GetConfigurationVariable(config, "DATA_PREPARATION_PYTHON_FILE");
            var b2cTenantId = StartupConfig.GetConfigurationVariable(config, "B2C_TENANT_ID");
            var backendServiceAppId = StartupConfig.GetConfigurationVariable(config, "BACKEND_SERVICE_APP_ID");

            connectionStringDatabase = StartupConfig.GetConfigurationVariable(config, "DATABASE_CONNECTIONSTRING");
            instrumentationKey = StartupConfig.GetConfigurationVariable(config, "APPINSIGHTS_INSTRUMENTATIONKEY");

            if (!int.TryParse(StartupConfig.GetConfigurationVariable(config, "CLUSTER_TIMEOUT_MINUTES"), out var clusterTimeoutMinutes))
            {
                throw new Exception($"Could not parse cluster timeout minutes in {nameof(ParseAndSetupConfiguration)}");
            }

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
                TimeSeriesPath = timeSeriesPath,
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
