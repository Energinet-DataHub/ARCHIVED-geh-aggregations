using System;
using System.Collections.Generic;

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

        public List<MeteringPoint> GetObjectsAfterMutate(List<MeteringPoint> meteringPoints)
        {
            meteringPoints.Add(new MeteringPoint()
            {
                MeteringPointType = MeteringPointType,
                SettlementMethod = SettlementMethod,
                Connected = Connected,
                MeteringPointId = MeteringPointId,
                ValidFrom = DateTime.Parse(EffectuationDate),
                ValidTo = DateTime.MaxValue,
            });
            return meteringPoints;
        }
    }
}
