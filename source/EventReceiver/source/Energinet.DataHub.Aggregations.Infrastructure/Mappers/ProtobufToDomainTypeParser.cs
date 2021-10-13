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
                _ => throw new ArgumentException("Could not pass argument", nameof(unitType))
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
                _ => throw new ArgumentException("Could not pass argument", nameof(product))
            };
        }

        public static ConnectionState ParseConnectionState(ConsumptionMeteringPointCreated.Types.ConnectionState connectionState)
        {
            return connectionState switch
            {
                ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew => ConnectionState.New,
                _ => throw new ArgumentException("Could not pass argument", nameof(connectionState))
            };
        }

        public static MeterReadingPeriodicity ParseMeterReadingPeriodicity(ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity meterReadingPeriodicity)
        {
            return meterReadingPeriodicity switch
            {
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly => MeterReadingPeriodicity.Hourly,
                ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpQuarterly => MeterReadingPeriodicity.Quarterly,
                _ => throw new ArgumentException("Could not pass argument", nameof(meterReadingPeriodicity))
            };
        }

        public static MeteringMethod ParseMeteringMethod(ConsumptionMeteringPointCreated.Types.MeteringMethod meteringMethod)
        {
            return meteringMethod switch
            {
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmCalculated => MeteringMethod.Calculated,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical => MeteringMethod.Physical,
                ConsumptionMeteringPointCreated.Types.MeteringMethod.MmVirtual => MeteringMethod.Virtual,
                _ => throw new ArgumentException("Could not pass argument", nameof(meteringMethod))
            };
        }

        public static SettlementMethod ParseSettlementMethod(ConsumptionMeteringPointCreated.Types.SettlementMethod settlementMethod)
        {
            return settlementMethod switch
            {
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex => SettlementMethod.Flex,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmProfiled => SettlementMethod.Profiled,
                ConsumptionMeteringPointCreated.Types.SettlementMethod.SmNonprofiled => SettlementMethod.NonProfiled,
                _ => throw new ArgumentException("Could not pass argument", nameof(settlementMethod))
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
