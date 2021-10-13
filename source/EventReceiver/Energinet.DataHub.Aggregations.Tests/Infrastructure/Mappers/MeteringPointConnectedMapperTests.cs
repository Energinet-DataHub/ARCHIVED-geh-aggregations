using System;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.MeteringPoints;
using Energinet.DataHub.Aggregations.Infrastructure.Mappers;
using Energinet.DataHub.Aggregations.Tests.Attributes;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Infrastructure.Mappers
{
    [UnitTest]
    public class MeteringPointConnectedMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapProtobufToInbound(
            [NotNull] MeteringPointConnected protobufMessage,
            [NotNull] MeteringPointConnectedMapper sut)
        {
            // Arrange
            protobufMessage.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var result = sut.Convert(protobufMessage) as MeteringPointConnectedEvent;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(protobufMessage.MeteringPointId, result.MeteringPointId);
            Assert.Equal(protobufMessage.EffectiveDate.Seconds, result.EffectiveDate.ToUnixTimeSeconds());
        }
    }
}
