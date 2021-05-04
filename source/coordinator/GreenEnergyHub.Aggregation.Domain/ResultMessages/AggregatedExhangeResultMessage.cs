namespace GreenEnergyHub.Aggregation.Domain.ResultMessages
{
    public class AggregatedExhangeResultMessage : AggregationResultMessage
    {
        public double Result { get; set; }

        // TODO: Move rest of exchange-only properties here
    }
}
