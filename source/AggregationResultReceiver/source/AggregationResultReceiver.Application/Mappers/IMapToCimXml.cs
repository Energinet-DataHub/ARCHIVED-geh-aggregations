using System.Collections.Generic;
using System.Xml.Linq;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Mappers
{
    /// <summary>
    /// IMapToCimXml
    /// </summary>
    public interface IMapToCimXml
    {
        /// <summary>
        /// Map
        /// </summary>
        IEnumerable<XDocument> Map(IEnumerable<ResultData> results, ResultsReadyForConversion messageData);
    }
}
