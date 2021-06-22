using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Infrastructure.CosmosDb;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.ChangeFeedListener
{
    public class ChangeFeedListenerFunction
    {
        private const string EndpointUrl = "https://localhost:8081";
        private const string AuthorizationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string DatabaseId = "Events";
        private readonly CosmosEventStore _eventStore;

        public ChangeFeedListenerFunction()
        {
            _eventStore = new CosmosEventStore(EndpointUrl, AuthorizationKey, DatabaseId, "MeteringPointEvents");
        }

        [FunctionName("ChangeFeedListener")]
        public async Task ChangeFeedListener(
            [CosmosDBTrigger(
            databaseName: "Events",
            collectionName: "MeteringPointEvents",
            ConnectionStringSetting = "CosmosDbEventsConnectionString",
            LeaseCollectionName = "MeteringPointEventsLeases",
            CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                var mp_id = input[0].GetPropertyValue<string>("MeteringPointId");
                var evts = _eventStore.LoadStreamAsync(mp_id);
            }
        }
    }
}
