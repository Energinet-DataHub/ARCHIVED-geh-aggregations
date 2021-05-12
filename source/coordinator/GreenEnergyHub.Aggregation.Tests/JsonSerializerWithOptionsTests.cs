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

using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Infrastructure;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Aggregation.Tests
{
    [UnitTest]
    public class JsonSerializerWithOptionsTests
    {
        private readonly JsonSerializerWithOption _sut;

        public JsonSerializerWithOptionsTests()
        {
            _sut = new JsonSerializerWithOption();
        }

        [Fact]
        public void JsonSerializer_SerializeFromTimeDto_ReturnIso8601()
        {
            // arrange
            const string expected = "2021-01-01T01:00:00Z";
            var timeDto = new TimeDto() { Time = Instant.FromUtc(2021, 1, 1, 1, 0) };

            // act
            var json = _sut.Serialize(timeDto);
            using var actual = JsonDocument.Parse(json);

            // assert
            Assert.Equal(expected, actual.RootElement.GetProperty("Time").GetString());
        }

        [Fact]
        public async Task JsonSerializer_DeserializeFromIso8601_ReturnNodaTimeInstant()
        {
            // arrange
            var expected = Instant.FromUtc(2021, 1, 1, 1, 0);
            const string jsonTime = "{\"Time\":\"2021-01-01T01:00:00Z\"}";

            // act
            await using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(jsonTime));
            var test = await _sut.DeserializeAsync<TimeDto>(stream, CancellationToken.None).ConfigureAwait(false);

            // assert
            Assert.Equal(expected, test.Time);
        }

        private class TimeDto
        {
            public Instant Time { get; set; }
        }
    }
}
