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
    internal static class CimXmlConstants
    {
        internal const string CimNamespace = "urn:ediel.org:measure:notifyaggregatedtimeseries:0:1";

        internal const string XmlSchemaNameSpace = "http://www.w3.org/2001/XMLSchema-instance";

        internal const string XmlSchemaLocation = "urn:ediel.org:measure:notifyaggregatedtimeseries:0:1 urn-ediel-org-measure-notifyaggregatedtimeseries-0-1.xsd";

        internal const string NotifyRootElement = "NotifyAggregatedTimeSeries_MarketDocument";

        internal const string XmlSchemaNamespaceAbbreviation = "xsi";

        internal const string CimNamespaceAbbreviation = "cim";

        internal const string SchemaLocation = "schemaLocation";

        internal const string CodingSchema = "codingScheme";

        internal const string Id = "mRID";

        internal const string Type = "type";

        internal const string ProcessType = "process.processType";

        internal const string SectorType = "businessSector.type";

        internal const string SenderId = "sender_MarketParticipant.mRID";

        internal const string SenderRole = "sender_MarketParticipant.marketRole.type";

        internal const string RecipientId = "receiver_MarketParticipant.mRID";

        internal const string RecipientRole = "receiver_MarketParticipant.marketRole.type";

        internal const string CreatedDateTime = "createdDateTime";

        internal const string Series = "Series";

        internal const string Version = "version";

        internal const string MeteringPointType = "marketEvaluationPoint.type";

        internal const string SettlementMethod = "marketEvaluationPoint.settlementMethod";

        internal const string GridArea = "meteringGridArea_Domain.mRID";

        internal const string Product = "product";

        internal const string Unit = "quantity_Measure_Unit.name";

        internal const string Period = "Period";

        internal const string Resolution = "resolution";

        internal const string TimeInterval = "timeInterval";

        internal const string TimeIntervalStart = "start";

        internal const string TimeIntervalEnd = "end";

        internal const string Point = "Point";

        internal const string Position = "position";

        internal const string Quantity = "quantity";

        internal const string Quality = "quality";
    }
}
