using System;
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
using Energinet.DataHub.Aggregations.Application.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Messaging.Protobuf;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Infrastructure.Mappers
{
    public class ConsumptionMeteringPointInboundMapper : ProtobufInboundMapper<ConsumptionMeteringPointCreated>
    {
        protected override IInboundMessage Convert(ConsumptionMeteringPointCreated obj)
        {
            if (obj == null) throw new ArgumentNullException(nameof(obj));

            return new ConsumptionMeteringPointCommand(
                obj.MeteringPointId,
                MeteringPointType.Consumption,
                obj.GridAreaCode,
                ParseSettlementMethod(obj.SettlementMethod),
                ParseMeteringMethod(obj.MeteringMethod),
                ParseMeterReadingPeriodicity(obj.MeterReadingPeriodicity),
                ParseConnectionState(obj.ConnectionState),
                ParseProduct(obj.Product),
                ParseUnitType(obj.UnitType),
                "1234",
                ParseEffectiveDate(obj.EffectiveDate));
        }

        private static Instant ParseEffectiveDate(Timestamp effectiveDate)
        {
            var time = Instant.FromUnixTimeSeconds(effectiveDate.Seconds);
            return time.PlusNanoseconds(effectiveDate.Nanos);
        }

        private static UnitType ParseUnitType(ConsumptionMeteringPointCreated.Types.UnitType unitType)
        {
            return unitType switch
            {
                ConsumptionMeteringPointCreated.Types.UnitType.UtWh => UnitType.Wh,
                ConsumptionMeteringPointCreated.Types.UnitType.UtKwh => UnitType.Kwh,
                ConsumptionMeteringPointCreated.Types.UnitType.UtMwh => UnitType.Mwh,
                ConsumptionMeteringPointCreated.Types.UnitType.UtGwh => UnitType.Gwh,
                _ => throw new ArgumentException("Could not pass argument", nameof(unitType))
            };
        }

        private static Product ParseProduct(ConsumptionMeteringPointCreated.Types.ProductType product)
        {
            return product switch
            {
                ConsumptionMeteringPointCreated.Types.ProductType.PtTariff => Product.Tariff,
                ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive => Product.EnergyActive,
                ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyreactive => Product.EnergyReactive,
                ConsumptionMeteringPointCreated.Types.ProductType.PtFuelquantity => Product.FuelQuantity,
                ConsumptionMeteringPointCreated.Types.ProductType.PtPoweractive => Product.PowerActive,
                ConsumptionMeteringPointCreated.Types.ProductType.PtPowerreactive => Product.PowerReactive,
                _ => throw new ArgumentException("Could not pass argument", nameof(product))
            };
        }

        private static ConnectionState ParseConnectionState(ConsumptionMeteringPointCreated.Types.ConnectionState connectionState)
        {
            return connectionState switch
            {
                ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew => ConnectionState.New,
                _ => throw new ArgumentException("Could not pass argument", nameof(connectionState))
            };
        }

        private static MeterReadingPeriodicity ParseMeterReadingPeriodicity(ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            return meterReadingPeriodicity switch
            {
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly => MeterReadingPeriodicity.Hourly,
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly => MeterReadingPeriodicity.Quarterly,
                _ => throw new ArgumentException("Could not pass argument", nameof(meterReadingPeriodicity))
            };
        }

        private static MeteringMethod ParseMeteringMethod(ConsumptionMeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            return meteringMethod switch
            {
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated => MeteringMethod.Calculated,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical => MeteringMethod.Physical,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual => MeteringMethod.Virtual,
                _ => throw new ArgumentException("Could not pass argument", nameof(meteringMethod))
            };
        }

        private static SettlementMethod ParseSettlementMethod(ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            return settlementMethod switch
            {
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex => SettlementMethod.Flex,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                _ => throw new ArgumentException("Could not pass argument", nameof(settlementMethod))
            };
        }
    }
}
