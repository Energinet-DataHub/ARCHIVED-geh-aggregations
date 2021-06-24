using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointConnectedEvent : EventBase<MeteringPoint>
    {
        public MeteringPointConnectedEvent(string meteringPointId)
        {
            Id = meteringPointId;
        }

        public override string Id { get; }

        public bool Connected => true;

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
