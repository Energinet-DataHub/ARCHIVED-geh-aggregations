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

using System.Text.Json.Serialization;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Core.Messaging.MessageTypes.Common;
using Energinet.DataHub.Core.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints
{
    public record ConsumptionMeteringPointCreatedEvent(
#pragma warning disable SA1313
            [property: JsonPropertyName("metering_point_id")] string MeteringPointId,
            [property: JsonPropertyName("metering_point_type")] MeteringPointType MeteringPointType,
            [property: JsonPropertyName("gsrn_number")] string MeteringGsrnNumber,
            [property: JsonPropertyName("grid_area_code")] string MeteringGridArea,
            [property: JsonPropertyName("settlement_method")] SettlementMethod SettlementMethod,
            [property: JsonPropertyName("metering_method")] MeteringMethod MeteringMethod,
            [property: JsonPropertyName("meter_reading_periodicity")] MeterReadingPeriodicity MeterReadingPeriodicity,
            [property: JsonPropertyName("net_settlement_group")] string NetSettlementGroup,
            [property: JsonPropertyName("product")] Product Product,
            [property: JsonPropertyName("connection_state")] ConnectionState ConnectionState,
            [property: JsonPropertyName("effective_date")] Instant EffectiveDate,
            [property: JsonPropertyName("parent_id")] string ParentID,
            [property: JsonPropertyName("resolution")] string Resolution,
            [property: JsonPropertyName("unit_type")] QuantityUnit QuantityUnit)
            : IInboundMessage
    {
        public Transaction Transaction { get; set; } = new ();
    }
#pragma warning restore SA1313
}
