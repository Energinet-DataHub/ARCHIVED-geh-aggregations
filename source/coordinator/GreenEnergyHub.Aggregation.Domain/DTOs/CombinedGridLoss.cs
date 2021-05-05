using System;
using System.Text.Json.Serialization;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class CombinedGridLoss
    {
        [JsonPropertyName("BalanceResponsibleParty_MarketParticipant_mRID")]
        public string BalanceResponsiblePartyMarketParticipantMRID { get; set; }

        [JsonPropertyName("ConnectionState")]
        public string ConnectionState { get; set; }

        [JsonPropertyName("IsGridLoss")]
        public bool IsGridLoss { get; set; }

        [JsonPropertyName("IsSystemCorrection")]
        public bool IsSystemCorrection { get; set; }

        [JsonPropertyName("MarketEvaluationPointType")]
        public string MarketEvaluationPointType { get; set; }

        [JsonPropertyName("MarketEvaluationPoint_mRID")]
        public string MarketEvaluationPointMRID { get; set; }

        [JsonPropertyName("MeterReadingPeriodicity")]
        public string MeterReadingPeriodicity { get; set; }

        [JsonPropertyName("MeteringGridArea_Domain_mRID")]
        public string MeteringGridAreaDomainMRID { get; set; }

        [JsonPropertyName("MeteringMethod")]
        public string MeteringMethod { get; set; }

        [JsonPropertyName("SettlementMethod")]
        public string SettlementMethod { get; set; }

        [JsonPropertyName("ValidFrom")]
        public DateTimeOffset ValidFrom { get; set; }

        [JsonPropertyName("added_system_correction")]
        public double AddedSystemCorrection { get; set; }

        [JsonPropertyName("time_window")]
        public TimeWindow TimeWindow { get; set; }
    }
}
