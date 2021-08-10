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
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Aggregation.Domain.MeteringPointMessage;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Messaging.Protobuf;

namespace GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf
{
    public class MeteringPointMessageToDtoMapper : ProtobufOutboundMapper<MeteringPointOutboundMessage>
    {
        protected override IMessage Convert(MeteringPointOutboundMessage obj, string type)
        {
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var message = new MeteringPointMessage()
            {
                SettlementMethod = obj.SettlementMethod,
                CorrelationId = obj.CorrelationId,
                MRID = obj.Mrid,
                MarketDocument = new MeteringPointMessage.Types._MarketDocument()
                {
                    ProcessType = obj.MarketDocument.ProcessType,
                    MRID = obj.MarketDocument.Mrid,
                    CreatedDateTime = Timestamp.FromDateTime(obj.MarketDocument.CreatedDateTime.ToDateTimeUtc()),
                    MarketServiceCategoryKind = obj.MarketDocument.MarketServiceCategoryKind,
                    RecipientMarketParticipant = new MeteringPointMessage.Types._MarketDocument.Types._RecipientMarketParticipant()
                    {
                        Type = obj.MarketDocument.RecipientMarketParticipant.Type,
                        MRID = obj.MarketDocument.RecipientMarketParticipant.Mrid,
                    },
                    SenderMarketParticipant = new MeteringPointMessage.Types._MarketDocument.Types._SenderMarketParticipant()
                    {
                        Type = obj.MarketDocument.SenderMarketParticipant.Type,
                        MRID = obj.MarketDocument.SenderMarketParticipant.Mrid,
                    },
                    Type = obj.MarketDocument.Type,
                },
                MarketEvaluationPointMRID = obj.MarketEvaluationPointMrid,
                MarketEvaluationPointType = obj.MarketEvaluationPointType,
                MessageReference = obj.MessageReference,
                MktActivityRecordStatus = obj.MktActivityRecordStatus,
                Period = new MeteringPointMessage.Types._Period()
                {
                    Points = new MeteringPointMessage.Types._Period.Types._Points()
                    {
                        Quality = obj.Period.Points.Quality,
                        Quantity = obj.Period.Points.Quantity,
                        Time = Timestamp.FromDateTime(obj.Period.Points.Time.ToDateTimeUtc()),
                    },
                    Resolution = obj.Period.Resolution,
                    TimeInterval = new MeteringPointMessage.Types._Period.Types._TimeInterval()
                    {
                        Start = Timestamp.FromDateTime(obj.Period.TimeInterval.Start.ToDateTimeUtc()),
                        End = Timestamp.FromDateTime(obj.Period.TimeInterval.End.ToDateTimeUtc()),
                    },
                },
            };
            return message;
        }
    }
}
