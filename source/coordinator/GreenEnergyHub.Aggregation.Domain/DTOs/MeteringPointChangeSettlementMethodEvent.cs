using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointChangeSettlementMethodEvent : EventBase
    {
        public MeteringPointChangeSettlementMethodEvent()
        {
        }

        public MeteringPointChangeSettlementMethodEvent(string meteringPointId, string settlementMethod)
        {
            Id = meteringPointId;
            SettlementMethod = settlementMethod;
        }

        public string SettlementMethod { get; }

        public override string Id { get; }

        public override Instant EffectuationDate { get; set; }

        public override void Mutate(IReplayableObject replayableObject)
        {
            if (replayableObject == null)
            {
                throw new ArgumentNullException(nameof(replayableObject));
            }

            var meteringPoint = (MeteringPoint)replayableObject;
            meteringPoint.SettlementMethod = SettlementMethod;
        }
    }
}
