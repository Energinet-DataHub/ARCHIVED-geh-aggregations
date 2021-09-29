using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using GreenEnergyHub.Aggregation.Domain.DTOs;

namespace GreenEnergyHub.Aggregation.Infrastructure.TableStorage
{
    public class TableEventStore : IEventStore
    {
        private const string StorageUri = "https://timeseriesdatajouless.table.core.windows.net";
        private const string AccountName = "timeseriesdatajouless";
        private const string StorageAccountKey =
            "KIGkX7XIGilmUwADO5qT7k4AzFk8S0nl5QmfBD3+ktHxqhoVwfQUPeV7DpMxvhmTNVe/bcdJwA6PIyUHuvqKZw==";

        private readonly TableClient _tableClient;

        public TableEventStore()
        {
            var tableName = "Events";
            try
            {
                _tableClient = new TableClient(
                    new Uri(StorageUri),
                    tableName,
                    new TableSharedKeyCredential(AccountName, StorageAccountKey));

                // Create the table in the service.
                _tableClient.CreateIfNotExists();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IEnumerable<IEvent>> LoadStreamAsync(string meteringPointId)
        {
            var queryResults = _tableClient.QueryAsync<TableEntity>(filter: $"PartitionKey eq '{meteringPointId}'");
            var events = new List<IEvent>();

            await foreach (var entity in queryResults)
            {
                var json = entity.GetString("object");
                var evt = JsonSerializer.Deserialize<EventWrapper>(json);
                var eventType = Type.GetType($"{evt.EventName}, {evt.AssemblyName}");
                events.Add((IEvent)JsonSerializer.Deserialize(evt.Data, eventType));
            }

            return events;
        }

        public Task<IEnumerable<IEvent>> LoadStreamAsync(string streamId, int fromVersion)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AppendToStreamAsync(string meteringPointId, EventWrapper eventObject)
        {
            var entity = new TableEntity(meteringPointId, Guid.NewGuid().ToString());
            entity.Add("object", JsonSerializer.Serialize(eventObject));

            var response = await _tableClient.AddEntityAsync(entity).ConfigureAwait(false);
            return response.Status == 204;
        }
    }
}
