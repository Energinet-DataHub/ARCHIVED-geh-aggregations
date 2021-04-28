using System;
using Newtonsoft.Json;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class CombinedGridLoss
    {
        [JsonProperty("BalanceResponsibleParty_MarketParticipant_mRID")]
        public string BalanceResponsiblePartyMarketParticipantMRid { get; set; }

        [JsonProperty("ConnectionState")]
        public string ConnectionState { get; set; }

        [JsonProperty("IsGridLoss")]
        public bool IsGridLoss { get; set; }

        [JsonProperty("IsSystemCorrection")]
        public bool IsSystemCorrection { get; set; }

        [JsonProperty("MarketEvaluationPointType")]
        public string MarketEvaluationPointType { get; set; }

        [JsonProperty("MarketEvaluationPoint_mRID")]
        public string MarketEvaluationPointMRid { get; set; }

        [JsonProperty("MeterReadingPeriodicity")]
        public string MeterReadingPeriodicity { get; set; }

        [JsonProperty("MeteringGridArea_Domain_mRID")]
        public string MeteringGridAreaDomainMRid { get; set; }

        [JsonProperty("MeteringMethod")]
        public string MeteringMethod { get; set; }

        [JsonProperty("SettlementMethod")]
        public string SettlementMethod { get; set; }

        [JsonProperty("ValidFrom")]
        public DateTimeOffset ValidFrom { get; set; }

        [JsonProperty("added_system_correction")]
        public double AddedSystemCorrection { get; set; }

        [JsonProperty("time_window")]
        public TimeWindow TimeWindow { get; set; }
    }
}
