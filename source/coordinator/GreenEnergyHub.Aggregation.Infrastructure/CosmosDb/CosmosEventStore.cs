using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventListener;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using Microsoft.Azure.Cosmos;

namespace GreenEnergyHub.Aggregation.Infrastructure.CosmosDb
{
    public class CosmosEventStore : IEventStore
    {
        private readonly CosmosClient _client;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosEventStore(
            string endpointUrl,
            string authorizationKey,
            string databaseId,
            string containerId)
        {
            _client = new CosmosClient(endpointUrl, authorizationKey);
            _databaseId = databaseId;
            _containerId = containerId;
        }

        public async Task<EventStream> LoadStreamAsync(string meteringPointId)
        {
            var container = _client.GetContainer(_databaseId, _containerId);

            var sqlQueryText = "SELECT * FROM events e"
                + " WHERE e.MeteringPointId = @meteringPointId"
                + " ORDER BY e.SequenceNumber";

            var queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@meteringPointId", meteringPointId);

            var version = 0;
            var events = new List<IEvent>();

            var feedIterator = container.GetItemQueryIterator<string>(queryDefinition);
            while (feedIterator.HasMoreResults)
            {
                var response = await feedIterator.ReadNextAsync();
                foreach (var evt in response)
                {
                    var x = evt;
                }
            }

            return null;
        }

        public async Task<EventStream> LoadStreamAsync(string streamId, int fromVersion)
        {
            var container = _client.GetContainer(_databaseId, _containerId);

            var sqlQueryText = "SELECT * FROM events e"
                + " WHERE e.stream.id = @streamId AND e.stream.version >= @fromVersion"
                + " ORDER BY e.stream.version";

            var queryDefinition = new QueryDefinition(sqlQueryText)
                .WithParameter("@streamId", streamId)
                .WithParameter("@fromVersion", fromVersion);

            int version = 0;
            var events = new List<IEvent>();

            var feedIterator = container.GetItemQueryIterator<EventWrapper>(queryDefinition);
            //while (feedIterator.HasMoreResults)
            //{
            //    FeedResponse<EventWrapper> response = await feedIterator.ReadNextAsync();
            //    foreach (var eventWrapper in response)
            //    {
            //        version = eventWrapper.StreamInfo.Version;

            //        events.Add(eventWrapper.GetEvent(_eventTypeResolver));
            //    }
            //}
            return new EventStream(streamId, version, events);
        }

        public async Task<bool> AppendToStreamAsync(string meteringPointId, EventWrapper @event)
        {
            var container = _client.GetContainer(_databaseId, _containerId);

            var partitionKey = new PartitionKey(meteringPointId);

            await container.CreateItemAsync(@event, partitionKey).ConfigureAwait(false);

            return true;
        }
    }
}
