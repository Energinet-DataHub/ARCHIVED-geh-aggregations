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
        private readonly IDataCollector _dataCollector;

        public CimXmlConverter(IGuidGenerator guidGenerator, IInstantGenerator instantGenerator, IDataCollector dataCollector)
        {
            _guidGenerator = guidGenerator;
            _instantGenerator = instantGenerator;
            _dataCollector = dataCollector;
        }

        public IEnumerable<OutgoingResult> Convert(IEnumerable<ResultData> results, JobCompletedEvent messageData)
        {
            var list = new List<IEnumerable<IEnumerable<ResultData>>>();
            list.Add(ResultGroupingMDR(results));
            // TODO: list.Add(ResultGroupingDDK(results));
            // TODO: list.Add(ResultGroupingDDQ(results));
            foreach (var resultGrouping in list)
            {
                var resultsGrouped = resultGrouping
                    .Select(g => g
                        .GroupBy(y => y.ResultName));
                foreach (var group in resultsGrouped)
                {
                    yield return Map(group, messageData);
                }
            }
        }

        private static IEnumerable<IEnumerable<ResultData>> ResultGroupingMDR(IEnumerable<ResultData> results)
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

        private static IEnumerable<IEnumerable<ResultData>> ResultGroupingDDK(IEnumerable<ResultData> results)
        {
            return results
                .GroupBy(x => new { x.BalanceResponsibleId, x.GridArea });
        }

        private static IEnumerable<IEnumerable<ResultData>> ResultGroupingDDQ(IEnumerable<ResultData> results)
        {
            return results
                .GroupBy(x => new { x.EnergySupplierId, x.GridArea });
        }

        private OutgoingResult Map(IEnumerable<IGrouping<string, ResultData>> result, JobCompletedEvent messageData)
        {
            var recipient = _dataCollector.GetRecipientData(result.First().First().GridArea);
            var messageId = _guidGenerator.GetGuidAsStringOnlyDigits();
            XDocument document = new XDocument(
                new XElement(
                    CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.NotifyRootElement,
                    new XAttribute(
                        XNamespace.Xmlns + CimXmlXNameConstants.XmlSchemaNamespaceAbbreviation,
                        CimXmlXNameSpace.XmlSchemaNameSpace),
                    new XAttribute(
                        XNamespace.Xmlns + CimXmlXNameConstants.CimNamespaceAbbreviation,
                        CimXmlXNameSpace.CimNamespace),
                    new XAttribute(
                        CimXmlXNameSpace.XmlSchemaNameSpace + CimXmlXNameConstants.SchemaLocation,
                        CimXmlXNameSpace.XmlSchemaLocation),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Id,
                        messageId),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Type,
                        CimXmlContentConstants.Type),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.ProcessType,
                        messageData.ProcessType.GetDescription()),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.SectorType,
                        CimXmlContentConstants.SectorTypeElectricity),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.SenderId,
                        new XAttribute(
                            CimXmlXNameConstants.CodingSchema,
                            CimXmlContentConstants.GlnCodingSchema),
                        CimXmlContentConstants.DataHubGlnNumber),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.SenderRole,
                        CimXmlContentConstants.DataHubRole),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.RecipientId,
                        new XAttribute(
                            CimXmlXNameConstants.CodingSchema,
                            recipient.CodingSchema),
                        recipient.Id),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.RecipientRole,
                        recipient.Role),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.CreatedDateTime,
                        _instantGenerator.GetCurrentDateTimeUtc()),
                    GetSeries(result, messageData)));
            return new OutgoingResult() { ResultId = messageId, Document = document };
        }

        private IEnumerable<XElement> GetSeries(IEnumerable<IGrouping<string, ResultData>> result, JobCompletedEvent messageData)
        {
            foreach (var series in result)
            {
                yield return new XElement(
                    CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Series,
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Id,
                        _guidGenerator.GetGuidAsStringOnlyDigits()),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Version,
                        messageData.Version),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.MeteringPointType,
                        series.First().MeteringPointType),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.SettlementMethod,
                        series.First().SettlementMethod),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.GridArea,
                        new XAttribute(
                            CimXmlXNameConstants.CodingSchema,
                            CimXmlContentConstants.GridAreaCodingSchemaForDenmark),
                        series.First().GridArea),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Product,
                        CimXmlContentConstants.ProductElectricity),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Unit,
                        CimXmlContentConstants.UnitElectricity),
                    GetPeriod(series, messageData));
            }
        }

#pragma warning disable SA1204
        private static XElement GetPeriod(IGrouping<string, ResultData> series, JobCompletedEvent messageData)
        {
            return new XElement(
                CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Period,
                new XElement(
                    CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Resolution,
                    series.First().Resolution),
                new XElement(
                    CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.TimeInterval,
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.TimeIntervalStart,
                        messageData.FromDate),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.TimeIntervalEnd,
                        messageData.ToDate)),
                GetPoints(series));
        }

        private static IEnumerable<XElement> GetPoints(IGrouping<string, ResultData> series)
        {
            var pointIndex = 1;
            foreach (var point in series.OrderBy(t => t.StartDateTime))
            {
                yield return new XElement(
                    CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Point,
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Position,
                        pointIndex),
                    new XElement(
                        CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Quantity,
                        point.SumQuantity),
                    point.Quality == "56" ? new XElement(CimXmlXNameSpace.CimNamespace + CimXmlXNameConstants.Quality, point.Quality) : null!);
                pointIndex++;
            }
        }
    }
}
