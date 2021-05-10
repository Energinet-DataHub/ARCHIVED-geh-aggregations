using System.Text.Json.Serialization;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public abstract class BaseDto
    {
        [JsonPropertyName("MeteringGridArea_Domain_mRID")]
        public string MeteringGridAreaDomainmRID { get; set; }

        [JsonPropertyName("BalanceResponsibleParty_MarketParticipant_mRID")]
        public string BalanceResponsiblePartyMarketParticipantmRID { get; set; }

        [JsonPropertyName("EnergySupplier_MarketParticipant_mRID")]
        public string EnergySupplierMarketParticipantmRID { get; set; }

        [JsonPropertyName("aggregated_quality")]
        public string AggregatedQuality { get; set; }
    }
}
