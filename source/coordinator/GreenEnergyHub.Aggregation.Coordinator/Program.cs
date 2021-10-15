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
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.BlobStorage;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using IPersistedDataService = GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces.IPersistedDataService;

namespace GreenEnergyHub.Aggregation.CoordinatorFunction
{
#pragma warning disable CA1812
    public class Program
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
                .ConfigureFunctionsWorkerDefaults();

            //wire up DI
            var buildHost = host.ConfigureServices((context, services) =>
                 {
                     // extract config values
                     ParseAndSetupConfiguration(
                         context.Configuration,
                         out var connectionStringDatabase,
                         out var datahubGln,
                         out var esettGln,
                         out var instrumentationKey,
                         out var coordinatorSettings,
                         out var connectionStringServiceBus);

                     // Setup Serilog
                     using var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                     telemetryConfiguration.InstrumentationKey = instrumentationKey;
                     var logger = new LoggerConfiguration()
                         .WriteTo.Console()
                         .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                         .CreateLogger();

                     services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
                     services.AddSingleton(coordinatorSettings);
                     services.AddSingleton(new GlnService(datahubGln, esettGln));
                     services.AddSingleton(x => new PostOfficeServiceBusChannel(connectionStringServiceBus, "aggregations", x.GetRequiredService<ILogger<PostOfficeServiceBusChannel>>()));
                     services.AddSingleton(x => new TimeSeriesServiceBusChannel(connectionStringServiceBus, "timeseries", x.GetRequiredService<ILogger<TimeSeriesServiceBusChannel>>()));
                     services.AddSingleton<IJsonSerializer>(x => new JsonSerializerWithOption());
                     services.AddSingleton<IPersistedDataService, PersistedDataService>();

                     services.AddSingleton<PostOfficeDispatcher>();
                     services.AddSingleton<IMessageDispatcher, TimeSeriesDispatcher>();
                     services.SendProtobuf<Document>();
                     services.AddSingleton<ISpecialMeteringPointsService, SpecialMeteringPointsService>();
                     services.AddSingleton<IMetaDataDataAccess>(x => new MetaDataDataAccess(connectionStringDatabase));
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
            out string datahubGln,
            out string esettGln,
            out string instrumentationKey,
            out CoordinatorSettings coordinatorSettings,
            out string connectionStringServiceBus)
        {
            // Configuration
            var connectionStringDatabricks = StartupConfig.GetConfigurationVariable(config, "CONNECTION_STRING_DATABRICKS");
            var tokenDatabricks = StartupConfig.GetConfigurationVariable(config, "TOKEN_DATABRICKS");
            var dataStorageContainerName = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_CONTAINER_NAME");
            var timeSeriesPath = StartupConfig.GetConfigurationVariable(config, "TIME_SERIES_PATH");
            var persistLocation = StartupConfig.GetConfigurationVariable(config, "PERSIST_LOCATION");
            var dataStorageAccountName = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_ACCOUNT_NAME");
            var dataStorageAccountKey = StartupConfig.GetConfigurationVariable(config, "DATA_STORAGE_ACCOUNT_KEY");
            var resultUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "RESULT_URL"));
            var snapshotUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "SNAPSHOT_URL"));
            var aggregationPythonFile = StartupConfig.GetConfigurationVariable(config, "AGGREGATION_PYTHON_FILE");
            var wholesalePythonFile = StartupConfig.GetConfigurationVariable(config, "WHOLESALE_PYTHON_FILE");
            var dataPreparationPythonFile = StartupConfig.GetConfigurationVariable(config, "DATA_PREPARATION_PYTHON_FILE");
            var hostKey = StartupConfig.GetConfigurationVariable(config, "HOST_KEY");
            var cosmosAccountEndpoint = StartupConfig.GetConfigurationVariable(config, "COSMOS_ACCOUNT_ENDPOINT");
            var cosmosAccountKey = StartupConfig.GetConfigurationVariable(config, "COSMOS_ACCOUNT_KEY");
            var cosmosDatabase = StartupConfig.GetConfigurationVariable(config, "COSMOS_DATABASE");
            var cosmosContainerMeteringPoints = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_METERING_POINTS");
            var cosmosContainerMarketRoles = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_MARKET_ROLES");
            var cosmosContainerCharges = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_CHARGES");
            var cosmosContainerChargeLinks = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_CHARGE_LINKS");
            var cosmosContainerChargePrices = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_CHARGE_PRICES");
            var cosmosContainerGridLossSysCorr = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_GRID_LOSS_SYS_CORR");
            var cosmosContainerEsBrpRelations = StartupConfig.GetConfigurationVariable(config, "COSMOS_CONTAINER_ES_BRP_RELATIONS");

            connectionStringServiceBus = StartupConfig.GetConfigurationVariable(config, "CONNECTION_STRING_SERVICEBUS");
            connectionStringDatabase = StartupConfig.GetConfigurationVariable(config, "DATABASE_CONNECTIONSTRING");
            datahubGln = StartupConfig.GetConfigurationVariable(config, "DATAHUB_GLN");
            esettGln = StartupConfig.GetConfigurationVariable(config, "ESETT_GLN");
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
                TimeSeriesPath = timeSeriesPath,
                PersistLocation = persistLocation,
                DataStorageAccountKey = dataStorageAccountKey,
                DataStorageAccountName = dataStorageAccountName,
                ResultUrl = resultUrl,
                SnapshotUrl = snapshotUrl,
                AggregationPythonFile = aggregationPythonFile,
                WholesalePythonFile = wholesalePythonFile,
                DataPreparationPythonFile = dataPreparationPythonFile,
                ClusterTimeoutMinutes = clusterTimeoutMinutes,
                HostKey = hostKey,
                CosmosAccountEndpoint = cosmosAccountEndpoint,
                CosmosAccountKey = cosmosAccountKey,
                CosmosDatabase = cosmosDatabase,
                CosmosContainerMeteringPoints = cosmosContainerMeteringPoints,
                CosmosContainerMarketRoles = cosmosContainerMarketRoles,
                CosmosContainerCharges = cosmosContainerCharges,
                CosmosContainerChargeLinks = cosmosContainerChargeLinks,
                CosmosContainerChargePrices = cosmosContainerChargePrices,
                CosmosContainerEsBrpRelations = cosmosContainerEsBrpRelations,
                CosmosContainerGridLossSysCorr = cosmosContainerGridLossSysCorr,
            };
        }
    }
#pragma warning restore CA1812
}
