using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPointDisconnectedEvent : IEvent
    {
        public MeteringPointDisconnectedEvent(string meteringPointId)
        {
            MeteringPointId = meteringPointId;
        }

        public string MeteringPointId { get; }

        public bool Connected => false;

        public string EffectuationDate { get; set; }
    }
}
