using Google.Protobuf;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf
{
    public class MeteringPointMessageToDtoMapper : ProtobufOutboundMapper<MeteringPointOutboundMessage>
    {
        protected override IMessage Convert(MeteringPointOutboundMessage obj)
        {
            return obj.MeteringPointMessage;
        }
    }
}
