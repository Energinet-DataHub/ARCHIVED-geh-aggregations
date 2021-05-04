namespace GreenEnergyHub.Aggregation.Domain.ResultMessages
{
    public class AggregatedExchangeNeighbourResultMessage : AggregatedExhangeResultMessage
    {
        public string InMeteringGridAreaDomainMRID { get; set; }

        public string OutMeteringGridAreaDomainMRID { get; set; }
    }
}
