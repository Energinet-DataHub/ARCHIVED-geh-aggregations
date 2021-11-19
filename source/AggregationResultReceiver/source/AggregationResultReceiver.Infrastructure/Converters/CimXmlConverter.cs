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
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain.Enums;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Infrastructure.CimXml;

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

        public IEnumerable<OutgoingResult> Convert(IEnumerable<DataResult> dataResults, JobCompletedEvent messageData)
        {
            var dataResultGroupedOnGrouping = dataResults.GroupBy(x => x.Grouping);
            var outgoingResultList = new List<OutgoingResult>();
            foreach (var dataResultGrouping in dataResultGroupedOnGrouping)
            {
                var list = new List<ResultData>();
                foreach (var dataResult in dataResultGrouping)
                {
                    var results = dataResult.ResultDataCollection;
                    list.AddRange(results);
                }

                outgoingResultList.AddRange(ConvertCollectionOfResultDataFromSpecificGroup(list, messageData, dataResultGrouping.Key));
            }

            return outgoingResultList;
        }

        private IEnumerable<OutgoingResult> ConvertCollectionOfResultDataFromSpecificGroup(IEnumerable<ResultData> results, JobCompletedEvent messageData, Grouping grouping)
        {
            var resultsGrouped = ResultGrouping(results, grouping) // use grouping from messageData
                .Select(g => g
                    .GroupBy(y => y.ResultName)
                    .Select(h => h));
            foreach (var group in resultsGrouped)
            {
                yield return Map(group, messageData);
            }
        }

        private IEnumerable<IEnumerable<ResultData>> ResultGrouping(IEnumerable<ResultData> results, Grouping grouping)
        {
            switch (grouping)
            {
                case Grouping.EnergySupplier: // use grouping enum
                    return results
                        .GroupBy(x => new { x.EnergySupplierId, x.GridArea }) // grouping on grid area as well as energy supplier to make xml messages sent smaller in size
                        .Select(y => y.ToList()).ToList();
                case Grouping.BalanceResponsible:
                    return results
                        .GroupBy(x => new { x.BalanceResponsibleId, x.GridArea }) // grouping on grid area as well as balance responsible to make xml messages sent smaller in size
                        .Select(y => y.ToList()).ToList();
                case Grouping.GridArea:
                    return results
                        .GroupBy(x => new { x.GridArea })
                        .Select(y => y.ToList()).ToList();
                default:
                    return null;
            }
        }

        private OutgoingResult Map(IEnumerable<IGrouping<string, ResultData>> result, JobCompletedEvent messageData) // include message from coordinator
        {
            List<XDocument> cimXmlFiles = new List<XDocument>();
            var messageId = _guidGenerator.GetGuid();
            XNamespace cimNamespace = CimXmlConstants.CimNamespace;
            XNamespace xmlSchemaNamespace = CimXmlConstants.XmlSchemaNameSpace;
            XNamespace xmlSchemaLocation = CimXmlConstants.XmlSchemaLocation;
            XDocument document = new XDocument(
                new XElement(
                    cimNamespace + CimXmlConstants.NotifyRootElement,
                    new XAttribute(
                        XNamespace.Xmlns + CimXmlConstants.XmlSchemaNamespaceAbbreviation,
                        xmlSchemaNamespace),
                    new XAttribute(
                        XNamespace.Xmlns + CimXmlConstants.CimNamespaceAbbreviation,
                        cimNamespace),
                    new XAttribute(
                        xmlSchemaNamespace + CimXmlConstants.SchemaLocation,
                        xmlSchemaLocation),
                    new XElement(
                        cimNamespace + CimXmlConstants.Id,
                        messageId),
                    new XElement(
                        cimNamespace + CimXmlConstants.Type,
                        "E31"), // const
                    new XElement(
                        cimNamespace + CimXmlConstants.ProcessType,
                        "D04"), // get from coordinator message
                    new XElement(
                        cimNamespace + CimXmlConstants.SectorType,
                        "23"), // always 23 for electricity
                    new XElement(
                        cimNamespace + CimXmlConstants.SenderId,
                        new XAttribute(
                            CimXmlConstants.CodingSchema,
                            "A10"), // const: A10 is datahub
                        "5790001330552"), // const: datahub gln number
                    new XElement(
                        cimNamespace + CimXmlConstants.SenderRole,
                        "DGL"), // const: role of datahub
                    new XElement(
                        cimNamespace + CimXmlConstants.RecipientId,
                        new XAttribute(
                            CimXmlConstants.CodingSchema,
                            "A10"), // get from some where
                        "5799999933318"), // gln
                    new XElement(
                        cimNamespace + CimXmlConstants.RecipientRole,
                        "MDR"), // get from coordinator message
                    new XElement(
                        cimNamespace + CimXmlConstants.CreatedDateTime,
                        _instantGenerator.GetCurrentDateTimeUtc()),
                    GetSeries(result, cimNamespace)));
            return new OutgoingResult() { ResultId = messageId, Document = document };
        }

        private IEnumerable<XElement> GetSeries(IEnumerable<IGrouping<string, ResultData>> item, XNamespace cimNamespace)
        {
            List<XElement> series = new List<XElement>();
            foreach (var s in item)
            {
                series.Add(new XElement(
                    cimNamespace + CimXmlConstants.Series,
                    new XElement(
                        cimNamespace + CimXmlConstants.Id,
                        _guidGenerator.GetGuid()),
                    new XElement(
                        cimNamespace + CimXmlConstants.Version,
                        "1"), // get from coordinator message
                    new XElement(
                        cimNamespace + CimXmlConstants.MeteringPointType,
                        s.First().MeteringPointType),
                    new XElement(
                        cimNamespace + CimXmlConstants.SettlementMethod,
                        s.First().SettlementMethod),
                    new XElement(
                        cimNamespace + CimXmlConstants.GridArea,
                        new XAttribute(
                            CimXmlConstants.CodingSchema,
                            "NDK"), // const: NDK is grid areas of denmark
                        s.First().GridArea),
                    new XElement(
                        cimNamespace + CimXmlConstants.Product,
                        "8716867000030"), // const: product type
                    new XElement(
                        cimNamespace + CimXmlConstants.Unit,
                        "KWH"),
                    GetPeriod(s, cimNamespace)));
            }

            return series;
        }

        private XElement GetPeriod(IGrouping<string, ResultData> s, XNamespace cimNamespace)
        {
            return new XElement(
                cimNamespace + CimXmlConstants.Period,
                new XElement(
                    cimNamespace + CimXmlConstants.Resolution,
                    s.First().Resolution),
                new XElement(
                    cimNamespace + CimXmlConstants.TimeInterval,
                    new XElement(
                        cimNamespace + CimXmlConstants.TimeIntervalStart,
                        "2021-09-05T22:00Z"),
                    new XElement(
                        cimNamespace + CimXmlConstants.TimeIntervalEnd,
                        "2221-09-06T22:00Z")),
                GetPoints(s, cimNamespace));
        }

        private IEnumerable<XElement> GetPoints(IGrouping<string, ResultData> s, XNamespace cimNamespace)
        {
            List<XElement> points = new List<XElement>();
            var pointIndex = 1;
            foreach (var point in s.OrderBy(t => t.StartDateTime))
            {
                points.Add(new XElement(
                    cimNamespace + CimXmlConstants.Point,
                    new XElement(
                        cimNamespace + CimXmlConstants.Position,
                        pointIndex),
                    new XElement(
                        cimNamespace + CimXmlConstants.Quantity,
                        point.SumQuantity),
                    point.Quality == "56" ? new XElement(cimNamespace + CimXmlConstants.Quality, point.Quality) : null));
                pointIndex++;
            }

            return points;
        }
    }
}
