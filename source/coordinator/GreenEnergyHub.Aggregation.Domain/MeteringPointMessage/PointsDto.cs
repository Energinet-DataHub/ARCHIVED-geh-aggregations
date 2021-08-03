using Google.Protobuf.WellKnownTypes;

namespace GreenEnergyHub.Aggregation.Domain.MeteringPointMessage
{
    public class PointsDto
    {
        public double Quantity { get; set; }

        public string Quality { get; set; }

        public Timestamp Time { get; set; }
    }
}
