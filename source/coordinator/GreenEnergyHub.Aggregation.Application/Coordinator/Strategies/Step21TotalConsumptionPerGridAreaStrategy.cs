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
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using Microsoft.Extensions.Logging;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class Step21TotalConsumptionPerGridAreaStrategy : BaseStrategy<AggregationResultDto, AggregationResultMessage>, IDispatchStrategy
    {
        private readonly GlnService _glnService;

        public Step21TotalConsumptionPerGridAreaStrategy(ILogger<AggregationResultDto> logger, IMessageDispatcher messageDispatcher, GlnService glnService)
            : base(logger, messageDispatcher)
        {
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "total_consumption";

        public override IEnumerable<AggregationResultMessage> PrepareMessages(IEnumerable<AggregationResultDto> aggregationResultList, string processType, Instant timeIntervalStart, Instant timeIntervalEnd)
        {
            CheckArguments(aggregationResultList);

            var dtos = aggregationResultList;

            foreach (var aggregationResults in dtos.GroupBy(e => new { e.MeteringGridAreaDomainmRID }))
            {
                yield return CreateMessage(aggregationResults, processType, ProcessRole.MeterDataResponsible, timeIntervalStart, timeIntervalEnd, _glnService.DataHubGln, aggregationResults.First().MeteringGridAreaDomainmRID, MarketEvaluationPointType.Consumption);
            }
        }
    }
}
