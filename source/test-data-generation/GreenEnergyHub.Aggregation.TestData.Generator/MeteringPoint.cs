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

using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.Generator
{
    public static class MeteringPointFunction
    {
        [FunctionName("MeteringPointBlobTrigger")]
        public static void Run([BlobTrigger("test-data-source/{name}", Connection = "TEST_DATA_SOURCE_CONNECTION_STRING")]Stream myblob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myblob.Length} Bytes");
            var reader = new StreamReader(myblob);
            var header = reader.ReadLine();
            var list = new List<object>();
            while (!reader.EndOfStream)
            {
                var meteringPointData = reader.ReadLine().Split(";");
                var meteringPointObject = new
                {
                    meteringPointId = meteringPointData[0], // MarketEvaluationPoint_mRID
                    meteringPointType = meteringPointData[1], // MarketEvaluationPointType
                    settlementMethod = meteringPointData[2], // SettlementMethod
                    meteringGridArea = meteringPointData[3], // MeteringGridArea_Domain_mRID
                    connectionState = meteringPointData[4], // ConnectionState
                    meterReadingPeriodicity = meteringPointData[5], // MeterReadingPeriodicity
                    fromDate = meteringPointData[6], // ValidFrom
                    toDate = meteringPointData[7], // ValidTo
                };
                list.Add(meteringPointObject);
            }

            var json = JsonSerializer.Serialize(list);

            // TODO Replace data in cosmos db
            log.LogInformation(json);
        }
    }
}
