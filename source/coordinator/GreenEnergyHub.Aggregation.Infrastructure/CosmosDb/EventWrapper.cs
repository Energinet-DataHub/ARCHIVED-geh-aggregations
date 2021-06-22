using System;
using System.Collections.Generic;
using System.Text;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GreenEnergyHub.Aggregation.Infrastructure.CosmosDb
{
    public class EventWrapper
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("stream")]
        public StreamInfo StreamInfo { get; set; }

        [JsonProperty("eventType")]
        public string EventType { get; set; }

        [JsonProperty("eventData")]
        public JObject EventData { get; set; }

        //public IEvent GetEvent(IEventTypeResolver eventTypeResolver)
        //{
        //    Type eventType = eventTypeResolver.GetEventType(EventType);

        //    return (IEvent)EventData.ToObject(eventType);
        //}
    }
}
