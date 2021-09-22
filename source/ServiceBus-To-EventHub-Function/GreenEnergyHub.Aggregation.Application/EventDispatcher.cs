using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using GreenEnergyHub.Aggregation.Application.Interfaces;
using GreenEnergyHub.Aggregation.Domain.Contracts;
using GreenEnergyHub.Aggregation.Domain.Contracts.ChargeLink;

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
            object obj = null;
            //since we don't have the ability to add interfaces to the probuf contracts we have to do everything by string matching
            switch (contractName)
            {
                case "charge-link-created":
                    obj = ChargeLinkCreated.Parser.ParseFrom(msgData);
                    break;
                //TODO map all know events and their protobuf contracts
            }

            if (obj == null)
            {
                throw new ArgumentException("Could not map contract in dispatcher");
            }

            await _eventHubService.SendEventHubTestMessageAsync(System.Text.Json.JsonSerializer.Serialize(obj)).ConfigureAwait(false);
        }
    }
}
