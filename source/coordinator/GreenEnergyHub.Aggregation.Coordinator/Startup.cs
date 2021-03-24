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
using GreenEnergyHub.Aggregation.Application.Coordinator.Handlers;
using GreenEnergyHub.Aggregation.Application.Coordinator.HourlyConsumption;
using GreenEnergyHub.Aggregation.Application.GLN;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.CoordinatorFunction;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
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
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
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
            var inputStorageAccountName = StartupConfig.GetConfigurationVariable("INPUTSTORAGE_ACCOUNT_NAME");
            var inputStorageAccountKey = StartupConfig.GetConfigurationVariable("INPUTSTORAGE_ACCOUNT_KEY");
            var telemetryInstrumentationKey = StartupConfig.GetConfigurationVariable("TELEMETRY_INSTRUMENTATION_KEY");
            var resultUrl = StartupConfig.GetConfigurationVariable("RESULT_URL");
            var pythonFile = StartupConfig.GetConfigurationVariable("PYTHON_FILE");
            var clusterTimeoutMinutes = StartupConfig.GetConfigurationVariable("CLUSTER_TIMEOUT_MINUTES");

            var coordinatorSettings = new CoordinatorSettings(
                connectionStringDatabricks: connectionStringDatabricks,
                tokenDatabricks: tokenDatabricks,
                inputStorageContainerName: inputStorageContainerName,
                inputStorageAccountKey: inputStorageAccountKey,
                inputStorageAccountName: inputStorageAccountName,
                telemetryInstrumentationKey: telemetryInstrumentationKey,
                resultUrl: resultUrl,
                pythonFile: pythonFile,
                clusterTimeOutMinutes: clusterTimeoutMinutes);

            builder.Services.AddSingleton(coordinatorSettings);

            builder.Services.AddSingleton<Channel>(new ServiceBusChannel(connectionStringServiceBus, "aggregations"));
            builder.Services.AddScoped<ICoordinatorService, CoordinatorService>();
            builder.Services.AddScoped<Dispatcher>();
            builder.Services.SendProtobuf<Document>();
            builder.Services.AddSingleton<IGLNService, GlnService>();
            builder.Services.AddSingleton<ISpecialMeteringPointsService, SpecialMeteringPointsService>();
            builder.Services.AddSingleton<HourlyConsumptionHandler>();
            builder.Services.AddSingleton<FlexConsumptionHandler>();
            builder.Services.AddSingleton<HourlyProductionHandler>();
            builder.Services.AddSingleton<AdjustedFlexConsumptionHandler>();
            builder.Services.AddSingleton<AdjustedProductionHandler>();
        }
    }
#pragma warning restore CA1812
}
