using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.MasterData
{
    public class MeteringPoint : IReplayableObject
    {
        public Guid RowId { get; set; }

        public string Id { get; set; } = string.Empty;

        public ConnectionState ConnectionState { get; set; }

        public SettlementMethod? SettlementMethod { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public Instant FromDate { get; set; }

        public Instant ToDate { get; set; }

        public string GridArea { get; set; }

        public string Resolution { get; set; }

        public object InGridArea { get; set; }

        public object OutGridArea { get; set; }

        public string MeteringMethod { get; set; }

        public object ParentMeteringPoint { get; set; }

        public string Unit { get; set; }

        public string Product { get; set; }

        public T ShallowCopy<T>()
            where T : IReplayableObject
        {
            return (T)MemberwiseClone();
        }
    }
}
