using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators
{
    public class MeteringPointConnectedMutator : MutatorBase
    {
        private readonly MeteringPointConnectedEvent _event;

        public MeteringPointConnectedMutator(MeteringPointConnectedEvent @event)
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
            meteringPoint.ConnectionState = ConnectionState.Connected;
        }
    }
}
