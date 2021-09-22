using System.IO;
using GreenEnergyHub.Aggregation.Application;
using GreenEnergyHub.Aggregation.Application.Interfaces;
using GreenEnergyHub.Aggregation.Infrastruct;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace GreenEnergyHub.Aggregation.ServiceBusToEvtHubFunction
{
    public class Program
    {
        public static void Main()
        {
            // wire up configuration
            var host = new HostBuilder().ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                    configurationBuilder.AddJsonFile("local.settings.json", true, true);
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults();
            //wire up DI
            var buildHost = host.ConfigureServices((context, services) =>
            {
              // Setup Serilog
                using var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                var logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                    .CreateLogger();

                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
                services.AddSingleton<IEventDispatcher, EventDispatcher>();
                services.AddSingleton<IEventHubService, EventHubService>();
            }).Build();

            buildHost.Run();
        }
    }
}
