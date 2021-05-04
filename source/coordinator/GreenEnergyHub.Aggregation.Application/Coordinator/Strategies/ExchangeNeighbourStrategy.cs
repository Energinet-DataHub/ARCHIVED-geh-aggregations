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
    public class ExchangeNeighbourStrategy : BaseStrategy<ExchangeNeighbourDto>, IDispatchStrategy
    {
        private readonly IGLNService _glnService;

        public ExchangeNeighbourStrategy(ILogger<ExchangeNeighbourDto> logger, IGLNService glnService, Dispatcher dispatcher)
            : base(logger, dispatcher)
        {
            _glnService = glnService;
        }

        public string FriendlyNameInstance => "net_exchange_per_neighbour_df";

        public override IEnumerable<IOutboundMessage> PrepareMessages(IEnumerable<ExchangeNeighbourDto> aggregationResultList, ProcessType processType, string timeIntervalStart, string timeIntervalEnd)
        {
            if (aggregationResultList == null) throw new ArgumentNullException(nameof(aggregationResultList));

            foreach (var exchangeDto in aggregationResultList)
            {
                yield return new AggregatedExchangeNeighbourResultMessage()
                {
                    MeteringGridAreaDomainMRID = exchangeDto.MeteringGridAreaDomainMRID,
                    InMeteringGridAreaDomainMRID = exchangeDto.InMeteringGridAreaDomainMRID,
                    OutMeteringGridAreaDomainMRID = exchangeDto.OutMeteringGridAreaDomainMRID,
                    Result = exchangeDto.Result,
                    MarketEvaluationPointType = MarketEvaluationPointType.Exchange,
                    AggregationType = CoordinatorSettings.ExchangeNeighbourName,
                    ProcessType = Enum.GetName(typeof(ProcessType), processType),
                    TimeIntervalStart = timeIntervalStart,
                    TimeIntervalEnd = timeIntervalEnd,
                    ReceiverMarketParticipantMRID = _glnService.GetEsettGln(),
                    SenderMarketParticipantMRID = _glnService.GetSenderGln(),
                    Transaction = new Transaction(),
                };
            }
        }
    }
}
