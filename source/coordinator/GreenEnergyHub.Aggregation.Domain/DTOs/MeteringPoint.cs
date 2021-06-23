using System;
using System.Collections.Generic;
using System.Text;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MeteringPoint
    {
        public string MeteringPointId { get; set; }

        public string MeteringPointType { get; set; }

        public string SettlementMethod { get; set; }

        public bool Connected { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }

        public MeteringPoint ShallowCopy()
        {
            return (MeteringPoint)MemberwiseClone();
        }
    }
}
