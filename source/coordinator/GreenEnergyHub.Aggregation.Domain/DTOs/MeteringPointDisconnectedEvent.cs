using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointDisconnectedEvent : EventBase
    {
        public MeteringPointDisconnectedEvent(string meteringPointId)
        {
            Id = meteringPointId;
        }

        public override string Id { get; }

        public bool Connected => false;

        public override Instant EffectuationDate { get; set; }

        public override void Mutate(IReplayableObject replayableObject)
        {
            if (replayableObject == null)
            {
                throw new ArgumentNullException(nameof(replayableObject));
            }

            var meteringPoint = (MeteringPoint)replayableObject;
            meteringPoint.Connected = Connected;
        }
    }
}
