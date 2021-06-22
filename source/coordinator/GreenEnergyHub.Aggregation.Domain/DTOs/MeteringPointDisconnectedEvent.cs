using System;
using System.Collections.Generic;
using System.Text;
using EventListener;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointDisconnectedEvent : EventBase
    {
        public MeteringPointDisconnectedEvent(string meteringPointId)
        {
            EventName = GetType().Name;
            MeteringPointId = meteringPointId;
        }

        public string MeteringPointId { get; }

        public bool Connected => false;

        public string EffectuationDate { get; set; }
    }
}
