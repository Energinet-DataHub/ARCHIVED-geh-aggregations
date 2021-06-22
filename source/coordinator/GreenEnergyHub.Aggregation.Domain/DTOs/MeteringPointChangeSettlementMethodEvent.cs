using System;
using System.Collections.Generic;
using System.Text;
using EventListener;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
  public class MeteringPointChangeSettlementMethodEvent : EventBase
    {
        public MeteringPointChangeSettlementMethodEvent(string meteringPointId, string settlementMethod)
        {
            EventName = GetType().Name;
            MeteringPointId = meteringPointId;
            SettlementMethod = settlementMethod;
        }

        public string SettlementMethod { get; }

        public string MeteringPointId { get; }

        public string EffectuationDate { get; set; }
    }
}
