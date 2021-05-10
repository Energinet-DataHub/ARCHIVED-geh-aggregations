using System.Text.Json;
using GreenEnergyHub.Aggregation.Application.Utilities;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using Xunit;
using Xunit.Abstractions;

namespace GreenEnergyHub.Aggregation.Tests
{
    public class TimeFormatingTests
    {
        private readonly ITestOutputHelper _output;

        public TimeFormatingTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Time_Serialize_Test()
        {
            var options = new JsonSerializerOptions();
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

            var timeDto = new TimeDTO() { Time = SystemClock.Instance.GetCurrentInstant() };

            var test = JsonSerializer.Serialize(timeDto, options);

            _output.WriteLine(test);
            _output.WriteLine(timeDto.Time.ToIso8601GeneralString());
        }

        [Fact]
        public void Time_Deserialize_Test()
        {
            var options = new JsonSerializerOptions();
            options.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            var jsonTime = "{\"Time\":\"2021-01-01T01:01:01Z\"}";

            var test = JsonSerializer.Deserialize<TimeDTO>(jsonTime, options);

            _output.WriteLine(test.Time.ToString());
        }

        public class TimeDTO
        {
            public Instant Time { get; set; }
        }
    }
}
