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
using System.IO;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Messaging;
using Microsoft.Extensions.FileProviders;

namespace GreenEnergyHub.Aggregation.Tests.Assets
{
    public class TestData
    {
        private readonly EmbeddedFileProvider _fileProvider;
        private readonly JsonSerializerWithOption _jsonSerializer;

        public TestData()
        {
            _fileProvider = new EmbeddedFileProvider(GetType().Assembly);
            _jsonSerializer = new JsonSerializerWithOption();
        }

        public IEnumerable<AggregationResultDto> ConsumptionGa => GetTestData<AggregationResultDto>($"{nameof(ConsumptionGa)}");

        public IEnumerable<AggregationResultDto> ConsumptionGaBrp => GetTestData<AggregationResultDto>($"{nameof(ConsumptionGaBrp)}");

        public IEnumerable<AggregationResultDto> ConsumptionGaEs => GetTestData<AggregationResultDto>($"{nameof(ConsumptionGaEs)}");

        public IEnumerable<AggregationResultDto> ConsumptionGaBrpEs => GetTestData<AggregationResultDto>($"{nameof(ConsumptionGaBrpEs)}");

        public IEnumerable<AggregationResultDto> FlexConsumptionGa => GetTestData<AggregationResultDto>($"{nameof(FlexConsumptionGa)}");

        public IEnumerable<AggregationResultDto> FlexConsumptionGaBrp => GetTestData<AggregationResultDto>($"{nameof(FlexConsumptionGaBrp)}");

        public IEnumerable<AggregationResultDto> FlexConsumptionGaEs => GetTestData<AggregationResultDto>($"{nameof(FlexConsumptionGaEs)}");

        public IEnumerable<AggregationResultDto> FlexConsumptionGaBrpEs => GetTestData<AggregationResultDto>($"{nameof(FlexConsumptionGaBrpEs)}");

        public IEnumerable<AggregationResultDto> ProductionGa => GetTestData<AggregationResultDto>($"{nameof(ProductionGa)}");

        public IEnumerable<AggregationResultDto> ProductionGaBrp => GetTestData<AggregationResultDto>($"{nameof(ProductionGaBrp)}");

        public IEnumerable<AggregationResultDto> ProductionGaEs => GetTestData<AggregationResultDto>($"{nameof(ProductionGaEs)}");

        public IEnumerable<AggregationResultDto> ProductionGaBrpEs => GetTestData<AggregationResultDto>($"{nameof(ProductionGaBrpEs)}");

        public IEnumerable<GridLossDto> GridLoss => GetTestData<GridLossDto>($"{nameof(GridLoss)}");

        public IEnumerable<SystemCorrectionDto> SystemCorrection => GetTestData<SystemCorrectionDto>($"{nameof(SystemCorrection)}");

        public IEnumerable<AggregationResultDto> ExchangeGa => GetTestData<AggregationResultDto>($"{nameof(ExchangeGa)}");

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
            return _jsonSerializer.Deserialize<IEnumerable<T>>(reader.ReadToEnd());
        }
    }
}
