using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf
{
    public class PostOfficeServiceBusChannel : ServiceBusChannelBase<PostOfficeServiceBusChannel>
    {
        public PostOfficeServiceBusChannel(string connectionString, string topic, ILogger<PostOfficeServiceBusChannel> logger)
            : base(connectionString, topic, logger)
        {
        }
    }
}
