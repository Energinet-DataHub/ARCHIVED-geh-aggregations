using System;
using Domain.Models;

namespace Domain.DTOs
{
    public class MeteringPointSettlementMethodChangedEvent : EventBase
    {
        public SettlementMethod? SettlementMethod { get; set; }

        public override string Id { get; set; }

        public override DateTime EffectiveDate { get; set; }

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
