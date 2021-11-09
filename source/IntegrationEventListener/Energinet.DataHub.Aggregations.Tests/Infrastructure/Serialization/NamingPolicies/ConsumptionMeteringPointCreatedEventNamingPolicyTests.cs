using System;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Aggregations.Infrastructure.Serialization.NamingPolicies;
using Energinet.DataHub.Aggregations.Tests.Attributes;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Infrastructure.Serialization.NamingPolicies
{
    [UnitTest]
    public class ConsumptionMeteringPointCreatedEventNamingPolicyTests
    {
        [Theory]
        [InlineAutoMoqData("MeteringPointId", "metering_point_id")]
        [InlineAutoMoqData("MeteringPointType", "metering_point_type")]
        [InlineAutoMoqData("GridArea", "grid_area")]
        [InlineAutoMoqData("SettlementMethod", "settlement_method")]
        [InlineAutoMoqData("MeteringMethod", "metering_method")]
        [InlineAutoMoqData("Resolution", "resolution")]
        [InlineAutoMoqData("Product", "product")]
        [InlineAutoMoqData("ConnectionState", "connection_state")]
        [InlineAutoMoqData("Unit", "unit")]
        [InlineAutoMoqData("EffectiveDate", "effective_date")]
        [InlineAutoMoqData("MessageVersion", "MessageVersion")]
        [InlineAutoMoqData("MessageType", "MessageType")]
        [InlineAutoMoqData("Transaction", "Transaction")]
        public void ConvertName_KnownProperty_ReturnsCorrectPropertyName(
            string propertyName,
            string expected,
            [NotNull] ConsumptionMeteringPointCreatedEventNamingPolicy sut)
        {
            // Act
            var actual = sut.ConvertName(propertyName);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ConvertName_UnknownProperty_ThrowsException()
        {
            // Arrange
            var sut = new ConsumptionMeteringPointCreatedEventNamingPolicy();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => sut.ConvertName("UnknownProperty"));
        }
    }
}
