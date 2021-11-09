using System.Text.Json.Serialization;

namespace AggregationResultReceiver.Domain
{
#pragma warning disable SA1313
    public record ResultData(
        [property:JsonPropertyName("job_id")]string JobId,
        [property:JsonPropertyName("snapshot_id")]string SnapshotId,
        [property:JsonPropertyName("result_id")]string ResultId,
        [property:JsonPropertyName("result_name")]string ResultName,
        [property:JsonPropertyName("grid_area")]string GridArea,
        [property:JsonPropertyName("in_grid_area")]string InGridArea,
        [property:JsonPropertyName("out_grid_area")]string OutGridArea,
        [property:JsonPropertyName("balance_responsible_id")]string BalanceResponsibleId,
        [property:JsonPropertyName("energy_supplier_id")]string EnergySupplierId,
        [property:JsonPropertyName("start_datetime")]string StartDateTime,
        [property:JsonPropertyName("end_datetime")]string EndDateTime,
        [property:JsonPropertyName("resolution")]string Resolution,
        [property:JsonPropertyName("sum_quantity")]decimal SumQuantity,
        [property:JsonPropertyName("quality")]string Quality,
        [property:JsonPropertyName("metering_point_type")]string MeteringPointType,
        [property:JsonPropertyName("settlement_method")]string SettlementMethod);
#pragma warning restore SA1313
}
