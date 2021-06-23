using GreenEnergyHub.Aggregation.Domain.DTOs;

namespace EventListener
{
    public class MeteringPointCreatedEvent
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
