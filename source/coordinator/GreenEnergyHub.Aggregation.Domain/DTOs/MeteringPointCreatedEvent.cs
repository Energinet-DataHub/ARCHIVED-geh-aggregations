using System;
using System.Collections.Generic;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointCreatedEvent : IEvent
    {
        public MeteringPointCreatedEvent()
        {
        }

        public MeteringPointCreatedEvent(string meteringPointId)
        {
            Id = meteringPointId;
        }

        public string Id { get; set; }

        public string MeteringPointType { get; set; }

        public string SettlementMethod { get; set; }

        public bool Connected { get; set; }

        public Instant EffectuationDate { get; set; }

        public List<IReplayableObject> GetObjectsAfterMutate(List<IReplayableObject> replayableObjects, Instant effectuationDate)
        {
            if (replayableObjects == null)
            {
                throw new ArgumentNullException(nameof(replayableObjects));
            }

            replayableObjects.Add(new MeteringPoint()
            {
                MeteringPointType = MeteringPointType,
                SettlementMethod = SettlementMethod,
                Connected = Connected,
                MeteringPointId = Id,
                ValidFrom = EffectuationDate,
                ValidTo = Instant.MaxValue,
            });
            return replayableObjects;
        }
    }
}
