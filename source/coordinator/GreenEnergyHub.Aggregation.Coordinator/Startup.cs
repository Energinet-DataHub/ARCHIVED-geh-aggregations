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
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.CoordinatorFunction;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.BlobStorage;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

[assembly: FunctionsStartup(typeof(Startup))]

namespace GreenEnergyHub.Aggregation.CoordinatorFunction
{
#pragma warning disable CA1812
    internal class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Register Serilog
            using var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY");
            var logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                .CreateLogger();
            builder.Services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));

            // Configuration
            var connectionStringDatabricks = StartupConfig.GetCustomConnectionString("CONNECTION_STRING_DATABRICKS");
            var tokenDatabricks = StartupConfig.GetConfigurationVariable("TOKEN_DATABRICKS");
            var connectionStringServiceBus = StartupConfig.GetConfigurationVariable("CONNECTION_STRING_SERVICEBUS");
            var dataStorageContainerName = StartupConfig.GetConfigurationVariable("DATA_STORAGE_CONTAINER_NAME");
            var timeSeriesPath = StartupConfig.GetConfigurationVariable("TIME_SERIES_PATH");
            var persistLocation = StartupConfig.GetConfigurationVariable("PERSIST_LOCATION");
            var dataStorageAccountName = StartupConfig.GetConfigurationVariable("DATA_STORAGE_ACCOUNT_NAME");
            var dataStorageAccountKey = StartupConfig.GetConfigurationVariable("DATA_STORAGE_ACCOUNT_KEY");
            var resultUrl = new Uri(StartupConfig.GetConfigurationVariable("RESULT_URL"));
            var snapshotUrl = new Uri(StartupConfig.GetConfigurationVariable("SNAPSHOT_URL"));
            var aggregationPythonFile = StartupConfig.GetConfigurationVariable("AGGREGATION_PYTHON_FILE");
            var wholesalePythonFile = StartupConfig.GetConfigurationVariable("WHOLESALE_PYTHON_FILE");
            var connectionStringDatabase = StartupConfig.GetConfigurationVariable("DATABASE_CONNECTIONSTRING");
            var datahubGln = StartupConfig.GetConfigurationVariable("DATAHUB_GLN");
            var esettGln = StartupConfig.GetConfigurationVariable("ESETT_GLN");
            var hostKey = StartupConfig.GetConfigurationVariable("HOST_KEY");
            var cosmosAccountEndpoint = StartupConfig.GetConfigurationVariable("COSMOS_ACCOUNT_ENDPOINT");
            var cosmosAccountKey = StartupConfig.GetConfigurationVariable("COSMOS_ACCOUNT_KEY");
            var cosmosDatabase = StartupConfig.GetConfigurationVariable("COSMOS_DATABASE");
            var cosmosContainerMeteringPoints = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_METERING_POINTS");
            var cosmosContainerMarketRoles = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_MARKET_ROLES");
            var cosmosContainerCharges = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_CHARGES");
            var cosmosContainerChargeLinks = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_CHARGE_LINKS");
            var cosmosContainerChargePrices = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_CHARGE_PRICES");
            var cosmosContainerGridLossSysCorr = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_GRID_LOSS_SYS_CORR");
            var cosmosContainerEsBrpRelations = StartupConfig.GetConfigurationVariable("COSMOS_CONTAINER_ES_BRP_RELATIONS");

            if (!int.TryParse(StartupConfig.GetConfigurationVariable("CLUSTER_TIMEOUT_MINUTES"), out var clusterTimeoutMinutes))
            {
                throw new Exception($"Could not parse cluster timeout minutes in {nameof(Startup)}");
            }

            var coordinatorSettings = new CoordinatorSettings
            {
                ConnectionStringDatabricks = connectionStringDatabricks,
                TokenDatabricks = tokenDatabricks,
                DataStorageContainerName = dataStorageContainerName,
                TimeSeriesPath = timeSeriesPath,
                PersistLocation = persistLocation,
                DataStorageAccountKey = dataStorageAccountKey,
                DataStorageAccountName = dataStorageAccountName,
                TelemetryInstrumentationKey = telemetryConfiguration.InstrumentationKey,
                ResultUrl = resultUrl,
                SnapshotUrl = snapshotUrl,
                AggregationPythonFile = aggregationPythonFile,
                WholesalePythonFile = wholesalePythonFile,
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

            builder.Services.AddSingleton(coordinatorSettings);
            builder.Services.AddSingleton(new GlnService(datahubGln, esettGln));
            builder.Services.AddSingleton(x => new PostOfficeServiceBusChannel(connectionStringServiceBus, "aggregations", x.GetRequiredService<ILogger<PostOfficeServiceBusChannel>>()));
            builder.Services.AddSingleton(x => new TimeSeriesServiceBusChannel(connectionStringServiceBus, "timeseries", x.GetRequiredService<ILogger<TimeSeriesServiceBusChannel>>()));
            builder.Services.AddSingleton<ICoordinatorService, CoordinatorService>();
            builder.Services.AddSingleton<IJsonSerializer>(x => new JsonSerializerWithOption());

            builder.Services.AddSingleton<PostOfficeDispatcher>();
            builder.Services.AddSingleton<TimeSeriesDispatcher>();
            builder.Services.SendProtobuf<Document>();
            builder.Services.AddSingleton<ISpecialMeteringPointsService, SpecialMeteringPointsService>();
            builder.Services.AddSingleton<IMetaDataDataAccess>(x => new MetaDataDataAccess(connectionStringDatabase));

            // Assemblies containing the stuff we want to wire up by convention
            var applicationAssembly = typeof(CoordinatorService).Assembly;
            var infrastructureAssembly = typeof(BlobService).Assembly;

            //Wire up all services in application
            builder.Services.AddSingletonsByConvention(applicationAssembly, x => x.Name.EndsWith("Service",  StringComparison.InvariantCulture));

            builder.Services.AddSingleton<ITriggerBaseArguments, TriggerBaseArguments>();

            //Wire up all services in infrastructure
            builder.Services.AddSingletonsByConvention(infrastructureAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

            // wire up all dispatch strategies.
            builder.Services.RegisterAllTypes<IDispatchStrategy>(new[] { applicationAssembly }, ServiceLifetime.Singleton);
            builder.Services.AddSingleton<IInputProcessor, InputProcessor>();
        }
    }
#pragma warning restore CA1812
}
