using System;
using System.Collections.Generic;
using System.Text;
using EventListener;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
   public class MeteringPointConnectedEvent
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
