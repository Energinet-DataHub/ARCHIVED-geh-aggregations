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
        public AggregationResultMessage()
        {
            Kind = 23;
            Status = 9;
            Product = 8716867000030;
            QuantityMeasurementUnitName = "KWH";
            Resolution = "PT1H";
            TimeIntervalStart = null;
            TimeIntervalEnd = null;
            MeteringGridAreaDomainmRID = string.Empty;
            MarketEvaluationPointType = string.Empty;
            EnergyQuantity = 0.0;
            QuantityQuality = string.Empty;
            SenderMarketParticipantmRID = string.Empty;
            ReceiverMarketParticipantmRID = string.Empty;
            ProcessType = string.Empty;

            Transaction = new Transaction();
        }

        public string ProcessType { get; set; }

        public Instant? TimeIntervalStart { get; set; }

        public Instant? TimeIntervalEnd { get; set; }

        public string MeteringGridAreaDomainmRID { get; set; }

        public string BalanceResponsiblePartyMarketParticipantmRID { get; set; }

        public string BalanceSupplierPartyMarketParticipantmRID { get; set; }

        public string MarketEvaluationPointType { get; set; }

        public double EnergyQuantity { get; set; }

        public string QuantityQuality { get; set; }

        public string SenderMarketParticipantmRID { get; set; }

        public string ReceiverMarketParticipantmRID { get; set; }

        public int Kind { get; }

        public int Status { get;  }

        public string QuantityMeasurementUnitName { get;  }

        public string Resolution { get; }

        public long Product { get;  }

        public Transaction Transaction { get; set; }
    }
}
