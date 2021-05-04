using System;
using System.Collections.Generic;
using GreenEnergyHub.Aggregation.Application.Services;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Aggregation.Domain.ResultMessages;
using GreenEnergyHub.Aggregation.Domain.Types;
using GreenEnergyHub.Aggregation.Infrastructure;
using GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf;
using GreenEnergyHub.Messaging.MessageTypes.Common;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Application.Coordinator.Strategies
{
    public class ExchangeStrategy : BaseStrategy<ExchangeDto>, IDispatchStrategy
    {
        private readonly IGLNService _glnService;

        public ExchangeStrategy(ILogger<ExchangeDto> logger, IGLNService glnService, Dispatcher dispatcher)
            : base(logger, dispatcher)
        {
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "net_exchange_per_ga_df";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<ExchangeDto> aggregationResultList, ProcessType processType, string timeIntervalStart, string timeIntervalEnd)
        {
            if (aggregationResultList == null) throw new ArgumentNullException(nameof(aggregationResultList));

            foreach (var exchangeDto in aggregationResultList)
            {
                yield return new AggregatedExhangeResultMessage
                {
                    MeteringGridAreaDomainMRID = exchangeDto.MeteringGridAreaDomainMRID,
                    Result = exchangeDto.Result,
                    Transaction = new Transaction(),
                    MarketEvaluationPointType = MarketEvaluationPointType.Exchange,
                    AggregationType = CoordinatorSettings.ExchangeName,
                    ProcessType = Enum.GetName(typeof(ProcessType), processType),
                    TimeIntervalStart = timeIntervalStart,
                    TimeIntervalEnd = timeIntervalEnd,
                    ReceiverMarketParticipantMRID = _glnService.GetEsettGln(),
                    SenderMarketParticipantMRID = _glnService.GetSenderGln(),
                };
            }
        }
    }
}
