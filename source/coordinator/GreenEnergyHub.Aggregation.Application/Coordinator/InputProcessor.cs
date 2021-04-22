using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class InputProcessor : IInputProcessor
    {
        private readonly ILogger<InputProcessor> _logger;
        private readonly IEnumerable<IDispatchStrategy> _dispatchStrategies;

        public InputProcessor(ILogger<InputProcessor> logger, IEnumerable<IDispatchStrategy> dispatchStrategies)
        {
            _logger = logger;
            if (null == dispatchStrategies || !dispatchStrategies.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(dispatchStrategies), "is null or empty");
            }
        }

        public async Task ProcessInputAsync(string nameOfAggregation, Stream blobStream, CancellationToken cancellationToken)
        {
            var strategy = FindStrategy(nameOfAggregation);
            await strategy.DispatchAsync(blobStream, cancellationToken).ConfigureAwait(false);
        }

        private IDispatchStrategy FindStrategy(string nameOfAggregation)
        {
            var foundDispatchStrategy = _dispatchStrategies.FirstOrDefault(s => s.FriendlyNameInstance.Equals(nameOfAggregation, StringComparison.OrdinalIgnoreCase));

            if (null == foundDispatchStrategy)
            {
                _logger.LogWarning($"IDispatchStrategy not found in input processor map. ('{nameOfAggregation}')");
            }

            return foundDispatchStrategy;
        }
    }
}
