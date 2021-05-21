using System.Text.Json.Serialization;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class TimeWindowDto
    {
        [JsonPropertyName("start")]
        public Instant Start { get; set; }

        [JsonPropertyName("end")]
        public Instant End { get; set; }
    }
}
