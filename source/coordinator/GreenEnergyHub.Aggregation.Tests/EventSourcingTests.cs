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
            var createEvent = new MeteringPointCreatedEvent("123")
            {
                Connected = false,
                EffectuationDate = NodaTime.SystemClock.Instance.GetCurrentInstant().ToString(),
                MeteringPointType = "E17",
                SettlementMethod = "D01",
            };

            var evts = new List<IEvent>();
            evts.Add(createEvent);
            var list = evts.AsEnumerable().First().GetObjectsAfterMutate(new List<MeteringPoint>());

            var connectEvent = new MeteringPointConnectedEvent("123")
            {
                EffectuationDate = SystemClock.Instance.GetCurrentInstant().Plus(Duration.FromHours(1)).ToDateTimeUtc(),
            };

            evts.Add(connectEvent);
            var list2 = evts.AsEnumerable().Last().GetObjectsAfterMutate(list);
        }
    }
}
