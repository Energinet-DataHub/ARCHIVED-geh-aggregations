using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Infrastructure.Repository.SqlInstructions;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository.InsertUpdates
{
    internal class MeteringPointSqlInstructions<T> : ISqlInstructions<IMasterDataObject>
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
            @"INSERT INTO dbo.MeteringPoint (RowId, MeteringPointId, MeteringPointType, SettlementMethod, GridArea, ConnectionState, Resolution, MeteringMethod, Unit , FromDate, ToDate)
                VALUES (@RowId, @Id, @MeteringPointType, @SettlementMethod, @GridArea, @ConnectionState, @Resolution, @MeteringMethod,@Unit, @FromDate, @ToDate)";

        public string GetSql =>
            $"SELECT md.* FROM dbo.{typeof(T).Name} md WHERE md.MeteringPointId = @id AND md.ToDate > @effectiveDate;";

        public object UpdateParameters(IMasterDataObject masterDataObject)
        {
            var meteringPoint = (T)masterDataObject;
            return new
            {
                meteringPoint.RowId,
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod,
                meteringPoint.MeteringPointType,
                FromDate = meteringPoint.FromDate.ToIso8601GeneralString(),
                ToDate = meteringPoint.ToDate.ToIso8601GeneralString(),
            };
        }

        public object InsertParameters(IMasterDataObject masterDataObject)
        {
            var meteringPoint = (T)masterDataObject;
            return new
            {
                meteringPoint.RowId,
                meteringPoint.Id,
                meteringPoint.MeteringPointType,
                meteringPoint.SettlementMethod,
                meteringPoint.GridArea,
                meteringPoint.ConnectionState,
                meteringPoint.Resolution,
                meteringPoint.MeteringMethod,
                meteringPoint.Unit,
                FromDate = meteringPoint.FromDate.ToIso8601GeneralString(),
                ToDate = meteringPoint.ToDate.ToIso8601GeneralString(),
            };
        }
    }
}
