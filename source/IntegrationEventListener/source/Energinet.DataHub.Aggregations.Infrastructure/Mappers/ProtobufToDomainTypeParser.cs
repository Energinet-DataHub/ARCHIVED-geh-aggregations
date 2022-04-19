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
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Infrastructure.Mappers
{
    public static class ProtobufToDomainTypeParser
    {
        public static Unit ParseUnitType(MeteringPointCreated.Types.UnitType unitType)
        {
            return unitType switch
            {
                MeteringPointCreated.Types.UnitType.UtKwh => Unit.Kwh,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, "Could not parse argument")
            };
        }

        public static Product ParseProduct(MeteringPointCreated.Types.ProductType product)
        {
            return product switch
            {
                MeteringPointCreated.Types.ProductType.PtEnergyactive => Product.EnergyActive,
                _ => throw new ArgumentOutOfRangeException(nameof(product), product, "Could not parse argument")
            };
        }

        public static ConnectionState ParseConnectionState(MeteringPointCreated.Types.ConnectionState connectionState)
        {
            return connectionState switch
            {
                MeteringPointCreated.Types.ConnectionState.CsNew => ConnectionState.New,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, "Could not parse argument")
            };
        }

        public static Resolution ParseMeterReadingPeriodicity(MeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            return meterReadingPeriodicity switch
            {
                MeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly => Resolution.Hourly,
                MeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly => Resolution.Quarterly,
                _ => throw new ArgumentOutOfRangeException(nameof(meterReadingPeriodicity), meterReadingPeriodicity, "Could not parse argument")
            };
        }

        public static MeteringMethod ParseMeteringMethod(MeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            return meteringMethod switch
            {
                MeteringPointCreated.Types.MeteringMethod.MmCalculated => MeteringMethod.Calculated,
                MeteringPointCreated.Types.MeteringMethod.MmPhysical => MeteringMethod.Physical,
                MeteringPointCreated.Types.MeteringMethod.MmVirtual => MeteringMethod.Virtual,
                _ => throw new ArgumentOutOfRangeException(nameof(meteringMethod), meteringMethod, "Could not parse argument")
            };
        }

        public static SettlementMethod ParseSettlementMethod(MeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            return settlementMethod switch
            {
                MeteringPointCreated.Types.SettlementMethod.SmFlex => SettlementMethod.Flex,
                MeteringPointCreated.Types.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                MeteringPointCreated.Types.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                _ => throw new ArgumentOutOfRangeException(nameof(settlementMethod), settlementMethod, "Could not parse argument")
            };
        }

        public static Instant ParseEffectiveDate(Timestamp effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            var time = Instant.FromUnixTimeSeconds(effectiveDate.Seconds);
            return time.PlusNanoseconds(effectiveDate.Nanos);
        }
    }
}
