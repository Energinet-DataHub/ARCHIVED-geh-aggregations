﻿// Copyright 2020 Energinet DataHub A/S
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
using System.IO;
using System.Text.Json;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using Microsoft.Extensions.FileProviders;
using NSubstitute.Exceptions;

namespace GreenEnergyHub.Aggregation.Tests.Assets
{
    public class TestData
    {
        private readonly EmbeddedFileProvider _fileProvider;

        public TestData()
        {
            _fileProvider = new EmbeddedFileProvider(GetType().Assembly);
        }

        public IEnumerable<ConsumptionDto> FlexConsumption => GetTestData<ConsumptionDto>($"{nameof(FlexConsumption)}");

        public IEnumerable<GridLossDto> GridLoss => GetTestData<GridLossDto>($"{nameof(GridLoss)}");

        public IEnumerable<SystemCorrectionDto> SystemCorrection => GetTestData<SystemCorrectionDto>($"{nameof(SystemCorrection)}");

        public IEnumerable<ConsumptionDto> HourlyConsumption => GetTestData<ConsumptionDto>($"{nameof(HourlyConsumption)}");

        public IEnumerable<ProductionDto> HourlyProduction => GetTestData<ProductionDto>($"{nameof(HourlyProduction)}");

        public IEnumerable<ConsumptionDto> HourlySettledConsumption => GetTestData<ConsumptionDto>($"{nameof(HourlySettledConsumption)}");

        public IEnumerable<ExchangeDto> Exchange => GetTestData<ExchangeDto>($"{nameof(Exchange)}");

        public IEnumerable<ExchangeNeighbourDto> ExchangeNeighbour => GetTestData<ExchangeNeighbourDto>($"{nameof(ExchangeNeighbour)}");

        private IEnumerable<T> GetTestData<T>(string fileName)
        {
            var fileInfo = _fileProvider.GetFileInfo($"Assets.{fileName}.json");
            if (!fileInfo.Exists)
            {
                throw new Exception("Could not find file. Did you perhaps forget to embed it ?");
            }

            var stream = fileInfo.CreateReadStream();
            using var reader = new StreamReader(stream);
            return JsonSerializer.Deserialize<IEnumerable<T>>(reader.ReadToEnd());
        }
    }
}
