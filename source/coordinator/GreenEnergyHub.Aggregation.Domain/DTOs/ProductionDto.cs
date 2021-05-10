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

using System.Text.Json.Serialization;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    public class ProductionDto
    {
        [JsonPropertyName("MeteringGridArea_Domain_mRID")]
        public string MeteringGridAreaDomainmRID { get; set; }

        [JsonPropertyName("BalanceResponsibleParty_MarketParticipant_mRID")]
        public string BalanceResponsiblePartyMarketParticipantmRID { get; set; }

        [JsonPropertyName("EnergySupplier_MarketParticipant_mRID")]
        public string EnergySupplierMarketParticipantmRID { get; set; }

        [JsonPropertyName("time_start")]
        public string TimeStart { get; set; }

        [JsonPropertyName("time_end")]
        public string TimeEnd { get; set; }

        [JsonPropertyName("sum_quantity")]
        public double SumQuantity { get; set; }
    }
}
