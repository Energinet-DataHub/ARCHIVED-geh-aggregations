using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentAssertions;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using NodaTime;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class EventSourcingTests
    {
        private const string MeteringPointId = "123";
        private readonly List<IEvent> _events;
        private readonly Instant _now;
        private readonly List<IReplayableObject> _list;

        public EventSourcingTests()
        {
            _events = new List<IEvent>();
            _now = SystemClock.Instance.GetCurrentInstant();
            var createEvent = new MeteringPointCreatedEvent(MeteringPointId)
            {
                Connected = false,
                EffectuationDate = _now,
                MeteringPointType = "E17",
                SettlementMethod = "D01",
            };
            _events.Add(createEvent);

            var connectEvent = new MeteringPointConnectedEvent(MeteringPointId)
            {
                EffectuationDate = _now.Plus(Duration.FromHours(1)),
            };
            _events.Add(connectEvent);

            var settlementMethodEvent = new MeteringPointChangeSettlementMethodEvent(MeteringPointId, "CX")
            {
                EffectuationDate = _now.Plus(Duration.FromMinutes(30)),
            };

            _events.Add(settlementMethodEvent);

            var meteringPointDisconnectedEvent = new MeteringPointDisconnectedEvent(MeteringPointId)
            {
                EffectuationDate = _now.Plus(Duration.FromHours(2)),
            };

            _events.Add(meteringPointDisconnectedEvent);

            //Replay
            _list = new List<IReplayableObject>();
            foreach (var @event in _events)
            {
                _list = @event.GetObjectsAfterMutate(_list, @event.EffectuationDate);
            }
        }

        [Fact]
        public void Create_MeteringPoint_Events_And_Test_Replay_Creates_Correct_First_Period()
        {
            var firstPeriod = (MeteringPoint)_list.ToArray()[0];

            //Assert the first period should be from 30 to 60 min
            firstPeriod.ValidFrom.ShouldBeEquivalentTo(_now);
            firstPeriod.ValidTo.ShouldBeEquivalentTo(_now.Plus(Duration.FromMinutes(30)));
            firstPeriod.Connected = false;
            firstPeriod.MeteringPointType = "E17";
            firstPeriod.SettlementMethod = "D01";
            firstPeriod.MeteringPointId = MeteringPointId;
        }

        [Fact]
        public void Create_MeteringPoint_Events_And_Test_Replay_Creates_Correct_Second_Period()
        {
            var firstPeriod = (MeteringPoint)_list.ToArray()[1];

            //Assert the first period 30 min to 1 hour
            firstPeriod.ValidFrom.ShouldBeEquivalentTo(_now.Plus(Duration.FromMinutes(30)));
            firstPeriod.ValidTo.ShouldBeEquivalentTo(_now.Plus(Duration.FromMinutes(60)));
            firstPeriod.Connected = true;
            firstPeriod.MeteringPointType = "E17";
            firstPeriod.SettlementMethod = "D01";
            firstPeriod.MeteringPointId = MeteringPointId;
        }

        [Fact]
        public void Create_MeteringPoint_Events_And_Test_Replay_Creates_Correct_Third_Period()
        {
            var firstPeriod = (MeteringPoint)_list.ToArray()[2];

            //Assert the third period 1 to 2 hours
            firstPeriod.ValidFrom.ShouldBeEquivalentTo(_now.Plus(Duration.FromMinutes(60)));
            firstPeriod.ValidTo.ShouldBeEquivalentTo(_now.Plus(Duration.FromMinutes(120)));
            firstPeriod.Connected = true;
            firstPeriod.MeteringPointType = "E17";
            firstPeriod.SettlementMethod = "CX";
            firstPeriod.MeteringPointId = MeteringPointId;
        }

        [Fact]
        public void Create_MeteringPoint_Events_And_Test_Replay_Creates_Correct_Last_Period()
        {
            var firstPeriod = (MeteringPoint)_list.ToArray()[3];

            //Assert the fourth period 2 hours to max
            firstPeriod.ValidFrom.ShouldBeEquivalentTo(_now.Plus(Duration.FromMinutes(120)));
            firstPeriod.ValidTo.ShouldBeEquivalentTo(Instant.MaxValue);
            firstPeriod.Connected = false;
            firstPeriod.MeteringPointType = "E17";
            firstPeriod.SettlementMethod = "CX";
            firstPeriod.MeteringPointId = MeteringPointId;
        }
    }
}
