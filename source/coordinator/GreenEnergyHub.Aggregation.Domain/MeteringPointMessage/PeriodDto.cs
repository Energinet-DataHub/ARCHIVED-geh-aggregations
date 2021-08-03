namespace GreenEnergyHub.Aggregation.Domain.MeteringPointMessage
{
   public class PeriodDto
    {
        public string Resolution { get; set; }

        public TimeIntervalDto TimeInterval { get; set; }

        public PointsDto Points { get; set; }
    }
}
