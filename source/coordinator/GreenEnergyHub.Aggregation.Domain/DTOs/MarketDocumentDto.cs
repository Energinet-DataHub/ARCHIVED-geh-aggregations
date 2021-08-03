using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Aggregation.Domain.MeteringPointMessage;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class MarketDocumentDto
    {
        public string MRID { get; set; }

        public string Type { get; set; }

        public Timestamp CreatedDateTime { get; set; }

        public SenderMarketParticipantDto SenderMarketParticipant { get; set; }

        public RecipientMarketParticipantDto RecipientMarketParticipant { get; set; }

        public string ProcessType { get; set; }

        public string MarketServiceCategoryKind { get; set; }
    }
}
