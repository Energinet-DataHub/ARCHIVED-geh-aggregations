using Energinet.DataHub.Aggregations.Domain;
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
            : ICommand;
#pragma warning restore SA1313
}
