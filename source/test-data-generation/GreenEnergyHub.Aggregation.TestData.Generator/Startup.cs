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
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

[assembly: FunctionsStartup(typeof(GreenEnergyHub.Aggregation.TestData.GeneratorFunction.Startup))]

namespace GreenEnergyHub.Aggregation.TestData.GeneratorFunction
{
    public class Startup : FunctionsStartup
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

            var masterDataStorageConnectionString = StartupConfig.GetConfigurationVariable("MASTERDATA_DB_CONNECTION_STRING");
            var chargesContainerName = StartupConfig.GetConfigurationVariable("CHARGES_DB_NAME");
            var chargeLinkContainerName = StartupConfig.GetConfigurationVariable("CHARGELINK_DB_NAME");
            var chargePriceContainerName = StartupConfig.GetConfigurationVariable("CHARGEPRICE_DB_NAME");
            var marketRolesContainerName = StartupConfig.GetConfigurationVariable("MARKETROLES_DB_NAME");
            var meteringPointContainerName = StartupConfig.GetConfigurationVariable("METERINGPOINTS_DB_NAME");
            var specialMeteringPointContainerName = StartupConfig.GetConfigurationVariable("GRID_LOSS_SYS_CORR_DB_NAME");

            // Configuration
            var generatorSettings = new GeneratorSettings()
            {
                MasterDataStorageConnectionString = masterDataStorageConnectionString,
                ChargesContainerName = chargesContainerName,
                ChargeLinkContainerName = chargeLinkContainerName,
                ChargePriceContainerName = chargePriceContainerName,
                MarketRolesContainerName = marketRolesContainerName,
                MeteringPointContainerName = meteringPointContainerName,
                SpecialMeteringPointContainerName = specialMeteringPointContainerName,
            };

            builder.Services.AddSingleton(generatorSettings);
            builder.Services.AddSingleton<IMasterDataStorage, MasterDataStorage>();
            builder.Services.AddSingleton<IGeneratorService, GeneratorService>();

            // Assemblies containing the stuff we want to wire up by convention
            var applicationAssembly = typeof(GeneratorService).Assembly;

            //Wire up all services in application
            builder.Services.AddSingletonsByConvention(applicationAssembly, x => x.Name.EndsWith("Service", StringComparison.InvariantCulture));

            //Wire up all test data parsers
            builder.Services.RegisterAllTypes<ITestDataParser>(new[] { applicationAssembly }, ServiceLifetime.Singleton);
        }
    }
}
