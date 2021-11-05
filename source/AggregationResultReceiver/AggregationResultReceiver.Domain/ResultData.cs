namespace AggregationResultReceiver.Domain
{
#pragma warning disable SA1313
    public record ResultData(
        string JobId,
        string SnapshotId,
        string ResultId,
        string ResultName,
        string GridArea,
        string InGridArea,
        string OutGridArea,
        string BalanceResponsibleId,
        string EnergySupplierId,
        string StartDateTime,
        string EndDateTime,
        string Resolution,
        decimal SumQuantity,
        string MeteringPointType,
        string SettlementMethod);
#pragma warning restore SA1313
}
