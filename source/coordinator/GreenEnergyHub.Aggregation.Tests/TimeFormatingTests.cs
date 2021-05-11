// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
