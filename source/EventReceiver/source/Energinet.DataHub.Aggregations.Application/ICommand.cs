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

    /// <summary>
    /// CQRS command with result
    /// </summary>
    /// <typeparam name="TResult"><see cref="IRequest"/></typeparam>
    public interface ICommand<out TResult> : IRequest<TResult>, IOutboundMessage, IInboundMessage
    {
    }
#pragma warning restore
}
