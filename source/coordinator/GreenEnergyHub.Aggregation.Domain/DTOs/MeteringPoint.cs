using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPoint : IReplayableObject
    {
        public string MeteringPointId { get; set; }

        public string MeteringPointType { get; set; }

        public string SettlementMethod { get; set; }

        public bool Connected { get; set; }

        public Instant ValidFrom { get; set; }

        public Instant ValidTo { get; set; }

        public IReplayableObject ShallowCopy()
        {
            return (IReplayableObject)MemberwiseClone();
        }
    }
}
