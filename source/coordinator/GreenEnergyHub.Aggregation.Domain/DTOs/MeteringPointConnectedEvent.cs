using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
   public class MeteringPointConnectedEvent : IEvent
    {
        public MeteringPointConnectedEvent(string meteringPointId)
        {
            MeteringPointId = meteringPointId;
        }

        public string MeteringPointId { get; }

        public bool Connected => true;

        public string EffectuationDate { get; set; }
    }
}
