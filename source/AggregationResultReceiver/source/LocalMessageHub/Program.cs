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
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Peek;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Dequeue;
using Energinet.DataHub.MessageHub.Model.Peek;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Energinet.DataHub.Aggregations.LocalMessageHub
{
    public static class Program
    {
        public static async Task Main()
        {
            var host = new HostBuilder()
                .ConfigureAppConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddEnvironmentVariables();
                })
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    // Add logging
                    services.AddLogging();

                    // Add Application insights telemetry
                    var appInsightsInstrumentationKey = Environment.GetEnvironmentVariable("APPINSIGHTS_INSTRUMENTATIONKEY") ?? string.Empty;
                    var appInsightsServiceOptions = new Microsoft.ApplicationInsights.WorkerService.ApplicationInsightsServiceOptions
                    {
                        InstrumentationKey = appInsightsInstrumentationKey,
                        EnableDependencyTrackingTelemetryModule = !string.IsNullOrWhiteSpace(appInsightsInstrumentationKey),
                    };

                    services.AddApplicationInsightsTelemetryWorkerService(appInsightsServiceOptions);

                    var serviceBusConnectionString = Environment.GetEnvironmentVariable("ServiceBusConnectionString");
                    var convertedMessagesBlobStorageConnectionString = Environment.GetEnvironmentVariable("ConvertedMessagesBlobStorageConnectionString");
                    var messageHubBlobStorageConnectionString = Environment.GetEnvironmentVariable("MessageHubBlobStorageConnectionString");
                    var dataAvailableQueueName = Environment.GetEnvironmentVariable("DataAvailableQueueName");
                    var replyQueueName = Environment.GetEnvironmentVariable("ReplyQueueName");
                    var messageHubFileStoreContainerName = Environment.GetEnvironmentVariable("MessageHubFileStoreContainerName");
                    var convertedMessagesFileStoreContainerName = Environment.GetEnvironmentVariable("ConvertedMessagesFileStoreContainerName");

                    // Add custom services
                    services.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
                    services.AddSingleton(_ => new BlobServiceClient(convertedMessagesBlobStorageConnectionString));
                    services.AddSingleton<IStorageHandler, StorageHandler>();

                    services.AddScoped(_ => new MessageHubConfig(dataAvailableQueueName!, replyQueueName!));
                    services.AddScoped(_ => new FileStorageConfiguration(
                        messageHubFileStoreContainerName!,
                        convertedMessagesFileStoreContainerName!));

                    services.AddSingleton<IServiceBusClientFactory>(_ => new ServiceBusClientFactory(serviceBusConnectionString!));
                    services.AddSingleton<IStorageServiceClientFactory>(_ => new StorageServiceClientFactory(messageHubBlobStorageConnectionString!));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();

                    services.AddScoped<IDataAvailableNotificationSender, DataAvailableNotificationSender>();
                    services.AddScoped(typeof(IDequeueNotificationParser), typeof(DequeueNotificationParser));
                    services.AddScoped<IFileStore, BlobStore>();
                    services.AddScoped<IRequestBundleParser, RequestBundleParser>();
                    services.AddScoped<IDataBundleResponseSender, DataBundleResponseSender>();
                    services.AddScoped<IResponseBundleParser, ResponseBundleParser>();
                    services.AddScoped<IStorageHandler, StorageHandler>();
                    services.AddScoped(_ => new StorageConfig(messageHubFileStoreContainerName!));
                })
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
