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

using System;
using System.Collections.Generic;
using Energinet.DataHub.Aggregations.Application.Extensions;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.Infrastructure.Serialization;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.UnitTest.IntegrationEventListener.Common
{
    [UnitTest]
    public class EventDataHelperTests
    {
        private readonly string _expectedEventIdentification = "eventIdentification";
        private readonly string _expectedMessageType = "messageType";
        private readonly string _expectedOperationCorrelationId = "operationCorrelationId";
        private readonly int _expectedMessageVersion = 1;
        private readonly Instant _expectedOperationTimestamp = Instant.FromUtc(2020, 1, 1, 0, 0);
        private readonly string _expectedDomain = "domain";
        private readonly EventDataHelper _sut;

        public EventDataHelperTests()
        {
            var mock = new Mock<ILogger<EventDataHelper>>();
            var logger = mock.Object;

            _sut = new EventDataHelper(new JsonSerializer(), logger);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentNullException_WhenContextIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _sut.GetEventMetaData(null));
        }

        [Fact]
        public void GetEventMetaData_ThrowsInvalidOperationException_WhenUserPropertiesDoesNotExist()
        {
            var context = GetContext();

            Assert.Throws<InvalidOperationException>(() => _sut.GetEventMetaData(context.Object));
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenEventIdentification_IsNotSet()
        {
            var context = GetContext(EventMetadataToJson(SetEventMetadata()));

            var exception = Assert.Throws<ArgumentException>(() => _sut.GetEventMetaData(context.Object));

            Assert.Equal("EventIdentification is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenMessageType_IsNotSet()
        {
            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification)));

            var exception = Assert.Throws<ArgumentException>(() => _sut.GetEventMetaData(context.Object));

            Assert.Equal("MessageType is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenOperationCorrelationId_IsNotSet()
        {
            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType)));

            var exception = Assert.Throws<ArgumentException>(() => _sut.GetEventMetaData(context.Object));

            Assert.Equal("OperationCorrelationId is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenMessageVersion_IsNotSet()
        {
            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId)));

            var exception = Assert.Throws<ArgumentException>(() => _sut.GetEventMetaData(context.Object));

            Assert.Equal("MessageVersion is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenOperationTimestamp_IsMinValue()
        {
            var context = GetContext(EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion)));

            var exception = Assert.Throws<ArgumentException>(() => _sut.GetEventMetaData(context.Object));

            Assert.Equal("OperationTimestamp is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ReturnsEventMetadataObject()
        {
            var expectedJson = EventMetadataToJson(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion,
                operationTimestamp: _expectedOperationTimestamp));

            var expected = new JsonSerializer().Deserialize<EventMetadata>(expectedJson);

            var context = GetContext(expectedJson);

            var result = _sut.GetEventMetaData(context.Object);

            result.Should().BeEquivalentTo(expected);
        }

        [Fact]
        public void GetEventHubMetaData_ThrowsArgumentNullException_WhenMetadata_IsNull()
        {
            var mock = new Mock<ILogger<EventDataHelper>>();
            var logger = mock.Object;

            Assert.Throws<ArgumentNullException>(() =>
                new EventDataHelper(new JsonSerializer(), logger).GetEventhubMetaData(null, null));
        }

        [Fact]
        public void GetEventHubMetaData_ReturnsDictionary()
        {
            var metadata = SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion,
                operationTimestamp: _expectedOperationTimestamp);

            var expected = new Dictionary<string, string>
            {
                { "event_id", _expectedEventIdentification },
                { "processed_date", _expectedOperationTimestamp.ToIso8601GeneralString() },
                { "event_name", _expectedMessageType },
                { "domain", _expectedDomain },
            };
            var mock = new Mock<ILogger<EventDataHelper>>();
            var logger = mock.Object;
            var result = new EventDataHelper(new JsonSerializer(), logger).GetEventhubMetaData(metadata, _expectedDomain);

            result.Should().BeEquivalentTo(expected);
        }

        private Mock<FunctionContext> GetContext(string metadata = null)
        {
            var context = new Mock<FunctionContext>();
            var bindingContext = new Mock<BindingContext>();
            var dict = new Dictionary<string, object?>();

            if (metadata != null)
            {
                dict.Add("UserProperties", metadata);
            }

            bindingContext.Setup(x => x.BindingData).Returns(dict);
            context.Setup(x => x.BindingContext).Returns(bindingContext.Object);

            return context;
        }

        private EventMetadata SetEventMetadata(
            Instant? operationTimestamp = null,
            int messageVersion = 0,
            string messageType = "",
            string eventIdentification = "",
            string operationCorrelationId = "")
        {
            return new EventMetadata(messageVersion, messageType, eventIdentification, operationTimestamp ?? Instant.MinValue, operationCorrelationId);
        }

        private string EventMetadataToJson(EventMetadata metadata)
        {
            return new JsonSerializer().Serialize(metadata);
        }
    }
}
