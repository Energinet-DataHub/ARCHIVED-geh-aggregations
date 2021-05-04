using System.Text.Json.Serialization;

namespace GreenEnergyHub.Aggregation.Application.Services
{
    public class DistributionItem
    {
        [JsonPropertyName("GRID_AREA_CODE")]
        public string GridAreaCode { get; set; }

        [JsonPropertyName("DELEGATIONS")]
        public string Delegations { get; set; }

        [JsonPropertyName("ORGANISATION_ID")]
        public string OrganisationId { get; set; }

        [JsonPropertyName("RecipientPartyID_mRID")]
        public string RecipientPartyId { get; set; }
    }
}
