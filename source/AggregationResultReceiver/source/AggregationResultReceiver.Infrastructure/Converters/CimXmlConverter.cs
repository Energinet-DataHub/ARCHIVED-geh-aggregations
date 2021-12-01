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
using System.Linq;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Converters;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Helpers;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Utilities;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain.Enums;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.Converters
{
    public class CimXmlConverter : ICimXmlConverter
    {
        private readonly IGuidGenerator _guidGenerator;
        private readonly IInstantGenerator _instantGenerator;

        public CimXmlConverter(IGuidGenerator guidGenerator, IInstantGenerator instantGenerator)
        {
            _guidGenerator = guidGenerator;
            _instantGenerator = instantGenerator;
        }

        public IEnumerable<OutgoingResult> Convert(IEnumerable<ResultData> results, JobCompletedEvent messageData)
        {
            var list = new List<IEnumerable<IEnumerable<ResultData>>>();
            list.Add(ResultGroupingMDR(results));
            // TODO: list.Add(ResultGroupingDDK(results));
            // TODO: list.Add(ResultGroupingDDQ(results));
            foreach (var resultGrouping in list)
            {
                var resultsGrouped = resultGrouping // use grouping from messageData
                    .Select(g => g
                        .GroupBy(y => y.ResultName));
                foreach (var group in resultsGrouped)
                {
                    yield return Map(group, messageData);
                }
            }
        }

        private static XElement GetPeriod(IGrouping<string, ResultData> s, XNamespace cimNamespace, JobCompletedEvent messageData)
        {
            return new XElement(
                cimNamespace + CimXmlXNameConstants.Period,
                new XElement(
                    cimNamespace + CimXmlXNameConstants.Resolution,
                    s.First().Resolution),
                new XElement(
                    cimNamespace + CimXmlXNameConstants.TimeInterval,
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.TimeIntervalStart,
                        messageData.FromDate),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.TimeIntervalEnd,
                        messageData.ToDate)),
                GetPoints(s, cimNamespace));
        }

        private static IEnumerable<XElement> GetPoints(IGrouping<string, ResultData> s, XNamespace cimNamespace)
        {
            var pointIndex = 1;
            foreach (var point in s.OrderBy(t => t.StartDateTime))
            {
                yield return new XElement(
                    cimNamespace + CimXmlXNameConstants.Point,
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Position,
                        pointIndex),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Quantity,
                        point.SumQuantity),
                    point.Quality == "56" ? new XElement(cimNamespace + CimXmlXNameConstants.Quality, point.Quality) : null!);
                pointIndex++;
            }
        }

        private IEnumerable<IEnumerable<ResultData>> ResultGroupingMDR(IEnumerable<ResultData> results)
        {
            return results
                .GroupBy(x => new { x.GridArea })
                .Select(x => x
                    .Select(y => y)
                    .Where(b => b.ResultName == ResultName.TotalConsumption.ToString() ||
                                b.ResultName == ResultName.FlexConsumptionPerGridArea.ToString() ||
                                b.ResultName == ResultName.HourlySettledConsumptionPerGridArea.ToString() ||
                                b.ResultName == ResultName.HourlyProductionPerGridArea.ToString() ||
                                b.ResultName == ResultName.NetExchangePerGridArea.ToString()));
        }

        private IEnumerable<IEnumerable<ResultData>> ResultGroupingDDK(IEnumerable<ResultData> results)
        {
            return results
                .GroupBy(x => new { x.BalanceResponsibleId, x.GridArea });
        }

        private IEnumerable<IEnumerable<ResultData>> ResultGroupingDDQ(IEnumerable<ResultData> results)
        {
            return results
                .GroupBy(x => new { x.EnergySupplierId, x.GridArea });
        }

        private OutgoingResult Map(IEnumerable<IGrouping<string, ResultData>> result, JobCompletedEvent messageData) // include message from coordinator
        {
            var messageId = _guidGenerator.GetGuidAsStringOnlyDigits();
            XNamespace cimNamespace = CimXmlXNameConstants.CimNamespace;
            XNamespace xmlSchemaNamespace = CimXmlXNameConstants.XmlSchemaNameSpace;
            XNamespace xmlSchemaLocation = CimXmlXNameConstants.XmlSchemaLocation;
            XDocument document = new XDocument(
                new XElement(
                    cimNamespace + CimXmlXNameConstants.NotifyRootElement,
                    new XAttribute(
                        XNamespace.Xmlns + CimXmlXNameConstants.XmlSchemaNamespaceAbbreviation,
                        xmlSchemaNamespace),
                    new XAttribute(
                        XNamespace.Xmlns + CimXmlXNameConstants.CimNamespaceAbbreviation,
                        cimNamespace),
                    new XAttribute(
                        xmlSchemaNamespace + CimXmlXNameConstants.SchemaLocation,
                        xmlSchemaLocation),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Id,
                        messageId),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Type,
                        CimXmlContentConstants.Type),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.ProcessType,
                        messageData.ProcessType.GetDescription()),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.SectorType,
                        CimXmlContentConstants.SectorTypeElectricity),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.SenderId,
                        new XAttribute(
                            CimXmlXNameConstants.CodingSchema,
                            CimXmlContentConstants.GlnCodingSchema), // const: A10 for gln
                        CimXmlContentConstants.DataHubGlnNumber), // const: datahub gln number
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.SenderRole,
                        CimXmlContentConstants.DataHubRole), // const: role of datahub
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.RecipientId,
                        new XAttribute(
                            CimXmlXNameConstants.CodingSchema,
                            "A10"), // get from some where
                        "5799999933318"), // gln
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.RecipientRole,
                        "MDR"), // get from coordinator message
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.CreatedDateTime,
                        _instantGenerator.GetCurrentDateTimeUtc()),
                    GetSeries(result, cimNamespace, messageData)));
            return new OutgoingResult() { ResultId = messageId, Document = document };
        }

        private IEnumerable<XElement> GetSeries(IEnumerable<IGrouping<string, ResultData>> item, XNamespace cimNamespace, JobCompletedEvent messageData)
        {
            foreach (var s in item)
            {
                yield return new XElement(
                    cimNamespace + CimXmlXNameConstants.Series,
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Id,
                        _guidGenerator.GetGuidAsStringOnlyDigits()),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Version,
                        messageData.Version), // get from coordinator message
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.MeteringPointType,
                        s.First().MeteringPointType),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.SettlementMethod,
                        s.First().SettlementMethod),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.GridArea,
                        new XAttribute(
                            CimXmlXNameConstants.CodingSchema,
                            CimXmlContentConstants.GridAreaCodingSchemaForDenmark), // const: NDK is grid areas of denmark
                        s.First().GridArea),
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Product,
                        CimXmlContentConstants.ProductElectricity), // const: product type
                    new XElement(
                        cimNamespace + CimXmlXNameConstants.Unit,
                        CimXmlContentConstants.UnitElectricity),
                    GetPeriod(s, cimNamespace, messageData));
            }
        }
    }
}
