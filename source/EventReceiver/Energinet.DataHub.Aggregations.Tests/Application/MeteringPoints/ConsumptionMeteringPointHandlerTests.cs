using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Energinet.DataHub.Aggregations.Application.Interfaces;
using Energinet.DataHub.Aggregations.Application.MeteringPoints;
using Energinet.DataHub.Aggregations.Tests.Attributes;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Application.MeteringPoints
{
    [UnitTest]
    public class ConsumptionMeteringPointHandlerTests
    {
        [Theory]
        [InlineAutoMoqData]
        public async Task Handle_WhenCalled_ShouldSerializeToJsonAndDispatch(
            [NotNull] ConsumptionMeteringPointCreatedCommand request,
            [NotNull][Frozen] Mock<IEventDispatcher> eventDispatcher,
            [NotNull][Frozen] Mock<IJsonSerializer> jsonSerializer,
            [NotNull] ConsumptionMeteringPointHandler sut,
            CancellationToken cancellationToken)
        {
            // Act
            await sut.Handle(request, cancellationToken);

            // Assert
            jsonSerializer.Verify(method => method.Serialize(request), Times.Once);
            eventDispatcher.Verify(method => method.DispatchAsync(It.IsAny<string>(), cancellationToken), Times.Once);
        }
    }
}
