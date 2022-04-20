using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators
{
    public class SettlementMethodChangedMutator : MutatorBase
    {
        private readonly SettlementMethodChangedEvent _event;

        public SettlementMethodChangedMutator(SettlementMethodChangedEvent @event)
        {
            _event = @event;
        }

        public override Instant EffectiveDate => _event.EffectiveDate;

        public override string Id => _event.MeteringPointId;

        public override void Mutate(IMasterDataObject masterDataObject)
        {
            if (masterDataObject == null)
            {
                throw new ArgumentNullException(nameof(masterDataObject));
            }

            var meteringPoint = (MeteringPoint)masterDataObject;
            meteringPoint.SettlementMethod = _event.SettlementMethod;
        }
    }
}
