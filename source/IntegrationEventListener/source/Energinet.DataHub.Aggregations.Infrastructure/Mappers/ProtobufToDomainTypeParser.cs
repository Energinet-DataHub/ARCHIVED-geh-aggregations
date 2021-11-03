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
        public static QuantityUnit ParseUnitType(ConsumptionMeteringPointCreated.Types.UnitType unitType)
        {
            return unitType switch
            {
                ConsumptionMeteringPointCreated.Types.UnitType.UtWh => QuantityUnit.Wh,
                ConsumptionMeteringPointCreated.Types.UnitType.UtKwh => QuantityUnit.Kwh,
                ConsumptionMeteringPointCreated.Types.UnitType.UtMwh => QuantityUnit.Mwh,
                ConsumptionMeteringPointCreated.Types.UnitType.UtGwh => QuantityUnit.Gwh,
                _ => throw new ArgumentOutOfRangeException(nameof(unitType), unitType, "Could not parse argument")
            };
        }

        public static Product ParseProduct(ConsumptionMeteringPointCreated.Types.ProductType product)
        {
            return product switch
            {
                ConsumptionMeteringPointCreated.Types.ProductType.PtTariff => Product.Tariff,
                ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive => Product.EnergyActive,
                ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyreactive => Product.EnergyReactive,
                ConsumptionMeteringPointCreated.Types.ProductType.PtFuelquantity => Product.FuelQuantity,
                ConsumptionMeteringPointCreated.Types.ProductType.PtPoweractive => Product.PowerActive,
                ConsumptionMeteringPointCreated.Types.ProductType.PtPowerreactive => Product.PowerReactive,
                _ => throw new ArgumentOutOfRangeException(nameof(product), product, "Could not parse argument")
            };
        }

        public static ConnectionState ParseConnectionState(ConsumptionMeteringPointCreated.Types.ConnectionState connectionState)
        {
            return connectionState switch
            {
                ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew => ConnectionState.New,
                _ => throw new ArgumentOutOfRangeException(nameof(connectionState), connectionState, "Could not parse argument")
            };
        }

        public static MeterReadingPeriodicity ParseMeterReadingPeriodicity(ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            return meterReadingPeriodicity switch
            {
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly => MeterReadingPeriodicity.Hourly,
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly => MeterReadingPeriodicity.Quarterly,
                _ => throw new ArgumentOutOfRangeException(nameof(meterReadingPeriodicity), meterReadingPeriodicity, "Could not parse argument")
            };
        }

        public static MeteringMethod ParseMeteringMethod(ConsumptionMeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            return meteringMethod switch
            {
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated => MeteringMethod.Calculated,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical => MeteringMethod.Physical,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual => MeteringMethod.Virtual,
                _ => throw new ArgumentOutOfRangeException(nameof(meteringMethod), meteringMethod, "Could not parse argument")
            };
        }

        public static SettlementMethod ParseSettlementMethod(ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            return settlementMethod switch
            {
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex => SettlementMethod.Flex,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                _ => throw new ArgumentOutOfRangeException(nameof(settlementMethod), settlementMethod, "Could not parse argument")
            };
        }

        public static Instant ParseEffectiveDate(Timestamp effectiveDate)
        {
            if (effectiveDate == null) throw new ArgumentNullException(nameof(effectiveDate));
            var time = Instant.FromUnixTimeSeconds(effectiveDate.Seconds);
            return time.PlusNanoseconds(effectiveDate.Nanos);
        }

        public static NetSettlementGroup ParseNetSettlementGroup(ConsumptionMeteringPointCreated.Types.NetSettlementGroup netSettlementGroup)
        {
            return netSettlementGroup switch
            {
                ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgZero => NetSettlementGroup.Zero,
                ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgOne => NetSettlementGroup.One,
                ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgTwo => NetSettlementGroup.Two,
                ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgThree => NetSettlementGroup.Three,
                ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgSix => NetSettlementGroup.Six,
                ConsumptionMeteringPointCreated.Types.NetSettlementGroup.NsgNinetynine => NetSettlementGroup.NinetyNine,
                _ => throw new ArgumentOutOfRangeException(nameof(netSettlementGroup), netSettlementGroup, "Could not parse argument")
            };
        }
    }
}
