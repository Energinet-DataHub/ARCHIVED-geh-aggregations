namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointCreatedEvent : IEvent
    {
        public MeteringPointCreatedEvent(string meteringPointId)
        {
            MeteringPointId = meteringPointId;
        }

        public string MeteringPointId { get; }

        public string MeteringPointType { get; set; }

        public string SettlementMethod { get; set; }

        public bool Connected { get; set; }

        public string EffectuationDate { get; set; }
    }
}
