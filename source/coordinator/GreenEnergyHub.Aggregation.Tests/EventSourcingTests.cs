using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    public class EventSourcingTests
    {
        [Fact]
        public void Test_Creating()
        {
            var events = new List<IEvent>();
            var now = SystemClock.Instance.GetCurrentInstant();
            var createEvent = new MeteringPointCreatedEvent("123")
            {
                Connected = false,
                EffectuationDate = now,
                MeteringPointType = "E17",
                SettlementMethod = "D01",
            };
            events.Add(createEvent);

            var connectEvent = new MeteringPointConnectedEvent("123")
            {
                EffectuationDate = now.Plus(Duration.FromHours(1)),
            };
            events.Add(connectEvent);

            var settlementMethodEvent = new MeteringPointChangeSettlementMethodEvent("123", "CX")
            {
                EffectuationDate = now.Plus(Duration.FromMinutes(30)),
            };

            events.Add(settlementMethodEvent);

            var meteringPointDisconnectedEvent = new MeteringPointDisconnectedEvent("123")
            {
                EffectuationDate = now.Plus(Duration.FromHours(2)),
            };

            events.Add(meteringPointDisconnectedEvent);

            //Replay
            var list = new List<IReplayableObject>();
            foreach (var @event in events)
            {
                list = @event.GetObjectsAfterMutate(list, @event.EffectuationDate);
            }
        }
    }
}
