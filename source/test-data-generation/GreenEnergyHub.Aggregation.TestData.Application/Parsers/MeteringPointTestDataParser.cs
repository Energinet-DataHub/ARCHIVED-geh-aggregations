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
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Application.Service;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;

namespace GreenEnergyHub.Aggregation.TestData.Application.Parsers
{
    public class MeteringPointTestDataParser : TestDataParserBase
    {
        public MeteringPointTestDataParser(IMasterDataStorage masterDataStorage)
            : base(masterDataStorage)
        {
        }

        public override string FileNameICanHandle => "MeteringPoints.csv";

        public override async Task ParseAsync(Stream stream)
        {
            var mp = new MeteringPoint();
            var reader = new StreamReader(stream);
            var header = reader.ReadLine();
            var list = new List<MeteringPoint>();
            while (!reader.EndOfStream)
            {
                var meteringPointData = reader.ReadLine().Split(";");
                var meteringPointObject = new MeteringPoint()
                {
                    MeteringPointId = meteringPointData[0], // MarketEvaluationPoint_mRID
                    MeteringPointType = int.Parse(meteringPointData[1]), // MarketEvaluationPointType
                    SettlementMethod = int.Parse(meteringPointData[2]), // SettlementMethod
                    MeteringGridArea = meteringPointData[3], // MeteringGridArea_Domain_mRID
                    ConnectionState = int.Parse(meteringPointData[4]), // ConnectionState
                    MeterReadingPeriodicity = int.Parse(meteringPointData[5]), // MeterReadingPeriodicity
                    FromDate = DateTime.Parse(meteringPointData[6]), // ValidFrom
                    ToDate = DateTime.Parse(meteringPointData[7]), // ValidTo
                };
                list.Add(meteringPointObject);
            }

            var json = JsonSerializer.Serialize(list);
            Console.WriteLine(json);
            await MasterDataStorage.WriteMeteringPointAsync(mp).ConfigureAwait(false);
        }
    }
}
