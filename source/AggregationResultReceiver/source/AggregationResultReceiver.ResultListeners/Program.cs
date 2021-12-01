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

using System.IO;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Configurations;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Serialization;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Serialization;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Storage;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.ResultListeners
{
    public static class Program
    {
        public static void Main()
        {
            var host = new HostBuilder().ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.SetBasePath(Directory.GetCurrentDirectory());
                configurationBuilder.AddJsonFile("local.settings.json", true, true);
                configurationBuilder.AddEnvironmentVariables();
            }).ConfigureFunctionsWorkerDefaults();

            var buildHost = host.ConfigureServices((context, services) =>
            {
                using var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
                telemetryConfiguration.InstrumentationKey = context.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
                var logger = new LoggerConfiguration()
                    .WriteTo.Console()
                    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
                    .CreateLogger();

                services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger));
                services.AddSingleton<IJsonSerializer, JsonSerializer>();
                services.AddSingleton<IGuidGenerator, GuidGenerator>();
                services.AddSingleton<IInstantGenerator, InstantGenerator>();
                services.AddSingleton<ICimXmlConverter, CimXmlConverter>();
                services.AddSingleton<IFileStore, FileStore>();
                services.AddSingleton(new FileStoreConfiguration(
                    context.Configuration["RESULT_RECEIVER_BLOB_STORAGE_CONNECTION_STRING"],
                    context.Configuration["AGGREGATION_RESULTS_CONTAINER_NAME"],
                    context.Configuration["CONVERTED_MESSAGES_CONTAINER_NAME"]));
            }).Build();

            buildHost.Run();
        }
    }
}
