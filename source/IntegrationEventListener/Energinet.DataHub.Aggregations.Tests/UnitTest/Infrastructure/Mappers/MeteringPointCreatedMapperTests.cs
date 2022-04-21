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
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Energinet.DataHub.Aggregations.Application.IntegrationEvents.DTOs.MeteringPoints;
using Energinet.DataHub.Aggregations.Domain;
using Energinet.DataHub.Aggregations.Infrastructure.Mappers;
using Energinet.DataHub.Aggregations.Tests.UnitTest.Attributes;
using Energinet.DataHub.MeteringPoints.IntegrationEventContracts;
using Google.Protobuf.WellKnownTypes;
using Xunit;
using Xunit.Categories;
using mpTypes = Energinet.DataHub.MeteringPoints.IntegrationEventContracts.MeteringPointCreated.Types;

namespace Energinet.DataHub.Aggregations.Tests.UnitTest.Infrastructure.Mappers
{
    [UnitTest]
    public class MeteringPointCreatedMapperTests
    {
        [Theory]
        [InlineAutoMoqData]
        public void Convert_WhenCalled_ShouldMapProtobufToInbound(
            [NotNull] MeteringPointCreated protobufMessage,
            [NotNull] MeteringPointCreatedMapper sut)
        {
            // Arrange
            protobufMessage.SettlementMethod = mpTypes.SettlementMethod.SmFlex;
            protobufMessage.MeteringMethod = mpTypes.MeteringMethod.MmPhysical;
            protobufMessage.MeterReadingPeriodicity = mpTypes.MeterReadingPeriodicity.MrpHourly;
            protobufMessage.ConnectionState = mpTypes.ConnectionState.CsNew;
            protobufMessage.Product = mpTypes.ProductType.PtEnergyactive;
            protobufMessage.UnitType = mpTypes.UnitType.UtKwh;
            protobufMessage.EffectiveDate = Timestamp.FromDateTime(new DateTime(2021, 10, 31, 23, 00, 00, 00, DateTimeKind.Utc));

            // Act
            var result = sut.Convert(protobufMessage) as MeteringPointCreatedEvent;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(protobufMessage.GsrnNumber, result.MeteringPointId);
            Assert.Equal(MeteringPointType.Consumption, result.MeteringPointType);
            Assert.Equal(protobufMessage.GridAreaCode, result.GridArea);
            Assert.Equal(SettlementMethod.Flex, result.SettlementMethod);
            Assert.Equal(MeteringMethod.Physical, result.MeteringMethod);
            Assert.Equal(Resolution.Hourly, result.Resolution);
            Assert.Equal(ConnectionState.New, result.ConnectionState);
            Assert.Equal(Product.EnergyActive, result.Product);
            Assert.Equal(Unit.Kwh, result.Unit);
            Assert.Equal(protobufMessage.EffectiveDate.Seconds, result.EffectiveDate.ToUnixTimeSeconds());
        }

        [Theory]
        [InlineAutoMoqData]
        public void Convert_whenCalledWithNull_ShouldThrow([NotNull] MeteringPointCreatedMapper sut)
        {
            Assert.Throws<InvalidOperationException>(() => sut.Convert(null));
        }

        [Theory]
        [InlineData(mpTypes.ConnectionState.CsNew, ConnectionState.New)]
        public void MapConnectionState_WhenCalled_ShouldMapCorrectly(
            mpTypes.ConnectionState protoConnectionState,
            ConnectionState connectionState)
        {
            var actual = ProtobufToDomainTypeMapper.MapConnectionState(protoConnectionState);

            Assert.Equal(connectionState, actual);
        }

