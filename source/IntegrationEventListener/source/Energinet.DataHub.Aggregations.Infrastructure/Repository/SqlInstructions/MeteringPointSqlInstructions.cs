// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Aggregations.Domain.MasterData.MeteringPoints;
using Energinet.DataHub.Aggregations.Infrastructure.Repository.SqlInstructions;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository.InsertUpdates
{
    internal class MeteringPointSqlInstructions<T> : ISqlInstructions<IMasterDataObject>
    where T : MeteringPoint
    {
        public string Update =>
            @"UPDATE MeteringPoint SET
                    [ConnectionState] = @ConnectionState,
                    [SettlementMethod] = @SettlementMethod,
                    [MeteringPointType] = @MeteringPointType,
                    [FromDate] = @FromDate,
                    [ToDate] = @ToDate
                    WHERE RowId = @RowId;";

        public string Insert =>
            @"INSERT INTO dbo.MeteringPoint (RowId, MeteringPointId, MeteringPointType, SettlementMethod, GridArea, ConnectionState, Resolution, MeteringMethod, Unit ,Product, FromDate, ToDate)
                VALUES (@RowId, @MeteringPointId, @MeteringPointType, @SettlementMethod, @GridArea, @ConnectionState, @Resolution, @MeteringMethod,@Unit, @Product, @FromDate, @ToDate)";

        public string Get =>
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
                meteringPoint.MeteringPointId,
                meteringPoint.ConnectionState,
                meteringPoint.SettlementMethod,
                meteringPoint.MeteringPointType,
                FromDate = meteringPoint.FromDate.ToIso8601GeneralString(),
                ToDate = meteringPoint.ToDate.ToIso8601GeneralString(),
                meteringPoint.GridArea,
                meteringPoint.InGridArea,
                meteringPoint.OutGridArea,
                meteringPoint.Resolution,
                meteringPoint.MeteringMethod,
                ParentMeteringPoint = meteringPoint.ParentMeteringPointId,
                meteringPoint.Unit,
                meteringPoint.Product,
            };
        }
    }
}
