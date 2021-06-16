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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Infrastructure;
using Microsoft.Extensions.Logging;
using NodaTime;
using NSubstitute;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class InputProcessorTests
    {
        private const string StrategyName = "TEST";
        private readonly InputProcessor _inputProcessor;
        private readonly IDispatchStrategy _strategy;

        public InputProcessorTests()
        {
            var list = new List<IDispatchStrategy>();
            _strategy = Substitute.For<IDispatchStrategy>();
            _strategy.FriendlyNameInstance.Returns(StrategyName);
            list.Add(_strategy);
            _inputProcessor = new InputProcessor(Substitute.For<ILogger<InputProcessor>>(), list, Substitute.For<IMetaDataDataAccess>());
        }

        [Fact]
        public async Task Check_strategy_no_match()
        {
            // Arrange
            var now = NodaTime.SystemClock.Instance.GetCurrentInstant();

            // Act
            await _inputProcessor.ProcessInputAsync(
                  "Unknown strategy",
                  null,
                  string.Empty,
                  now,
                  now.Plus(Duration.FromHours(1)),
                  new Result("jobid1", "name", "path"),
                  CancellationToken.None).ConfigureAwait(false);

            // Assert
            await _strategy.DidNotReceiveWithAnyArgs().DispatchAsync(
                null,
                string.Empty,
                Instant.MaxValue,
                Instant.MaxValue,
                string.Empty,
                CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task Check_strategy_match()
        {
            // Arrange
            var now = NodaTime.SystemClock.Instance.GetCurrentInstant();

            // Act
            await _inputProcessor.ProcessInputAsync(
                StrategyName,
                null,
                string.Empty,
                now,
                now.Plus(Duration.FromHours(1)),
                new Result("jobid1", "name", "path"),
                CancellationToken.None).ConfigureAwait(false);

            // Assert
            await _strategy.ReceivedWithAnyArgs().DispatchAsync(
                null,
                string.Empty,
                Instant.MaxValue,
                Instant.MaxValue,
                string.Empty,
                CancellationToken.None).ConfigureAwait(false);
        }
    }
}
