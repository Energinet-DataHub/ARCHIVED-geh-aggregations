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
using Azure.Messaging.ServiceBus;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace Energinet.DataHub.Aggregations.IntegrationEventListener.IntegrationTests.Assets
{
    public static class TestMessages
    {
        private const string MeteringPointId = "1";

        public static ServiceBusMessage CreateMpCreatedMessage(string meteringPointId, DateTime effectiveDate)
        {
            var timestamp = Timestamp.FromDateTime(DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc));
            var message = new MeteringPointCreated
            {
                Product = MeteringPointCreated.Types.ProductType.PtEnergyactive,
                ConnectionState = MeteringPointCreated.Types.ConnectionState.CsNew,
                GsrnNumber = meteringPointId,
                MeteringMethod = MeteringPointCreated.Types.MeteringMethod.MmPhysical,
                SettlementMethod = MeteringPointCreated.Types.SettlementMethod.SmFlex,
                UnitType = MeteringPointCreated.Types.UnitType.UtKwh,
                GridAreaCode = "500",
                MeteringPointId = MeteringPointId,
                MeterReadingPeriodicity = MeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly,
                NetSettlementGroup = MeteringPointCreated.Types.NetSettlementGroup.NsgOne,
                EffectiveDate = timestamp,
            };

            return ServiceBusMessageCreator.CreateSbMessage(
                effectiveDate,
                message.ToByteArray(),
                "1bf1b76337f14b78badc248a3289d021",
                "2542ed0d242e46b68b8b803e93ffbf7b",
                "MeteringPointCreated");
        }

        public static ServiceBusMessage CreateMpConnectedMessage(string meteringPointId, DateTime effectiveDate)
        {
            var timestamp = Timestamp.FromDateTime(DateTime.SpecifyKind(effectiveDate, DateTimeKind.Utc));
            var message = new MeteringPointConnected()
            {
                GsrnNumber = meteringPointId,
                MeteringPointId = MeteringPointId,
                EffectiveDate = timestamp,
            };
            return ServiceBusMessageCreator.CreateSbMessage(
                effectiveDate,
                message.ToByteArray(),
                "1bf1b76337f14b78badc248a3289d022",
                "2542ed0d242e46b68b8b803e93ffbf7c",
                "MeteringPointConnected");
        }
    }
}
