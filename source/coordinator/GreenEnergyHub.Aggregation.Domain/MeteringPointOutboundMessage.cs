using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Aggregation.Domain
{
    public class MeteringPointOutboundMessage : IOutboundMessage
    {
        public MeteringPointOutboundMessage(MeteringPointMessage meteringPointMessage)
        {
            MeteringPointMessage = meteringPointMessage;
        }

        public MeteringPointMessage MeteringPointMessage { get; }

        public Transaction Transaction { get; set; }
    }
}
