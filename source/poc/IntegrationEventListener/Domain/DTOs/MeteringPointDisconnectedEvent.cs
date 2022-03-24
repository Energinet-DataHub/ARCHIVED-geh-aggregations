using System;
using Domain.Models;

namespace Domain.DTOs
{
    public class MeteringPointDisconnectedEvent : EventBase
    {

        public override string Id { get; set; }

        public ConnectionState ConnectionState => ConnectionState.Disconnected;

        public override DateTime EffectiveDate { get; set; }

        public override void Mutate(IReplayableObject replayableObject)
        {
            if (replayableObject == null)
            {
                throw new ArgumentNullException(nameof(replayableObject));
            }

            var meteringPoint = (MeteringPoint)replayableObject;
            meteringPoint.ConnectionState = ConnectionState;
        }
    }
}
