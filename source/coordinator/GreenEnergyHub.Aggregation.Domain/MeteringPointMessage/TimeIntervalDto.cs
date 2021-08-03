using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.MeteringPointMessage
{
    public class TimeIntervalDto
    {
        public Instant Start { get; set; }

        public Instant End { get; set; }
    }
}
