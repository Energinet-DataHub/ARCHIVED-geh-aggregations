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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AutoFixture.Xunit2;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain.Enums;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.Assets;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests.TestHelpers;
using Xunit;
using Xunit.Categories;
using JsonSerializer = Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Serialization.JsonSerializer;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Tests
{
    [UnitTest]
    public class JsonSerializerTests
    {
        [Theory]
        [AutoData]
        public void Deserialize_JobCompletedEvent_ReturnsValidObject(
            [NotNull] TestDocuments testDocuments,
            [NotNull] JsonSerializer sut)
        {
            // Arrange
            var json = testDocuments.JobCompletedEvent;

            // Act
            var actual = sut.Deserialize<JobCompletedEvent>(json);

            // Assert
            Assert.NotNull(actual);
            Assert.Equal(ProcessType.WholesaleFixing, actual.ProcessType);
            Assert.Equal(ProcessVariant.ThirdRun, actual.ProcessVariant);
            Assert.Equal(Resolution.Hourly, actual.Resolution);
            Assert.Equal("https://some.path", actual.Results.First().ResultPath);
            Assert.Equal(Grouping.Neighbour, actual.Results.First().Grouping);
            Assert.Equal(15, actual.Results.ToList().Count);
        }

        [Theory]
        [AutoData]
        public void DeserializeStream_CanMapStreamToObject([NotNull] JsonSerializer sut)
        {
            var expected = new List<User>()
            {
                new User() { FirstName = "John", LastName = "Doe" },
                new User() { FirstName = "Jane", LastName = "Doe" },
            };

            var stream = TestDataGenerator.EmbeddedResourceAssetReader("DeserializeStreamTestData.json");
            var actual = sut.DeserializeStream<User>(stream).ToList();

            Assert.Equal(System.Text.Json.JsonSerializer.Serialize(expected), System.Text.Json.JsonSerializer.Serialize(actual));
        }

        private class User
        {
            public string FirstName { get; set; }

            public string LastName { get; set; }
        }
    }
}
