using System;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using Newtonsoft.Json;

namespace EventListener
{
    public abstract class EventBase : IEvent
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int SequenceNumber { get; set; }

        public string EventName { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
