using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs.Producer;
using Energinet.DataHub.Aggregations.Infrastructure;
using Energinet.DataHub.Aggregations.Infrastructure.Wrappers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Infrastructure
{
    [UnitTest]
    public class EventHubServiceTests
    {
        [Fact]
        public async Task SendEventHubMessageAsync_Called_ShouldCallEventHubProducerClient()
        {
            // Arrange
            var client = new Mock<IEventHubProducerClientWrapper>();
            var logger = new Mock<ILogger<EventHubProducerClientWrapper>>();
            const string message = "testMessage";
            var cancellationToken = CancellationToken.None;

            // Act
            var sut = new EventDispatcher(client.Object, logger.Object);
            await sut.DispatchAsync(message, cancellationToken);

            // Assert
            client.Verify(m => m.CreateEventBatchAsync(message, cancellationToken), Times.Once);
            client.Verify(m => m.SendAsync(It.IsAny<EventDataBatch>(), cancellationToken), Times.Once);
            client.Verify(m => m.CloseAsync(cancellationToken), Times.Once);
            client.Verify(m => m.DisposeAsync(), Times.Once);
            logger.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }

        [Fact]
        public async Task SendEventHubMessageAsync_CreatingEventBatchDataFails_ShouldLogAndReThrowException()
        {
            // Arrange
            var client = new Mock<IEventHubProducerClientWrapper>();
            var logger = new Mock<ILogger<EventHubProducerClientWrapper>>();
            const string message = "testMessage";
            var cancellationToken = CancellationToken.None;
            client.Setup(m => m.CreateEventBatchAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).Throws<Exception>();

            // Act
            var sut = new EventDispatcher(client.Object, logger.Object);

            // Assert
            await Assert.ThrowsAsync<Exception>(() => sut.DispatchAsync(message, cancellationToken));
            logger.Verify(
                m => m.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
        }
    }
}
