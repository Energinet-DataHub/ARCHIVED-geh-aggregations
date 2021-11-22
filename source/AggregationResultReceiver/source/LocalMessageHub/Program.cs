using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;
using Energinet.DataHub.MessageHub.Client;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Client.Factories;
using Energinet.DataHub.MessageHub.Client.Storage;
using Energinet.DataHub.MessageHub.Model.Dequeue;
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
                    var blobStorageConnectionString = Environment.GetEnvironmentVariable("BlobStorageConnectionString");
                    var dataAvailableQueueName = Environment.GetEnvironmentVariable("DataAvailableQueueName");
                    var replyQueueName = Environment.GetEnvironmentVariable("ReplyQueueName");

                    // Add custom services
                    services.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
                    services.AddSingleton(_ => new BlobServiceClient(blobStorageConnectionString));
                    services.AddSingleton<IStorageHandler, StorageHandler>();

                    services.AddScoped(_ => new StorageConfig("postoffice-blobstorage"));
                    services.AddScoped(_ => new MessageHubConfig(dataAvailableQueueName!, replyQueueName!));

                    services.AddSingleton<IServiceBusClientFactory>(_ => new ServiceBusClientFactory(serviceBusConnectionString!));
                    services.AddSingleton<IStorageServiceClientFactory>(_ => new StorageServiceClientFactory(blobStorageConnectionString!));
                    services.AddSingleton<IMessageBusFactory, AzureServiceBusFactory>();

                    services.AddScoped<IDataAvailableNotificationSender, DataAvailableNotificationSender>();
                    services.AddScoped(typeof(IDequeueNotificationParser), typeof(DequeueNotificationParser));
                })
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }
    }
}
