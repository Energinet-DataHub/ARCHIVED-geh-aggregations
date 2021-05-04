using System;
using System.Collections.Generic;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Aggregation.Domain.ResultMessages
{
    public class AggregationResultMessage : IOutboundMessage
    {
        // TODO: This class should only contain properties common to all result messages
        protected AggregationResultMessage()
        {
            Kind = 23;
            MkrActivityRecordStatus = 9;
            Product = 8716867000030;
            QuantityMeasurementUnitName = "KWH";
            Resolution = "PT1H";
            TimeIntervalStart = string.Empty;
            TimeIntervalEnd = string.Empty;
            BalanceResponsiblePartyMarketParticipantMRID = string.Empty;
            BalanceSupplierPartyMarketParticipantMRID = string.Empty;
            MeteringGridAreaDomainMRID = string.Empty;
            MarketEvaluationPointType = string.Empty;
            SettlementMethod = string.Empty;
            Quantities = Array.Empty<double>();
            SenderMarketParticipantMRID = string.Empty;
            ReceiverMarketParticipantMRID = string.Empty;
            ProcessType = string.Empty;
            AggregationType = string.Empty;

            Transaction = new Transaction();
        }

        public string AggregationType { get; set; }

        public int MkrActivityRecordStatus { get;  }

        public string QuantityMeasurementUnitName { get;  }

        public string TimeIntervalStart { get; set; }

        public string TimeIntervalEnd { get; set; }

        public string BalanceResponsiblePartyMarketParticipantMRID { get; set; }

        public string BalanceSupplierPartyMarketParticipantMRID { get; set; }

        public string MeteringGridAreaDomainMRID { get; set; }

        public string SenderMarketParticipantMRID { get; set; }

        public string ReceiverMarketParticipantMRID { get; set; }

        public string ProcessType { get; set; }

        public IEnumerable<double> Quantities { get; set; }

        public string SettlementMethod { get; set; }

        public string MarketEvaluationPointType { get; set; }

        public string Resolution { get; }

        public long Product { get;  }

        public int Kind { get; }

        public Transaction Transaction { get; set; }
    }
}
