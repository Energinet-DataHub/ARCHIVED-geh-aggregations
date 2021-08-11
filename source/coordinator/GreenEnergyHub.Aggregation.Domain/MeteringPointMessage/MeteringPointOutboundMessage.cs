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

using GreenEnergyHub.Aggregation.Domain.DTOs;

namespace GreenEnergyHub.Aggregation.Domain.MeteringPointMessage
{
    public class MeteringPointOutboundMessage : IOutboundMessage
    {
        public string Mrid { get; set; } = string.Empty;

        public string MessageReference { get; set; } = string.Empty;

        public MarketDocumentDto MarketDocument { get; set; } = null!;

        public string MktActivityRecordStatus { get; set; } = string.Empty;

        public string Product { get; set; } = string.Empty;

        public string QuantityMeasurementUnitName { get; set; } = string.Empty;

        public string MarketEvaluationPointType { get; set; } = string.Empty;

        public string SettlementMethod { get; set; } = string.Empty;

        public string MarketEvaluationPointMrid { get; set; } = string.Empty;

        public string CorrelationId { get; set; } = string.Empty;

        public PeriodDto Period { get; set; } = null!;

        public Transaction Transaction { get; set; } = null!;
    }
}
