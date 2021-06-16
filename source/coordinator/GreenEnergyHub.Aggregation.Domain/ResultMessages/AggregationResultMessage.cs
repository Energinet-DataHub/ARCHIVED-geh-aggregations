// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.ResultMessages
{
    public class AggregationResultMessage : IOutboundMessage
    {
        public AggregationResultMessage(
            string processType,
            string processRole,
            Instant timeIntervalStart,
            Instant timeIntervalEnd,
            string meteringGridAreaDomainmRID,
            string balanceResponsiblePartyMarketParticipantmRID,
            string balanceSupplierPartyMarketParticipantmRID,
            string marketEvaluationPointType,
            IEnumerable<EnergyObservation> energyObservation,
            string senderMarketParticipantmRID,
            string receiverMarketParticipantmRID)
        {
            ProcessType = processType;
            ProcessRole = processRole;
            TimeIntervalStart = timeIntervalStart;
            TimeIntervalEnd = timeIntervalEnd;
            MeteringGridAreaDomainmRID = meteringGridAreaDomainmRID;
            BalanceResponsiblePartyMarketParticipantmRID = balanceResponsiblePartyMarketParticipantmRID;
            BalanceSupplierPartyMarketParticipantmRID = balanceSupplierPartyMarketParticipantmRID;
            MarketEvaluationPointType = marketEvaluationPointType;
            EnergyObservation = energyObservation;
            SenderMarketParticipantmRID = senderMarketParticipantmRID;
            ReceiverMarketParticipantmRID = receiverMarketParticipantmRID;
        }

        protected AggregationResultMessage(AggregationResultMessage other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            ProcessType = other.ProcessType;
            ProcessRole = other.ProcessRole;
            TimeIntervalStart = other.TimeIntervalStart;
            TimeIntervalEnd = other.TimeIntervalEnd;
            MeteringGridAreaDomainmRID = other.MeteringGridAreaDomainmRID;
            BalanceResponsiblePartyMarketParticipantmRID = other.BalanceResponsiblePartyMarketParticipantmRID;
            BalanceSupplierPartyMarketParticipantmRID = other.BalanceSupplierPartyMarketParticipantmRID;
            MarketEvaluationPointType = other.MarketEvaluationPointType;
            EnergyObservation = other.EnergyObservation;
            SenderMarketParticipantmRID = other.SenderMarketParticipantmRID;
            ReceiverMarketParticipantmRID = other.ReceiverMarketParticipantmRID;
        }

        public string ProcessType { get; set; }

        public string ProcessRole { get; set; }

        public Instant? TimeIntervalStart { get; set; }

        public Instant? TimeIntervalEnd { get; set; }

        public string MeteringGridAreaDomainmRID { get; set; }

        public string BalanceResponsiblePartyMarketParticipantmRID { get; set; } = string.Empty;

        public string BalanceSupplierPartyMarketParticipantmRID { get; set; } = string.Empty;

        public string MarketEvaluationPointType { get; set; }

        public IEnumerable<EnergyObservation> EnergyObservation { get; set; }

        public string SenderMarketParticipantmRID { get; set; }

        public string ReceiverMarketParticipantmRID { get; set; }

        public string DocumentType { get; } = "E31";

        public int Kind { get; } = 23;

        public int Status { get; } = 9;

        public string QuantityMeasurementUnitName { get; } = "KWH";

        public string Resolution { get; } = "PT1H";

        public long Product { get; } = 8716867000030;

        public Transaction Transaction { get; set; }
    }
}
