using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class EventWrapper
    {
        public EventWrapper()
        {
        }

        public EventWrapper(int sequenceNumber, string meteringPointId, object dataObject)
        {
            SequenceNumber = sequenceNumber;
            MeteringPointId = meteringPointId;

            if (dataObject == null)
            {
                throw new ArgumentNullException();
            }

            EventName = dataObject.GetType().Name;
            Data = JObject.FromObject(dataObject);
        }

        [JsonProperty("id")]
        public Guid Id { get; } = Guid.NewGuid();

        public int SequenceNumber { get; }

        public string EventName { get; }

        public string MeteringPointId { get; }

        public JObject Data { get; }
    }
}
