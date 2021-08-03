using GreenEnergyHub.Aggregation.Domain.MeteringPointMessage;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MarketDocumentDto
    {
        public string MRID { get; set; }

        public string Type { get; set; }

        public Instant CreatedDateTime { get; set; }

        public SenderMarketParticipantDto SenderMarketParticipant { get; set; }

        public RecipientMarketParticipantDto RecipientMarketParticipant { get; set; }

        public string ProcessType { get; set; }

        public string MarketServiceCategoryKind { get; set; }
    }
}
