using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
  public class MeteringPointChangeSettlementMethodEvent : IEvent
    {
        public MeteringPointChangeSettlementMethodEvent(string meteringPointId, string settlementMethod)
        {
            MeteringPointId = meteringPointId;
            SettlementMethod = settlementMethod;
        }

        public string SettlementMethod { get; }

        public string MeteringPointId { get; }

        public string EffectuationDate { get; set; }

        public List<MeteringPoint> GetObjectsAfterMutate(List<MeteringPoint> meteringPoints)
        {
            throw new NotImplementedException();
        }
    }
}
