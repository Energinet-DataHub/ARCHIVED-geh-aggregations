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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints;
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
        private readonly List<MeteringPoint> _meteringPoints;

        public MeteringPointEventTransformTest()
        {
            _meteringPoints = new List<MeteringPoint>();
            _meteringPoints.Add(new MeteringPoint()
            {
                Id = "1",
                MeteringPointType = MeteringPointType.Consumption,
                SettlementMethod = SettlementMethod.Flex,
                GridArea = "ga",
                ConnectionState = ConnectionState.New,
                Resolution = Resolution.Hourly,
                InGridArea = null,
                OutGridArea = null,
                MeteringMethod = MeteringMethod.Calculated,
                ParentMeteringPoint = null,
                Unit = Unit.Kwh,
                Product = "prod",
                FromDate = Instant.FromUtc(2021, 1, 1, 0, 0, 0),
                ToDate = Instant.FromUtc(2021, 1, 7, 0, 0, 0),
            });
            _meteringPoints.Add(new MeteringPoint()
            {
                Id = "1",
                MeteringPointType = MeteringPointType.Consumption,
                SettlementMethod = SettlementMethod.Flex,
                GridArea = "ga",
                ConnectionState = ConnectionState.New,
                Resolution = Resolution.Hourly,
                InGridArea = null,
                OutGridArea = null,
                MeteringMethod = MeteringMethod.Calculated,
                ParentMeteringPoint = null,
                Unit = Unit.Kwh,
                Product = "prod",
                FromDate = Instant.FromUtc(2021, 1, 7, 0, 0, 0),
                ToDate = Instant.FromUtc(2021, 1, 9, 0, 0, 0),
            });
            _meteringPoints.Add(new MeteringPoint()
            {
                Id = "1",
                MeteringPointType = MeteringPointType.Consumption,
                SettlementMethod = SettlementMethod.Flex,
                GridArea = "ga",
                ConnectionState = ConnectionState.New,
                Resolution = Resolution.Hourly,
                InGridArea = null,
                OutGridArea = null,
                MeteringMethod = MeteringMethod.Calculated,
                ParentMeteringPoint = null,
                Unit = Unit.Kwh,
                Product = "prod",
                FromDate = Instant.FromUtc(2021, 1, 9, 0, 0, 0),
                ToDate = Instant.FromUtc(2021, 1, 12, 0, 0, 0),
            });
            _meteringPoints.Add(new MeteringPoint()
            {
                Id = "1",
                MeteringPointType = MeteringPointType.Consumption,
                SettlementMethod = SettlementMethod.Flex,
                GridArea = "ga",
                ConnectionState = ConnectionState.New,
                Resolution = Resolution.Hourly,
                InGridArea = null,
                OutGridArea = null,
                MeteringMethod = MeteringMethod.Calculated,
                ParentMeteringPoint = null,
                Unit = Unit.Kwh,
                Product = "prod",
                FromDate = Instant.FromUtc(2021, 1, 12, 0, 0, 0),
                ToDate = Instant.FromUtc(2021, 1, 17, 0, 0, 0),
            });
            _meteringPoints.Add(new MeteringPoint()
            {
                Id = "1",
                MeteringPointType = MeteringPointType.Consumption,
                SettlementMethod = SettlementMethod.Flex,
                GridArea = "ga",
                ConnectionState = ConnectionState.New,
                Resolution = Resolution.Hourly,
                InGridArea = null,
                OutGridArea = null,
                MeteringMethod = MeteringMethod.Calculated,
                ParentMeteringPoint = null,
                Unit = Unit.Kwh,
                Product = "prod",
                FromDate = Instant.FromUtc(2021, 1, 17, 0, 0, 0),
                ToDate = Instant.MaxValue,
            });
        }

        public void AssertAllAfterSecondPeriodAreConnected(MeteringPoint[] result)
        {
            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.New, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 8/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[5].ConnectionState); // 17/1
        }

        [Fact]
        public void TestChangedPeriodAfterUpdate()
        {
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var result = connectedEvent.GetObjectsAfterMutate(_meteringPoints, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();
            //result_df = period_mutations(consumption_mps_df, settlement_method_updated_df, [Colname.settlement_method]).orderBy(Colname.to_date)
            Assert.Equal(_meteringPoints.Count, result.Count());

            AssertInitialPeriods(result);

            Assert.Equal(Instant.MaxValue, result[4].ToDate);

            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1
        }

        [Fact]
        public void TestAddNewPeriodAfterUpdate()
        {
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 8, 0, 0));

            var result = connectedEvent.GetObjectsAfterMutate(_meteringPoints, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            Assert.Equal(5, _meteringPoints.Count);
            Assert.Equal(6, result.Length);

            AssertNewPeriods(result);

            AssertAllAfterSecondPeriodAreConnected(result);
        }

    [Fact]
        public void TestMultiplePropertiesUpdatedAfterUpdate()
        {
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var settlementMethodChangedEvent =
                new SettlementMethodChanged("1", SettlementMethod.NonProfiled, Instant.FromUtc(2021, 1, 7, 0, 0));

            var result = connectedEvent.GetObjectsAfterMutate(_meteringPoints, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();
            var result2 = settlementMethodChangedEvent.GetObjectsAfterMutate(result.ToList(), connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            Assert.Equal(5, _meteringPoints.Count);
            Assert.Equal(5, result2.Length);

            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1

            Assert.Equal(SettlementMethod.Flex, result[0].SettlementMethod); // 1/1
            Assert.Equal(SettlementMethod.NonProfiled, result[1].SettlementMethod); // 7/1
            Assert.Equal(SettlementMethod.NonProfiled, result[2].SettlementMethod); // 9/1
            Assert.Equal(SettlementMethod.NonProfiled, result[3].SettlementMethod); // 12/1
            Assert.Equal(SettlementMethod.NonProfiled, result[4].SettlementMethod); // 17/1
        }

        [Fact]
        public void TestIdempotency()
        {
            var connectedEvent =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var connectedEvent2 =
                new MeteringPointConnectedEvent("1", ConnectionState.Connected, Instant.FromUtc(2021, 1, 7, 0, 0));

            var result = connectedEvent.GetObjectsAfterMutate(_meteringPoints, connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            Assert.Equal(5, _meteringPoints.Count);
            Assert.Equal(5, result.Length);

            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1

            AssertInitialPeriods(result);

            var result2 = connectedEvent2.GetObjectsAfterMutate(result.ToList(), connectedEvent.EffectiveDate).OrderBy(o => o.ToDate).ToArray();

            Assert.Equal(5, _meteringPoints.Count);
            Assert.Equal(5, result2.Length);

            Assert.Equal(ConnectionState.New, result[0].ConnectionState); // 1/1
            Assert.Equal(ConnectionState.Connected, result[1].ConnectionState); // 7/1
            Assert.Equal(ConnectionState.Connected, result[2].ConnectionState); // 9/1
            Assert.Equal(ConnectionState.Connected, result[3].ConnectionState); // 12/1
            Assert.Equal(ConnectionState.Connected, result[4].ConnectionState); // 17/1

            AssertInitialPeriods(result2);
        }

        private static void AssertInitialPeriods(MeteringPoint[] result)
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

        private static void AssertNewPeriods(MeteringPoint[] result)
        {
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
        }
    }
}
