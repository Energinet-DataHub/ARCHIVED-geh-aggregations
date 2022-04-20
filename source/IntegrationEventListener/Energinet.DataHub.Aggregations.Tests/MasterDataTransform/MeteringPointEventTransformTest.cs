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
using System.Linq;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.Mutators;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.MasterDataTransform
{
    [UnitTest]
    public class MeteringPointEventTransformTest
    {
        private readonly List<MeteringPoint> _meteringPointPeriods;

        /// <summary>
        /// Create a metering point with a set of periods.
        /// We do this by creating 5 metering points with the same properties but different to and from dates.
        /// </summary>
        public MeteringPointEventTransformTest()
        {
            _meteringPointPeriods = new List<MeteringPoint>();
            var mpb = new MeteringPointBuilder();
            _meteringPointPeriods.Add(mpb.Build());
            _meteringPointPeriods.Add(mpb
                .WithFromToDates(
                    Instant.FromUtc(2021, 1, 7, 0, 0, 0),
                    Instant.FromUtc(2021, 1, 9, 0, 0, 0))
                .Build());
            _meteringPointPeriods.Add(mpb
                .WithFromToDates(
                    Instant.FromUtc(2021, 1, 9, 0, 0, 0),
                    Instant.FromUtc(2021, 1, 12, 0, 0, 0))
                .Build());
            _meteringPointPeriods.Add(mpb
                .WithFromToDates(
                    Instant.FromUtc(2021, 1, 12, 0, 0, 0),
                    Instant.FromUtc(2021, 1, 17, 0, 0, 0))
                .Build());
            _meteringPointPeriods.Add(mpb
                .WithFromToDates(
                    Instant.FromUtc(2021, 1, 17, 0, 0, 0),
                    Instant.MaxValue)
                .Build());
        }

        /// <summary>
        /// Test that an incoming event creates a new period
        /// </summary>
        [Fact]
        public void TestChangedPeriodAfterUpdate()
        {
            //from day 7 we are connected
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var mutator = new MeteringPointConnectedMutator(connectedEvent);
            //apply the event to the set of current metering point periods
            var result = mutator.GetObjectsAfterMutate(_meteringPointPeriods, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            //the amount of periods should be the same
            Assert.Equal(_meteringPointPeriods.Count, result.Count());

            //the dates of the periods should be the same
            AssertInitialPeriodDates(result);

            //the last period should end in max value
            Assert.Equal(Instant.MaxValue, result[4].ToDate);

            //Check that from day 7 we are connected
            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1
        }

        /// <summary>
        /// Check that if we get an event with a new period that it is added
        /// </summary>
        [Fact]
        public void TestAddNewPeriodAfterUpdate()
        {
            //From day 8 we are connected
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 8, 0, 0));

            var mutator = new MeteringPointConnectedMutator(connectedEvent);
            var result = mutator.GetObjectsAfterMutate(_meteringPointPeriods, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            //Initially we have 5 periods
            Assert.Equal(5, _meteringPointPeriods.Count);
            //With the event we should now have 6 periods
            Assert.Equal(6, result.Length);

            //Check that the dates of the periods are OK
            Assert.Equal(Instant.FromUtc(2021, 1, 1, 0, 0, 0), result[0].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 7, 0, 0, 0), result[0].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 7, 0, 0, 0), result[1].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 8, 0, 0, 0), result[1].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 8, 0, 0, 0), result[2].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 9, 0, 0, 0), result[2].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 9, 0, 0, 0), result[3].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 12, 0, 0, 0), result[3].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 12, 0, 0, 0), result[4].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 17, 0, 0, 0), result[4].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 17, 0, 0, 0), result[5].FromDate);
            Assert.Equal(Instant.MaxValue, result[5].ToDate);

            //Check that we are connected from day 8
            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.New, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 8/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[5].ConnectionState); // 17/1
        }

        //Check that we can manipulate multiple types of properties with different events
        [Fact]
        public void TestMultiplePropertiesUpdatedAfterUpdate()
        {
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var settlementMethodChangedEvent =
                new SettlementMethodChangedEvent("1", SettlementMethod.NonProfiled, Instant.FromUtc(2021, 1, 7, 0, 0));

            var mutator = new MeteringPointConnectedMutator(connectedEvent);
            var result = mutator.GetObjectsAfterMutate(_meteringPointPeriods, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            var mutator2 = new SettlementMethodChangedMutator(settlementMethodChangedEvent);
            var result2 = mutator2.GetObjectsAfterMutate(result.ToList(), connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            Assert.Equal(5, _meteringPointPeriods.Count);
            Assert.Equal(5, result2.Length);

            //Check the connected event are in effect from day 7
            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1

            //Check that SettlementMethod is changed from day 7
            Assert.Equal(SettlementMethod.Flex, result[0].SettlementMethod); // 1/1
            Assert.Equal(SettlementMethod.NonProfiled, result[1].SettlementMethod); // 7/1
            Assert.Equal(SettlementMethod.NonProfiled, result[2].SettlementMethod); // 9/1
            Assert.Equal(SettlementMethod.NonProfiled, result[3].SettlementMethod); // 12/1
            Assert.Equal(SettlementMethod.NonProfiled, result[4].SettlementMethod); // 17/1
        }

        /// <summary>
        /// Check that if we get the same event multiple times that it does not change the expected periods and the properties within
        /// </summary>
        [Fact]
        public void TestIdempotency()
        {
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var connectedEvent2 =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var mutator = new MeteringPointConnectedMutator(connectedEvent);

            var result = mutator.GetObjectsAfterMutate(_meteringPointPeriods, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            CheckStateOfPeriodsInIdempotency(result);

            var mutator2 = new MeteringPointConnectedMutator(connectedEvent2);

            //Manipulate the periods from the first event with the second event
            var result2 = mutator2.GetObjectsAfterMutate(result.ToList(), connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            //Check that the properties are the same
            CheckStateOfPeriodsInIdempotency(result2);
        }

        private static void AssertInitialPeriodDates(MeteringPoint[] result)
        {
            Assert.Equal(Instant.FromUtc(2021, 1, 1, 0, 0, 0), result[0].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 7, 0, 0, 0), result[0].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 7, 0, 0, 0), result[1].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 9, 0, 0, 0), result[1].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 9, 0, 0, 0), result[2].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 12, 0, 0, 0), result[2].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 12, 0, 0, 0), result[3].FromDate);
            Assert.Equal(Instant.FromUtc(2021, 1, 17, 0, 0, 0), result[3].ToDate);

            Assert.Equal(Instant.FromUtc(2021, 1, 17, 0, 0, 0), result[4].FromDate);
        }

        private void CheckStateOfPeriodsInIdempotency(MeteringPoint[] result)
        {
            Assert.Equal(5, _meteringPointPeriods.Count);
            Assert.Equal(5, result.Length);

            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1

            AssertInitialPeriodDates(result);
        }
    }
}
