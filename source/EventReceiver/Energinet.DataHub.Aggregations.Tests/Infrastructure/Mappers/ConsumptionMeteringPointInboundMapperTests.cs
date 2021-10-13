using System;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Aggregations.Application.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Infrastructure.Mappers;
using Energinet.DataHub.Aggregations.Tests.Attributes;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.Tests.Infrastructure.Mappers
{
    [UnitTest]
    public class ConsumptionMeteringPointInboundMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapProtobufToInbound(
            [NotNull] ConsumptionMeteringPointCreated protobufMessage,
            [NotNull] ConsumptionMeteringPointCreatedInboundMapper sut)
        {
            // Arrange
            protobufMessage.SettlementMethod = ConsumptionMeteringPointCreated.Types.SettlementMethod.SmFlex;
            protobufMessage.MeteringMethod = ConsumptionMeteringPointCreated.Types.MeteringMethod.MmPhysical;
            protobufMessage.MeterReadingPeriodicity = ConsumptionMeteringPointCreated.Types.MeterReadingPeriodicity.MrpHourly;
            protobufMessage.ConnectionState = ConsumptionMeteringPointCreated.Types.ConnectionState.CsNew;
            protobufMessage.Product = ConsumptionMeteringPointCreated.Types.ProductType.PtEnergyactive;
            protobufMessage.UnitType = ConsumptionMeteringPointCreated.Types.UnitType.UtKwh;
            protobufMessage.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var result = sut.Convert(protobufMessage) as ConsumptionMeteringPointCreatedCommand;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(protobufMessage.MeteringPointId, result.MeteringPointId);
            Assert.Equal(MeteringPointType.Consumption, result.MeteringPointType);
            Assert.Equal(protobufMessage.GridAreaCode, result.MeteringGridArea);
            Assert.Equal(SettlementMethod.Flex, result.SettlementMethod);
            Assert.Equal(MeteringMethod.Physical, result.MeteringMethod);
            Assert.Equal(MeterReadingPeriodicity.Hourly, result.MeterReadingPeriodicity);
            Assert.Equal(ConnectionState.New, result.ConnectionState);
            Assert.Equal(Product.EnergyActive, result.Product);
            Assert.Equal(QuantityUnit.Kwh, result.QuantityUnit);
            Assert.Equal(protobufMessage.EffectiveDate.Seconds, result.EffectiveDate.ToUnixTimeSeconds());
        }
    }
}
