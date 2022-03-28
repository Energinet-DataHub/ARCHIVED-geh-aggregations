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

        string id,
        SettlementMethod SettlementMethod,
        Instant EffectiveDate)
        : EventBase, IInboundMessage
    {
        public override void Mutate(IReplayableObject replayableObject)
        {
            if (replayableObject == null)
            {
                throw new ArgumentNullException(nameof(replayableObject));
            }

            var meteringPoint = (MeteringPoint)replayableObject;
            meteringPoint.SettlementMethod = SettlementMethod;
        }

        public Transaction Transaction { get; set; }
    }
#pragma warning restore SA1313
}
