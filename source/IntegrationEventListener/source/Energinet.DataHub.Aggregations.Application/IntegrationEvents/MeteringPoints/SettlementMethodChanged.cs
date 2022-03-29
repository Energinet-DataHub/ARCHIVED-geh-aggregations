using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints
{
    public record SettlementMethodChanged(
#pragma warning disable SA1313

        string MeteringPointId,
        SettlementMethod SettlementMethod,
        Instant EffectiveDate)
        : EventBase, IInboundMessage
    {
        public Transaction Transaction { get; set; }

        public override string Id => MeteringPointId;

        public override void Mutate(IMasterDataObject masterDataObject)
        {
            if (masterDataObject == null)
            {
                throw new ArgumentNullException(nameof(masterDataObject));
            }

            var meteringPoint = (MeteringPoint)masterDataObject;
            meteringPoint.SettlementMethod = SettlementMethod;
        }
    }
#pragma warning restore SA1313
}
