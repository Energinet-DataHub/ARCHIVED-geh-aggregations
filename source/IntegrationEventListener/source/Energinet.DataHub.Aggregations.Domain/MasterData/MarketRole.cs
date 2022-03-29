using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Domain.MasterData
{
    internal class MarketRole : IMasterDataObject
    {
        public Instant FromDate { get; set; }

        public Instant ToDate { get; set; }

        public Guid RowId { get; set; }

        public T ShallowCopy<T>()
            where T : IMasterDataObject
        {
            throw new NotImplementedException();
        }
    }
}
