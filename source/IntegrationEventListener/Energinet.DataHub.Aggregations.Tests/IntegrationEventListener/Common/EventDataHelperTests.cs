using System;
using System.Collections.Generic;
using Energinet.DataHub.Aggregations.Common;
using Energinet.DataHub.Aggregations.Infrastructure.Serialization;
using FluentAssertions;
using Microsoft.Azure.Functions.Worker;
using Moq;
using NodaTime;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.IntegrationEventListener.Common
{
    [UnitTest]
    public class EventDataHelperTests
    {
        private readonly string _expectedEventIdentification = "eventIdentification";
        private readonly string _expectedMessageType = "messageType";
        private readonly string _expectedOperationCorrelationId = "operationCorrelationId";
        private readonly int _expectedMessageVersion = 1;
        private readonly Instant _expectedOperationTimestamp = Instant.FromUtc(2020, 1, 1, 0, 0);

        [Fact]
        public void GetEventMetaData_ThrowsArgumentNullException_WhenContextIsNull()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            Assert.Throws<ArgumentNullException>(() => sut.GetEventMetaData(null));
        }

        [Fact]
        public void GetEventMetaData_ThrowsInvalidOperationException_WhenUserPropertiesDoesNotExist()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext();

            Assert.Throws<InvalidOperationException>(() => sut.GetEventMetaData(context.Object));
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenEventIdentification_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(SetEventMetadata());

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("EventIdentification is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenMessageType_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(SetEventMetadata(eventIdentification: _expectedEventIdentification));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("MessageType is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenOperationCorrelationId_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("OperationCorrelationId is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenMessageVersion_IsNotSet()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("MessageVersion is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ThrowsArgumentException_WhenOperationTimestamp_IsMinValue()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var context = GetContext(SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion));

            var exception = Assert.Throws<ArgumentException>(() => sut.GetEventMetaData(context.Object));

            Assert.Equal("OperationTimestamp is not set", exception.Message);
        }

        [Fact]
        public void GetEventMetaData_ReturnsEventMetadataObject()
        {
            var sut = new EventDataHelper(new JsonSerializer());

            var expectedJson = SetEventMetadata(
                eventIdentification: _expectedEventIdentification,
                messageType: _expectedMessageType,
                operationCorrelationId: _expectedOperationCorrelationId,
                messageVersion: _expectedMessageVersion,
                operationTimestamp: _expectedOperationTimestamp);

            var expected = new JsonSerializer().Deserialize<EventMetadata>(expectedJson);

            var context = GetContext(expectedJson);

            var result = sut.GetEventMetaData(context.Object);

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

        private string SetEventMetadata(
            Instant? operationTimestamp = null,
            int messageVersion = 0,
            string messageType = "",
            string eventIdentification = "",
            string operationCorrelationId = "")
        {
            return new JsonSerializer().Serialize(new EventMetadata(messageVersion, messageType, eventIdentification, operationTimestamp ?? Instant.MinValue, operationCorrelationId));
        }
    }
}