        [Theory]
        [InlineData((mpTypes.ConnectionState)999)]
        public void MapConnectionState_WhenCalledWithInvalidValue_ShouldThrow(
            mpTypes.ConnectionState protoConnectionState)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProtobufToDomainTypeMapper.MapConnectionState(protoConnectionState));
        }

        [Theory]
        [InlineData(mpTypes.UnitType.UtWh, Unit.Wh)]
        [InlineData(mpTypes.UnitType.UtGwh, Unit.Gwh)]
        [InlineData(mpTypes.UnitType.UtMwh, Unit.Mwh)]
        [InlineData(mpTypes.UnitType.UtKwh, Unit.Kwh)]
        public void MapUnitType_WhenCalled_ShouldMapCorrectly(
            mpTypes.UnitType protoUnitType,
            Unit unitType)
        {
            var actual = ProtobufToDomainTypeMapper.MapUnitType(protoUnitType);

            Assert.Equal(unitType, actual);
        }

        [Theory]
        [InlineData((mpTypes.UnitType)999)]
        public void MapUnitType_WhenCalledWithInvalidValue_ShouldThrow(
            mpTypes.UnitType protoUnitType)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProtobufToDomainTypeMapper.MapUnitType(protoUnitType));
        }

        [Theory]
        [InlineData(mpTypes.ProductType.PtEnergyreactive, Product.EnergyReactive)]
        [InlineData(mpTypes.ProductType.PtFuelquantity, Product.FuelQuantity)]
        [InlineData(mpTypes.ProductType.PtPoweractive, Product.PowerActive)]
        [InlineData(mpTypes.ProductType.PtPowerreactive, Product.PowerReactive)]
        [InlineData(mpTypes.ProductType.PtTariff, Product.Tariff)]
        [InlineData(mpTypes.ProductType.PtEnergyactive, Product.EnergyActive)]
        public void MapProduct_WhenCalled_ShouldMapCorrectly(
            mpTypes.ProductType protoProductType,
            Product productType)
        {
            var actual = ProtobufToDomainTypeMapper.MapProduct(protoProductType);

            Assert.Equal(productType, actual);
        }

        [Theory]
        [InlineData((mpTypes.ProductType)999)]
        public void MapProductType_WhenCalledWithInvalidValue_ShouldThrow(
            mpTypes.ProductType protoProductType)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProtobufToDomainTypeMapper.MapProduct(protoProductType));
        }

        [Theory]
        [InlineData(mpTypes.MeterReadingPeriodicity.MrpHourly, Resolution.Hourly)]
        [InlineData(mpTypes.MeterReadingPeriodicity.MrpQuarterly, Resolution.Quarterly)]
        public void MapMeterReadingPeriodicity_WhenCalled_ShouldMapCorrectly(
            mpTypes.MeterReadingPeriodicity protoMeterReadingPeriodicity,
            Resolution resolution)
        {
            var actual = ProtobufToDomainTypeMapper.MapMeterReadingPeriodicity(protoMeterReadingPeriodicity);

            Assert.Equal(resolution, actual);
        }

        [Theory]
        [InlineData((mpTypes.MeterReadingPeriodicity)999)]
        public void MapMeterReadingPeriodicity_WhenCalledWithInvalidValue_ShouldThrow(
            mpTypes.MeterReadingPeriodicity protoMeterReadingPeriodicity)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProtobufToDomainTypeMapper.MapMeterReadingPeriodicity(protoMeterReadingPeriodicity));
        }

        [Theory]
        [InlineData(mpTypes.MeteringMethod.MmCalculated, MeteringMethod.Calculated)]
        [InlineData(mpTypes.MeteringMethod.MmPhysical, MeteringMethod.Physical)]
        [InlineData(mpTypes.MeteringMethod.MmVirtual, MeteringMethod.Virtual)]
        public void MapMeteringMethod_WhenCalled_ShouldMapCorrectly(
            mpTypes.MeteringMethod protoMeteringMethod,
            MeteringMethod meteringMethod)
        {
            var actual = ProtobufToDomainTypeMapper.MapMeteringMethod(protoMeteringMethod);

            Assert.Equal(meteringMethod, actual);
        }

        [Theory]
        [InlineData((mpTypes.MeteringMethod)999)]
        public void MapMeteringMethod_WhenCalledWithInvalidValue_ShouldThrow(
            mpTypes.MeteringMethod protoMeteringMethod)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProtobufToDomainTypeMapper.MapMeteringMethod(protoMeteringMethod));
        }

        [Theory]
        [InlineData(mpTypes.SettlementMethod.SmNull, null)]
        [InlineData(mpTypes.SettlementMethod.SmFlex, SettlementMethod.Flex)]
        [InlineData(mpTypes.SettlementMethod.SmNonprofiled, SettlementMethod.NonProfiled)]
        [InlineData(mpTypes.SettlementMethod.SmProfiled, SettlementMethod.Profiled)]
        public void MapSettlementMethod_WhenCalled_ShouldMapCorrectly(
            mpTypes.SettlementMethod protoSettlementMethod,
            SettlementMethod? settlementMethod)
        {
            var actual = ProtobufToDomainTypeMapper.MapSettlementMethod(protoSettlementMethod);

            Assert.Equal(settlementMethod, actual);
        }

        [Theory]
        [InlineData((mpTypes.SettlementMethod)999)]
        public void MapSettlementMethod_WhenCalledWithInvalidValue_ShouldThrow(
            mpTypes.SettlementMethod protoSettlementMethod)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ProtobufToDomainTypeMapper.MapSettlementMethod(protoSettlementMethod));
        }
    }
}
