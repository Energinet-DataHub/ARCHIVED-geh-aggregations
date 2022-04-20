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
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Core.Messaging.Protobuf;
using Energinet.DataHub.Core.Messaging.Transport;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;

namespace Energinet.DataHub.Aggregations.Infrastructure.Mappers
{
    public class MeteringPointCreatedMapper : ProtobufInboundMapper<MeteringPointCreated>
    {
        protected override IInboundMessage Convert(MeteringPointCreated obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            return new MeteringPointCreatedEvent(
                MeteringPointId: obj.GsrnNumber,
                MeteringPointType: MeteringPointType.Consumption,
                GridArea: obj.GridAreaCode,
                SettlementMethod: ProtobufToDomainTypeParser.ParseSettlementMethod(obj.SettlementMethod),
                MeteringMethod: ProtobufToDomainTypeParser.ParseMeteringMethod(obj.MeteringMethod),
                Resolution: ProtobufToDomainTypeParser.ParseMeterReadingPeriodicity(obj.MeterReadingPeriodicity),
                Product: ProtobufToDomainTypeParser.ParseProduct(obj.Product),
                ConnectionState: ProtobufToDomainTypeParser.ParseConnectionState(obj.ConnectionState),
                Unit: ProtobufToDomainTypeParser.ParseUnitType(obj.UnitType),
                EffectiveDate: ProtobufToDomainTypeParser.ParseEffectiveDate(obj.EffectiveDate));
        }
    }
}
