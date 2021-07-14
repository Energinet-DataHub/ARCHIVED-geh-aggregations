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
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.CoordinatorFunction;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.BlobStorage;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Protobuf;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GreenEnergyHub.Aggregation.CoordinatorFunction
{
#pragma warning disable CA1812
    internal class Program
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "This is main")]
        private static Task Main(string[] args)
        {
#if DEBUG
            Debugger.Launch();
#endif

            // Assemblies containing the stuff we want to wire up by convention
            var applicationAssembly = typeof(CoordinatorService).Assembly;
            var infrastructureAssembly = typeof(BlobService).Assembly;

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
                     services.AddSingleton<ICoordinatorService, CoordinatorService>();
                     services.AddSingleton<IJsonSerializer>(x => new JsonSerializerWithOption());

                     services.AddSingleton<PostOfficeDispatcher>();
                     services.AddSingleton<TimeSeriesDispatcher>();
                     services.SendProtobuf<Document>();
                     services.AddSingleton<ISpecialMeteringPointsService, SpecialMeteringPointsService>();
                     services.AddSingleton<IMetaDataDataAccess>(x => new MetaDataDataAccess(connectionStringDatabase));

                     // Wire up all services in application
                     services.AddSingletonsByConvention(applicationAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

                     // Wire up all services in infrastructure
                     services.AddSingletonsByConvention(infrastructureAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

                     // wire up all dispatch strategies.
                     services.RegisterAllTypes<IDispatchStrategy>(new[] { applicationAssembly }, ServiceLifetime.Singleton);
                     services.AddSingleton<IInputProcessor, InputProcessor>();
                 }).Build();

            return buildHost.RunAsync();
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
            var inputStorageContainerName = StartupConfig.GetConfigurationVariable(config, "INPUTSTORAGE_CONTAINER_NAME");
            var inputPath = StartupConfig.GetConfigurationVariable(config, "INPUT_PATH");
            var gridLossSysCorPath = StartupConfig.GetConfigurationVariable(config, "GRID_LOSS_SYS_COR_PATH");
            var persistLocation = StartupConfig.GetConfigurationVariable(config, "PERSIST_LOCATION");
            var inputStorageAccountName = StartupConfig.GetConfigurationVariable(config, "INPUTSTORAGE_ACCOUNT_NAME");
            var inputStorageAccountKey = StartupConfig.GetConfigurationVariable(config, "INPUTSTORAGE_ACCOUNT_KEY");
            var resultUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "RESULT_URL"));
            var snapshotUrl = new Uri(StartupConfig.GetConfigurationVariable(config, "SNAPSHOT_URL"));
            var pythonFile = StartupConfig.GetConfigurationVariable(config, "PYTHON_FILE");
            var hostKey = StartupConfig.GetConfigurationVariable(config, "HOST_KEY");

            connectionStringDatabase = StartupConfig.GetConfigurationVariable(config, "DATABASE_CONNECTIONSTRING");
            datahubGln = StartupConfig.GetConfigurationVariable(config, "DATAHUB_GLN");
            esettGln = StartupConfig.GetConfigurationVariable(config, "ESETT_GLN");
            connectionStringServiceBus = StartupConfig.GetConfigurationVariable(config, "CONNECTION_STRING_SERVICEBUS");
            instrumentationKey = StartupConfig.GetConfigurationVariable(config, "APPINSIGHTS_INSTRUMENTATIONKEY");

            if (!int.TryParse(StartupConfig.GetConfigurationVariable(config, "CLUSTER_TIMEOUT_MINUTES"), out var clusterTimeoutMinutes))
            {
                throw new Exception($"Could not parse cluster timeout minutes in {nameof(Program)}");
            }

            coordinatorSettings = new CoordinatorSettings
            {
                ConnectionStringDatabricks = connectionStringDatabricks,
                TokenDatabricks = tokenDatabricks,
                InputStorageContainerName = inputStorageContainerName,
                InputPath = inputPath,
                GridLossSysCorPath = gridLossSysCorPath,
                PersistLocation = persistLocation,
                InputStorageAccountKey = inputStorageAccountKey,
                InputStorageAccountName = inputStorageAccountName,
                TelemetryInstrumentationKey = instrumentationKey,
                ResultUrl = resultUrl,
                SnapshotUrl = snapshotUrl,
                PythonFile = pythonFile,
                ClusterTimeoutMinutes = clusterTimeoutMinutes,
                HostKey = hostKey,
            };
        }
    }
#pragma warning restore CA1812
}
