using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.MeteringPointMessage
{
    public class PointsDto
    {
        public double Quantity { get; set; }

        public string Quality { get; set; }

        public Instant Time { get; set; }
    }
}
