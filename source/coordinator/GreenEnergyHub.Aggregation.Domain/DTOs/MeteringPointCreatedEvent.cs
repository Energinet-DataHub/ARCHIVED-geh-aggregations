namespace EventListener
{
    public class MeteringPointCreatedEvent : EventBase
    {
        public MeteringPointCreatedEvent(string meteringPointId)
        {
            EventName = GetType().Name;
            MeteringPointId = meteringPointId;
        }

        public string MeteringPointId { get; }

        public string MeteringPointType { get; set; }

        public string SettlementMethod { get; set; }

        public bool Connected { get; set; }

        public string EffectuationDate { get; set; }
    }
}
