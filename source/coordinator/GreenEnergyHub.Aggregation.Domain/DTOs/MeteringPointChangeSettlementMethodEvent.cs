using System;
using System.Collections.Generic;
using System.Text;
using EventListener;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
  public class MeteringPointChangeSettlementMethodEvent
    {
        public MeteringPointChangeSettlementMethodEvent(string meteringPointId, string settlementMethod)
        {
            MeteringPointId = meteringPointId;
            SettlementMethod = settlementMethod;
        }

        public string SettlementMethod { get; }

        public string MeteringPointId { get; }

        public string EffectuationDate { get; set; }
    }
}
