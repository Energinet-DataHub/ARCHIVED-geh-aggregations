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
            var inputStorageContainerName = StartupConfig.GetConfigurationVariable("INPUTSTORAGE_CONTAINER_NAME");
            var inputPath = StartupConfig.GetConfigurationVariable("INPUT_PATH");
            var gridLossSysCorPath = StartupConfig.GetConfigurationVariable("GRID_LOSS_SYS_COR_PATH");
            var inputStorageAccountName = StartupConfig.GetConfigurationVariable("INPUTSTORAGE_ACCOUNT_NAME");
            var inputStorageAccountKey = StartupConfig.GetConfigurationVariable("INPUTSTORAGE_ACCOUNT_KEY");
            var resultUrl = new Uri(StartupConfig.GetConfigurationVariable("RESULT_URL"));
            var pythonFile = StartupConfig.GetConfigurationVariable("PYTHON_FILE");
            if (!int.TryParse(StartupConfig.GetConfigurationVariable("CLUSTER_TIMEOUT_MINUTES"), out var clusterTimeoutMinutes))
            {
                throw new Exception($"Could not parse cluster timeout minutes in {nameof(Startup)}");
            }

            var coordinatorSettings = new CoordinatorSettings
            {
                ConnectionStringDatabricks = connectionStringDatabricks,
                TokenDatabricks = tokenDatabricks,
                InputStorageContainerName = inputStorageContainerName,
                InputPath = inputPath,
                GridLossSysCorPath = gridLossSysCorPath,
                InputStorageAccountKey = inputStorageAccountKey,
                InputStorageAccountName = inputStorageAccountName,
                TelemetryInstrumentationKey = telemetryConfiguration.InstrumentationKey,
                ResultUrl = resultUrl,
                PythonFile = pythonFile,
                ClusterTimeoutMinutes = clusterTimeoutMinutes,
            };

            builder.Services.AddSingleton(coordinatorSettings);
            builder.Services.AddSingleton(x => new PostOfficeServiceBusChannel(connectionStringServiceBus, "aggregations", x.GetRequiredService<ILogger<PostOfficeServiceBusChannel>>()));
            //TODO: This configuration of servicebus is only for testing and need to be corrected, when timeseries domain is ready
            builder.Services.AddSingleton(x => new TimeSeriesServiceBusChannel(connectionStringServiceBus, "timeseries", x.GetRequiredService<ILogger<TimeSeriesServiceBusChannel>>()));
            builder.Services.AddSingleton<ICoordinatorService, CoordinatorService>();
            builder.Services.AddSingleton<IJsonSerializer>(x => new JsonSerializerWithOption());
            //TODO: I think this should be MessageDispatcher and not Dispatcher
            builder.Services.AddSingleton<Dispatcher>();
            builder.Services.AddSingleton<TimeSeriesDispatcher>();
            builder.Services.SendProtobuf<Document>();
            builder.Services.AddSingleton<ISpecialMeteringPointsService, SpecialMeteringPointsService>();

            // Assemblies containing the stuff we want to wire up by convention
            var applicationAssembly = typeof(CoordinatorService).Assembly;
            var infrastructureAssembly = typeof(BlobService).Assembly;

            //Wire up all services in application
            builder.Services.AddSingletonsByConvention(applicationAssembly, x => x.Name.EndsWith("Service",  StringComparison.InvariantCulture));

            //Wire up all services in infrastructure
            builder.Services.AddSingletonsByConvention(infrastructureAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

            // wire up all dispatch strategies.
            builder.Services.RegisterAllTypes<IDispatchStrategy>(new[] { applicationAssembly }, ServiceLifetime.Singleton);
            builder.Services.AddSingleton<IInputProcessor, InputProcessor>();
        }
    }
#pragma warning restore CA1812
}
