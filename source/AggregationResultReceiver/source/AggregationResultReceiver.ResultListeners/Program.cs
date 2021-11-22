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
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
                services.AddSingleton<IGuidGenerator, GuidGenerator>();
                services.AddSingleton<IInstantGenerator, InstantGenerator>();
                services.AddSingleton(new BlockBlobClientGenerator(
                    context.Configuration["CONNECTION_STRING"],
                    context.Configuration["CONTAINER_NAME"]));
            }).Build();

            buildHost.Run();
        }
    }
}
