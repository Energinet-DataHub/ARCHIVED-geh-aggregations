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
using System.Linq;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class Step0809CombinedGridLossStrategy : BaseStrategy<CombinedGridLossDto>, IDispatchStrategy
    {
        private readonly IGLNService _glnService;

        public Step0809CombinedGridLossStrategy(
            IGLNService glnService,
            ILogger<CombinedGridLossDto> logger,
            TimeSeriesDispatcher timeSeriesDispatcher,
            IJsonSerializer jsonSerializer)
        : base(logger, timeSeriesDispatcher, jsonSerializer)
        {
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "combined_grid_loss";

        public override IEnumerable<IOutboundMessage> PrepareMessages(
            IEnumerable<CombinedGridLossDto> list,
            string processType,
            Instant timeIntervalStart,
            Instant timeIntervalEnd)
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
                    CreatedDateTime = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    SenderMarketParticipant =
                        new MeteringPointMessage.Types._MarketDocument.Types._SenderMarketParticipant()
                        {
                            MRID = _glnService.GetSenderGln(),
                            Type = "2",
                        },
                    RecipientMarketParticipant =
                        new MeteringPointMessage.Types._MarketDocument.Types._RecipientMarketParticipant()
                        {
                            MRID = x.EnergySupplierMarketParticipantmRID,
                            Type = "2",
                        },
                    ProcessType = processType,
                    MarketServiceCategoryKind = "4",
                },
                MktActivityRecordStatus = "1",
                Product = "1",
                QuantityMeasurementUnitName = "1",
                MarketEvaluationPointType = x.MarketEvaluationPointType,
                SettlementMethod = x.SettlementMethod,
                MarketEvaluationPointMRID = x.MarketEvaluationPointmRID,
                CorrelationId = "1",
                Period = new MeteringPointMessage.Types._Period()
                {
                    Resolution = x.MeterReadingPeriodicity,
                    TimeInterval =
                        new MeteringPointMessage.Types._Period.Types._TimeInterval()
                        {
                            Start = x.TimeStart.ToDateTimeUtc().ToTimestamp(),
                            End = x.TimeEnd.ToDateTimeUtc().ToTimestamp(),
                        },
                    Points = new MeteringPointMessage.Types._Period.Types._Points()
                    {
                        Quantity = x.AddedSystemCorrection,
                        Quality = "1",
                        Time = Timestamp.FromDateTime(DateTime.Now.ToUniversalTime()),
                    },
                },
            }).Select(x => new MeteringPointOutboundMessage(x));
        }
    }
}
