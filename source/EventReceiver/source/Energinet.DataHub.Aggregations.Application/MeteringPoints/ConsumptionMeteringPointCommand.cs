using Energinet.DataHub.Aggregations.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.MeteringPoints
{
    public record ConsumptionMeteringPointCommand(
#pragma warning disable SA1313
        string MeteringPointId,
        MeteringPointType MeteringPointType,
        string MeteringGridArea,
        SettlementMethod SettlementMethod,
        MeteringMethod MeteringMethod,
        MeterReadingPeriodicity MeterReadingPeriodicity,
        ConnectionState ConnectionState,
        Product Product,
        UnitType QuantityUnit,
        string ParentMeteringPointId,
        Instant EffectiveDate)
            : ICommand
    {
        public Transaction Transaction { get; set; }
    }
#pragma warning restore SA1313
}
