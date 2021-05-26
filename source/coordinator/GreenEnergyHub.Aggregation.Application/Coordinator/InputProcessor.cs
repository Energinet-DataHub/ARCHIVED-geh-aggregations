// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    public class InputProcessor : IInputProcessor
    {
        private readonly ILogger<InputProcessor> _logger;
        private readonly IMetaDataDataAccess _metaDataDataAccess;
        private readonly IEnumerable<IDispatchStrategy> _dispatchStrategies;

        public InputProcessor(ILogger<InputProcessor> logger, IEnumerable<IDispatchStrategy> dispatchStrategies, IMetaDataDataAccess metaDataDataAccess)
        {
            _logger = logger;
            _metaDataDataAccess = metaDataDataAccess;
            var strategies = dispatchStrategies as IDispatchStrategy[] ?? dispatchStrategies.ToArray();
            if (null == dispatchStrategies || !strategies.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(dispatchStrategies), "is null or empty");
            }

            _dispatchStrategies = strategies;
        }

        public async Task ProcessInputAsync(
            string nameOfAggregation,
            Stream blobStream,
            ProcessType pt,
            Instant startTime,
            Instant endTime,
            Result result,
            CancellationToken cancellationToken)
        {
            var strategy = FindStrategy(nameOfAggregation);
            if (strategy == null)
            {
                _logger.LogInformation("No strategy found for {nameOfAggregation}", nameOfAggregation);
                return;
            }

            result.State = "Ready to dispatch";
            await _metaDataDataAccess.UpdateResultItemAsync(result);

            await strategy.DispatchAsync(blobStream, pt, startTime, endTime, cancellationToken).ConfigureAwait(false);

            result.State = "Dispatched";
            await _metaDataDataAccess.UpdateResultItemAsync(result);
        }

        private IDispatchStrategy FindStrategy(string nameOfAggregation)
        {
            var foundDispatchStrategy = _dispatchStrategies.SingleOrDefault(s => s.FriendlyNameInstance.Equals(nameOfAggregation, StringComparison.OrdinalIgnoreCase));
            return foundDispatchStrategy;
        }
    }
}
