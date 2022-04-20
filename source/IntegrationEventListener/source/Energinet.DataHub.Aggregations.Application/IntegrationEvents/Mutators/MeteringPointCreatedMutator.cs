using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators
{
    public class MeteringPointCreatedMutator : MutatorBase
    {
        private readonly MeteringPointCreatedEvent _event;

        public MeteringPointCreatedMutator(MeteringPointCreatedEvent @event)
        {
            _event = @event;
        }

        public override Instant EffectiveDate { get; }

        public override string Id => _event.MeteringPointId;

        public List<T> GetObjectsAfterMutate<T>(List<T> replayableObjects, Instant effectiveDate)
            where T : IMasterDataObject
        {
            if (replayableObjects == null)
            {
                throw new ArgumentNullException(nameof(replayableObjects));
            }

            var mp = new MeteringPoint()
            {
                RowId = Guid.NewGuid(),
                MeteringPointType = _event.MeteringPointType,
                SettlementMethod = _event.SettlementMethod,
                ConnectionState = _event.ConnectionState,
                Id = _event.MeteringPointId,
                Unit = _event.Unit,
                GridArea = _event.GridArea,
                MeteringMethod = _event.MeteringMethod,
                Resolution = _event.Resolution,
                FromDate = EffectiveDate,
                ToDate = Instant.MaxValue,
            };

            replayableObjects.Add((T)Convert.ChangeType(mp, typeof(T), CultureInfo.InvariantCulture));
            return replayableObjects;
        }

        public override void Mutate(IMasterDataObject masterDataObject)
        {
            var meteringPoint = (MeteringPoint)masterDataObject;

            meteringPoint.RowId = Guid.NewGuid();
            meteringPoint.MeteringPointType = _event.MeteringPointType;
            meteringPoint.SettlementMethod = _event.SettlementMethod;
            meteringPoint.ConnectionState = _event.ConnectionState;
            meteringPoint.Id = _event.MeteringPointId;
            meteringPoint.Unit = _event.Unit;
            meteringPoint.GridArea = _event.GridArea;
            meteringPoint.MeteringMethod = _event.MeteringMethod;
            meteringPoint.Resolution = _event.Resolution;
            meteringPoint.FromDate = EffectiveDate;
            meteringPoint.ToDate = Instant.MaxValue;
        }
    }
}
