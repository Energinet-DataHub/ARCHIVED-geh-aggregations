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
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints
{
    public record ConsumptionMeteringPointCreatedEvent(
#pragma warning disable SA1313
#pragma warning disable CA1801
            [property: JsonPropertyName("metering_point_id")] string MeteringPointId,
            [property: JsonPropertyName("metering_point_type")] MeteringPointType MeteringPointType,
            [property: JsonPropertyName("grid_area")] string GridArea,
            [property: JsonPropertyName("settlement_method")] SettlementMethod SettlementMethod,
            [property: JsonPropertyName("metering_method")] MeteringMethod MeteringMethod,
            [property: JsonPropertyName("resolution")] Resolution Resolution,
            [property: JsonPropertyName("product")] Product Product,
            [property: JsonPropertyName("connection_state")] ConnectionState ConnectionState,
            [property: JsonPropertyName("unit")] Unit Unit,
            [property: JsonPropertyName("effective_date")] Instant EffectiveDate)
            : IInboundMessage
    {
        public Transaction Transaction { get; set; } = new ();
    }
#pragma warning restore SA1313
#pragma warning restore CA1801
}
