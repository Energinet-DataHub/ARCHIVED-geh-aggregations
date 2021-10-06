using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Transport;
using Energinet.DataHub.Aggregations.Infrastructure.Transport.Protobuf;

namespace Energinet.DataHub.Aggregations.Infrastructure.Transport
{
    public class MessagingService
    {
        private readonly IProtobufMessageFactory _protobufMessageFactory;
        private readonly ProtobufInboundMapperFactory _protobufInboundMapperFactory;

        public MessagingService(IProtobufMessageFactory protobufMessageFactory, ProtobufInboundMapperFactory protobufInboundMapperFactory)
        {
            _protobufMessageFactory = protobufMessageFactory;
            _protobufInboundMapperFactory = protobufInboundMapperFactory;
        }

        /// <summary>
        /// Handler for event message, to convert and map to corresponding inbound message
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="messageData"></param>
        /// <returns>Inbound message</returns>
        public Task<IInboundMessage> HandleEventMessageAsync(string eventName, byte[] messageData)
        {
            var message = _protobufMessageFactory.CreateMessageFrom(messageData, eventName);
            var mapper = _protobufInboundMapperFactory.GetMapper(message.GetType());
            var command = mapper.Convert(message);
            return Task.FromResult(command);
        }
    }
}
