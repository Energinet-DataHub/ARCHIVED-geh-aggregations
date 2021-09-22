using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Interfaces;

namespace GreenEnergyHub.Aggregation.Application
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IEventHubService _eventHubService;

        public EventDispatcher(IEventHubService eventHubService)
        {
            _eventHubService = eventHubService;
        }

        public async Task DispatchAsync(byte[] msgData, string contractName)
        {
            await _eventHubService.SendEventHubMessageAsync(msgData).ConfigureAwait(false);
        }
    }
}
