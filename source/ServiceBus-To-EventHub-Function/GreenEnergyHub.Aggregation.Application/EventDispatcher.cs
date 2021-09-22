using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Interfaces;
using GreenEnergyHub.Aggregation.Domain.ChargeLink;

namespace GreenEnergyHub.Aggregation.Application
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IEventHubService _eventHubService;

        public EventDispatcher(IEventHubService eventHubService)
        {
            _eventHubService = eventHubService;
        }

        public async Task DispatchAsync(byte[] msgData)
        {
            var obj = ChargeLinkCreatedContract.Parser.ParseFrom(msgData);
            await _eventHubService.SendEventHubTestMessageAsync(System.Text.Json.JsonSerializer.Serialize(obj)).ConfigureAwait(false);
        }
    }
}
