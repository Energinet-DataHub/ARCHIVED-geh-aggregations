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
    public static class ProtobufToDomainTypeMapper
    {
        public static Unit MapUnitType(MeteringPointCreated.Types.UnitType unitType)
        {
            return unitType switch
            {
                MeteringPointCreated.Types.UnitType.UtKwh => Unit.Kwh,
                MeteringPointCreated.Types.UnitType.UtGwh => Unit.Gwh,
                MeteringPointCreated.Types.UnitType.UtMwh => Unit.Mwh,
                MeteringPointCreated.Types.UnitType.UtWh => Unit.Wh,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, "Could not map argument"),
            };
        }

        public static Product MapProduct(MeteringPointCreated.Types.ProductType product)
        {
            return product switch
            {
                MeteringPointCreated.Types.ProductType.PtEnergyactive => Product.EnergyActive,
                MeteringPointCreated.Types.ProductType.PtEnergyreactive => Product.EnergyReactive,
                MeteringPointCreated.Types.ProductType.PtFuelquantity => Product.FuelQuantity,
                MeteringPointCreated.Types.ProductType.PtPoweractive => Product.PowerActive,
                MeteringPointCreated.Types.ProductType.PtPowerreactive => Product.PowerReactive,
                MeteringPointCreated.Types.ProductType.PtTariff => Product.Tariff,
                _ => throw new ArgumentOutOfRangeException(nameof(product), product, "Could not map argument"),
            };
        }

        public static ConnectionState MapConnectionState(MeteringPointCreated.Types.ConnectionState connectionState)
        {
            return connectionState switch
            {
                MeteringPointCreated.Types.ConnectionState.CsNew =>
                    ConnectionState.New,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, "Could not map argument"),
            };
        }

        public static Resolution MapMeterReadingPeriodicity(MeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            return meterReadingPeriodicity switch
            {
                MeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly => Resolution.Hourly,
                MeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly => Resolution.Quarterly,
                _ => throw new ArgumentOutOfRangeException(nameof(meterReadingPeriodicity), meterReadingPeriodicity, "Could not map argument"),
            };
        }

        public static MeteringMethod MapMeteringMethod(MeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            return meteringMethod switch
            {
                MeteringPointCreated.Types.MeteringMethod.MmCalculated => MeteringMethod.Calculated,
                MeteringPointCreated.Types.MeteringMethod.MmPhysical => MeteringMethod.Physical,
                MeteringPointCreated.Types.MeteringMethod.MmVirtual => MeteringMethod.Virtual,
                _ => throw new ArgumentOutOfRangeException(nameof(meteringMethod), meteringMethod, "Could not map argument"),
            };
        }

        public static SettlementMethod? MapSettlementMethod(MeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            return settlementMethod switch
            {
                MeteringPointCreated.Types.SettlementMethod.SmFlex => SettlementMethod.Flex,
                MeteringPointCreated.Types.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                MeteringPointCreated.Types.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                MeteringPointCreated.Types.SettlementMethod.SmNull => null,
                _ => throw new ArgumentOutOfRangeException(nameof(settlementMethod), settlementMethod, "Could not map argument"),
            };
        }

        public static Instant MapEffectiveDate(Timestamp effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            var time = Instant.FromUnixTimeSeconds(effectiveDate.Seconds);
            return time.PlusNanoseconds(effectiveDate.Nanos);
        }
    }
}
