using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Infrastructure.CosmosDb;
using GreenEnergyHub.Aggregation.Infrastructure.TableStorage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NodaTime.Text;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace EventListener
{
    public class EventListener
    {
        private readonly TableEventStore _eventStore;

        public EventListener()
        {
            _eventStore = new TableEventStore();
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
                EffectuationDate = InstantPattern.General.Parse(req.Query["EffectuationDate"]).GetValueOrThrow(),
            };

            var eventWrapper = new EventWrapper(0, meteringPointCreatedEvent.Id, meteringPointCreatedEvent);

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointCreatedEvent.Id,
                eventWrapper).ConfigureAwait(false);

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
                EffectuationDate = InstantPattern.General.Parse(req.Query["EffectuationDate"]).GetValueOrThrow(),
            };

            var eventWrapper = new EventWrapper(int.Parse(req.Query["SequenceNumber"]), meteringPointConnectedEvent.Id, meteringPointConnectedEvent);

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointConnectedEvent.Id,
                eventWrapper).ConfigureAwait(false);

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
                EffectuationDate = InstantPattern.General.Parse(req.Query["EffectuationDate"]).GetValueOrThrow(),
            };

            var eventWrapper = new EventWrapper(int.Parse(req.Query["SequenceNumber"]), meteringPointConnectedEvent.Id, meteringPointConnectedEvent);

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointConnectedEvent.Id,
                eventWrapper).ConfigureAwait(false);

            return new OkObjectResult(succes);
        }

        [FunctionName("DisconnectEvent")]
        public async Task<IActionResult> DisconnectEvent(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var meteringPointDisconnectedEvent = new MeteringPointDisconnectedEvent("87000001")
            {
                EffectuationDate = InstantPattern.General.Parse(req.Query["EffectuationDate"]).GetValueOrThrow(),
            };

            var eventWrapper = new EventWrapper(int.Parse(req.Query["SequenceNumber"]), meteringPointDisconnectedEvent.Id, meteringPointDisconnectedEvent);

            var succes = await _eventStore.AppendToStreamAsync(
                meteringPointDisconnectedEvent.Id,
                eventWrapper).ConfigureAwait(false);

            return new OkObjectResult(succes);
        }

        public Type GetEventType(string typeName)
        {
            return GetType();
        }

        [FunctionName("Trigger")]
        public async Task<IActionResult> Trigger(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            var mp_id = "87000001";
            var evts = await _eventStore.LoadStreamAsync(mp_id).ConfigureAwait(false);
            var list = new List<IReplayableObject>();
            foreach (var @event in evts)
            {
                list = @event.GetObjectsAfterMutate(list, @event.EffectuationDate);
            }

            return new OkObjectResult(JsonSerializer.Serialize(list.Select(o => (MeteringPoint)o)));
        }
    }
}
