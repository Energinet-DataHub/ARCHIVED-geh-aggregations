using System;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class EventWrapper
    {
        public EventWrapper()
        {
        }

        public EventWrapper(int sequenceNumber, string meteringPointId, object dataObject)
        {
            Id = Guid.NewGuid();
            SequenceNumber = sequenceNumber;
            MeteringPointId = meteringPointId;

            if (dataObject == null)
            {
                throw new ArgumentNullException();
            }

            EventName = dataObject.GetType().FullName;
            AssemblyName = dataObject.GetType().Assembly.FullName;
            Data = JsonSerializer.Serialize(dataObject);
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        public int SequenceNumber { get; set; }

        public string EventName { get; set; }

        public string AssemblyName { get; set; }

        public string MeteringPointId { get; set; }

        public string Data { get; set; }
    }
}
