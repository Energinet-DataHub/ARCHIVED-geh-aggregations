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
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime.Text;
using Enum = System.Enum;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class CombinedGridLossStrategy : BaseStrategy<CombinedGridLoss>, IDispatchStrategy
    {
        private readonly IGLNService _glnService;
        private readonly ISpecialMeteringPointsService _specialMeteringPointsService;

        public CombinedGridLossStrategy(
            IGLNService glnService,
            ISpecialMeteringPointsService specialMeteringPointsService,
            ILogger<CombinedGridLoss> logger,
            TimeseriesDispatcher timeseriesDispatcher)
        : base(logger, timeseriesDispatcher)
        {
            _glnService = glnService;
            _specialMeteringPointsService = specialMeteringPointsService;
        }

        public string FriendlyNameInstance => "combined_grid_loss";

        public override IEnumerable<IOutboundMessage> PrepareMessages(
            IEnumerable<CombinedGridLoss> list,
            ProcessType processType,
            string timeIntervalStart,
            string timeIntervalEnd)
        {
            // TODO: Implement Mapping
            return list.Select(x => new MeteringPointMessage()
            {
                MRID = "1",
                MessageReference = "1",
                MarketDocument = new MeteringPointMessage.Types._MarketDocument()
                {
                    MRID = "1",
                    Type = "2",
                    CreatedDateTime = Timestamp.FromDateTime(DateTime.Now),
                    SenderMarketParticipant =
                        new MeteringPointMessage.Types._MarketDocument.Types._SenderMarketParticipant()
                        {
                            MRID = _glnService.GetSenderGln(), Type = "2",
                        },
                    RecipientMarketParticipant =
                        new MeteringPointMessage.Types._MarketDocument.Types._RecipientMarketParticipant()
                        {
                            MRID = _specialMeteringPointsService.GridLossOwner(
                                x.MeteringGridAreaDomainMRID,
                                InstantPattern.ExtendedIso.Parse(x.ValidFrom.ToString()).GetValueOrThrow()),
                            Type = "2",
                        },
                    ProcessType = Enum.GetName(typeof(ProcessType), processType),
                    MarketServiceCategoryKind = "4",
                },
                MktActivityRecordStatus = "1",
                Product = "1",
                QuantityMeasurementUnitName = "1",
                MarketEvaluationPointType = x.MarketEvaluationPointType,
                SettlementMethod = x.SettlementMethod,
                MarketEvaluationPointMRID = x.MarketEvaluationPointMRID,
                CorrelationId = "1",
                Period = new MeteringPointMessage.Types._Period()
                {
                    Resolution = x.MeterReadingPeriodicity,
                    TimeInterval =
                        new MeteringPointMessage.Types._Period.Types._TimeInterval()
                        {
                            Start = x.TimeWindow.Start.ToUniversalTime().ToTimestamp(), End = x.TimeWindow.End.ToUniversalTime().ToTimestamp(),
                        },
                    Points = new MeteringPointMessage.Types._Period.Types._Points()
                    {
                        Quantity = x.AddedSystemCorrection,
                        Quality = "1",
                        Time = Timestamp.FromDateTime(DateTime.Now),
                    },
                },
            }).Select(x => new MeteringPointOutboundMessage(x));
        }
    }
}
