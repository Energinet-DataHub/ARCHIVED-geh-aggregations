using System;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GreenEnergyHub.Aggregation.TestData.GeneratorFunction
{
    public class Startup : FunctionsStartup
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

            var masterDataStorageConnectionString = StartupConfig.GetConfigurationVariable("MASTERDATA_DB_CONNECTION_STRING");
            var chargesContainerName = StartupConfig.GetConfigurationVariable("CHARGES_DB_NAME");
            var chargeLinkContainerName = StartupConfig.GetConfigurationVariable("CHARGELINK_DB_NAME");
            var marketRolesContainerName = StartupConfig.GetConfigurationVariable("MARKETROLES_DB_NAME");
            var meteringPointContainerName = StartupConfig.GetConfigurationVariable("METERINGPOINTS_DB_NAME");

            // Configuration
            var generatorSettings = new GeneratorSettings()
            {
                MasterDataStorageConnectionString = masterDataStorageConnectionString,
                ChargesContainerName = chargesContainerName,
                ChargeLinkContainerName = chargeLinkContainerName,
                MarketRolesContainerName = marketRolesContainerName,
                MeteringPointContainerName = meteringPointContainerName,
            };

            builder.Services.AddSingleton(generatorSettings);
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
