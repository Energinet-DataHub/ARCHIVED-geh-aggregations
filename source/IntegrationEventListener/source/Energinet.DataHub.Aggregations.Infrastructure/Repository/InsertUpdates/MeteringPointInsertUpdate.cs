using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain.MasterData;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository.InsertUpdates
{
    internal class MeteringPointInsertUpdate<T> : IInsertUpdate<IMasterDataObject>
    where T : MeteringPoint
    {
        public string UpdateSql =>
            @"UPDATE MeteringPoint SET
                    [ConnectionState] = @ConnectionState,
                    [SettlementMethod] = @SettlementMethod,
                    [MeteringPointType] = @MeteringPointType,
                    [FromDate] = @FromDate,
                    [ToDate] = @ToDate
                    WHERE RowId = @RowId;";

        public string InsertSql =>
            @"INSERT INTO dbo.MeteringPoint (RowId, Id, ConnectionState, MeteringPointType, SettlementMethod, FromDate, ToDate)
                VALUES (@RowId, @Id, @ConnectionState, @MeteringPointType, @SettlementMethod, @FromDate, @ToDate)";

        public object UpdateParameters(IMasterDataObject masterDataObject)
        {
            var meteringPoint = (T)masterDataObject;
            return new
            {
                meteringPoint.RowId,
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod,
                meteringPoint.MeteringPointType,
                meteringPoint.FromDate,
                meteringPoint.ToDate,
            };
        }

        public object InsertParameters(IMasterDataObject masterDataObject)
        {
            var meteringPoint = (T)masterDataObject;
            return new
            {
                meteringPoint.RowId,
                meteringPoint.Id,
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod,
                meteringPoint.MeteringPointType,
                meteringPoint.FromDate,
                meteringPoint.ToDate,
            };
        }
    }
}
