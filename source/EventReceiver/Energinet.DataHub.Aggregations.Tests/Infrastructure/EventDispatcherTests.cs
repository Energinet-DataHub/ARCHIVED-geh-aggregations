using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Aggregations.Infrastructure;
using Energinet.DataHub.Aggregations.Tests.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Infrastructure
{
    [UnitTest]
    public class EventDispatcherTests : InlineAutoDataAttribute
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task DispatchAsync_Called_ShouldSendMessage(
            string message,
            [NotNull][Frozen] Mock<IEventHubService> eventHubService,
            [NotNull] EventDispatcher sut)
        {
            // Act
            await sut.DispatchAsync(message);

            // Assert
            eventHubService.Verify(method => method.SendEventHubMessageAsync(message, It.IsAny<CancellationToken>()));
        }
    }
}
