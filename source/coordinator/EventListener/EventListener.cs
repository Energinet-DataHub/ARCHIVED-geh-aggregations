using System;
using System.IO;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Infrastructure.CosmosDb;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EventListener
{
    public class EventListener : IEventTypeResolver
    {
        private const string EndpointUrl = "https://localhost:8081";
        private const string AuthorizationKey = "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==";
        private const string DatabaseId = "Events";
        private readonly CosmosEventStore _eventStore;

        public EventListener()
        {
            _eventStore = new CosmosEventStore(this, EndpointUrl, AuthorizationKey, DatabaseId, "MeteringPointEvents");
        }

        [FunctionName("CreateEvent")]
        public async Task<IActionResult> CreateEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var meteringPointCreatedEvent = new MeteringPointCreatedEvent("87000001")
            {
                Connected = false,
                SettlementMethod = "D01",
                MeteringPointType = "E17",
                EffectuationDate = req.Query["EffectuationDate"],
                SequenceNumber = 0,
            };

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointCreatedEvent.MeteringPointId,
                meteringPointCreatedEvent).ConfigureAwait(false);

            return new OkObjectResult(succes);
        }

        [FunctionName("ConnectEvent")]
        public async Task<IActionResult> ConnectEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var meteringPointConnectedEvent = new MeteringPointConnectedEvent("87000001")
            {
                EffectuationDate = req.Query["EffectuationDate"],
                SequenceNumber = int.Parse(req.Query["SequenceNumber"]),
            };

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointConnectedEvent.MeteringPointId,
                meteringPointConnectedEvent).ConfigureAwait(false);

            return new OkObjectResult(succes);
        }

        [FunctionName("UpdateSettlementMethodEvent")]
        public async Task<IActionResult> UpdateSettlementMethodEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var meteringPointConnectedEvent = new MeteringPointChangeSettlementMethodEvent("87000001", req.Query["SettlementMethod"])
            {
                EffectuationDate = req.Query["EffectuationDate"],
                SequenceNumber = int.Parse(req.Query["SequenceNumber"]),
            };

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointConnectedEvent.MeteringPointId,
                meteringPointConnectedEvent).ConfigureAwait(false);

            return new OkObjectResult(succes);
        }

        [FunctionName("DisconnectEvent")]
        public async Task<IActionResult> DisconnectEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var meteringPointConnectedEvent = new MeteringPointDisconnectedEvent("87000001")
            {
                EffectuationDate = req.Query["EffectuationDate"],
                SequenceNumber = int.Parse(req.Query["SequenceNumber"]),
            };

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointConnectedEvent.MeteringPointId,
                meteringPointConnectedEvent).ConfigureAwait(false);

            return new OkObjectResult(succes);
        }

        public Type GetEventType(string typeName)
        {
            return GetType();
        }
    }
}
