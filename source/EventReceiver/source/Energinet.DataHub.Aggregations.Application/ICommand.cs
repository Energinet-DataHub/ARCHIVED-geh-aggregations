using Energinet.DataHub.Aggregations.Application.Transport;
using MediatR;

namespace Energinet.DataHub.Aggregations.Application
{
#pragma warning disable CA1040
    /// <summary>
    /// CQRS command object
    /// </summary>
    public interface ICommand : IRequest, IOutboundMessage, IInboundMessage
    {
    }
#pragma warning restore
}
