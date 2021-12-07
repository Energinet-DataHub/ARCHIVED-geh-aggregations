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

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Converters
{
    internal static class CimXmlContentConstants
    {
        internal const string Type = "E31";

        internal const string SectorTypeElectricity = "23";

        internal const string ProductElectricity = "8716867000030";

        internal const string UnitElectricity = "KWH";

        internal const string DataHubGlnNumber = "5790001330552";

        internal const string DataHubRole = "DGL";

        internal const string GlnCodingSchema = "A10";

        internal const string GridAreaCodingSchemaForDenmark = "NDK";
    }
}
