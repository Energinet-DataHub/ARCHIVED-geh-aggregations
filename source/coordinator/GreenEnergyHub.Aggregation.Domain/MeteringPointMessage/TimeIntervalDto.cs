using Google.Protobuf.WellKnownTypes;

namespace GreenEnergyHub.Aggregation.Domain.MeteringPointMessage
{
    public class TimeIntervalDto
    {
        public Timestamp Start { get; set; }

        public Timestamp End { get; set; }
    }
}
