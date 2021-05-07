﻿// Copyright 2020 Energinet DataHub A/S
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
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain
{
    public class AggregatedMeteredDataTimeSeries : IOutboundMessage
    {
        public AggregatedMeteredDataTimeSeries(string aggregationType)
        {
            Kind = 23;
            MkrActivityRecordStatus = 9;
            Product = 8716867000030;
            QuantityMeasurementUnitName = "KWH";
            Resolution = "PT1H";
            TimeIntervalStart = null;
            TimeIntervalEnd = null;
            BalanceResponsiblePartyMarketParticipantmRID = string.Empty;
            BalanceSupplierPartyMarketParticipantmRID = string.Empty;
            MeteringGridAreaDomainMRid = string.Empty;
            MarketEvaluationPointType = string.Empty;
            SettlementMethod = string.Empty;
            Quantities = Array.Empty<double>();
            SenderMarketParticipantmRID = string.Empty;
            ReceiverMarketParticipantmRID = string.Empty;
            ProcessType = string.Empty;
            AggregationType = aggregationType;

            Transaction = new Transaction();
        }

        public string AggregationType { get; }

        public int MkrActivityRecordStatus { get;  }

        public string QuantityMeasurementUnitName { get;  }

        public string TimeIntervalStart { get; set; }

        public string TimeIntervalEnd { get; set; }

        public string BalanceResponsiblePartyMarketParticipantmRID { get; set; }

        public string BalanceSupplierPartyMarketParticipantmRID { get; set; }

        public string MeteringGridAreaDomainMRid { get; set; }

        public string SenderMarketParticipantmRID { get; set; }

        public string ReceiverMarketParticipantmRID { get; set; }

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
