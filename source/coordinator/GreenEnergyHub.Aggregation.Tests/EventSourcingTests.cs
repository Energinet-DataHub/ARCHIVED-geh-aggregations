using System;
using System.Collections.Generic;
using System.Text;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    public class EventSourcingTests
    {
        [Fact]
        public void OnlyCreatedEventTest()
        {
            var events = new List<IEvent>();

            var createdEvent = new MeteringPointCreatedEvent("1")
            {
                Connected = false,
                EffectuationDate = "2020-01-01",
                MeteringPointType = "E17",
                SettlementMethod = "D01",
            };

            events.Add(createdEvent);

            var linkedList = new LinkedList<MeteringPoint>();


        }
    }
}
