using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf
{
    public class TimeseriesDispatcher : MessageDispatcher
    {
        public TimeseriesDispatcher(MessageSerializer serializer, TimeseriesServiceBusChannel channel)
            : base(serializer, channel)
        {
        }
    }
}
