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

using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Tests.UnitTest.MasterDataTransform
{
    internal class MeteringPointBuilder
    {
        private Instant _fromDate;
        private Instant _toDate;

        public MeteringPointBuilder()
        {
            _fromDate = Instant.FromUtc(2021, 1, 1, 0, 0, 0);
            _toDate = Instant.FromUtc(2021, 1, 7, 0, 0, 0);
        }

        public MeteringPoint Build()
        {
            return new MeteringPoint()
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
                FromDate = _fromDate,
                ToDate = _toDate,
            };
        }

        public MeteringPointBuilder WithFromToDates(Instant fromUtc, Instant toUtc)
        {
            _fromDate = fromUtc;
            _toDate = toUtc;
            return this;
        }
    }
}
