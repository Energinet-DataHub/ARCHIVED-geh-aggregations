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
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.Contracts;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;
using NodaTime.Text;

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
            var validTime = InstantPattern.ExtendedIso.Parse(timeIntervalStart).GetValueOrThrow();

            // TODO: Implement Mapping
            var msg = new MeteringPointMessage()
            {
                MRID = "1",
                MessageReference = "2",
                MarketDocument = new MeteringPointMessage.Types._MarketDocument()
                {
                    MRID = "1",
                    Type = "2",
                    CreatedDateTime = Timestamp.FromDateTime(DateTime.Now),
                    SenderMarketParticipant =
                        new MeteringPointMessage.Types._MarketDocument.Types._SenderMarketParticipant()
                        {
                            MRID = "1", Type = "2",
                        },
                    RecipientMarketParticipant =
                        new MeteringPointMessage.Types._MarketDocument.Types._RecipientMarketParticipant()
                        {
                            MRID = "1", Type = "2",
                        },
                    ProcessType = "3",
                    MarketServiceCategoryKind = "4",
                },
                MktActivityRecordStatus = "1",
                Product = "1",
                QuantityMeasurementUnitName = "1",
                MarketEvaluationPointType = "1",
                SettlementMethod = "1",
                MarketEvaluationPointMRID = "1",
                CorrelationId = "1",
                Period = new MeteringPointMessage.Types._Period()
                {
                    Resolution = "1",
                    TimeInterval =
                        new MeteringPointMessage.Types._Period.Types._TimeInterval()
                        {
                            Start = Timestamp.FromDateTime(DateTime.Now),
                            End = Timestamp.FromDateTime(DateTime.Now),
                        },
                    Points = new MeteringPointMessage.Types._Period.Types._Points()
                    {
                        Quantity = 1.0, Quality = "1",
                        Time = Timestamp.FromDateTime(DateTime.Now),
                    },
                },
            };

            return null;
        }
    }
}
