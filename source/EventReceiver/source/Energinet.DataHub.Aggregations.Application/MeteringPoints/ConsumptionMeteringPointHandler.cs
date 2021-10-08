using System.Threading;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using MediatR;

namespace Energinet.DataHub.Aggregations.Application.MeteringPoints
{
    public class ConsumptionMeteringPointHandler : IRequestHandler<ConsumptionMeteringPointCommand>
    {
        private readonly IEventDispatcher _eventDispatcher;
        private readonly IJsonSerializer _jsonSerializer;

        public ConsumptionMeteringPointHandler(IEventDispatcher eventDispatcher, IJsonSerializer jsonSerializer)
        {
            _eventDispatcher = eventDispatcher;
            _jsonSerializer = jsonSerializer;
        }

        public async Task<Unit> Handle(ConsumptionMeteringPointCommand request, CancellationToken cancellationToken)
        {
            var serializedMessage = _jsonSerializer.Serialize(request);
            await _eventDispatcher.DispatchAsync(serializedMessage, cancellationToken).ConfigureAwait(false);
            return Unit.Value;
        }
    }
}
