using System.Collections.Generic;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Converters
{
    /// <summary>
    /// IMapToCimXml
    /// </summary>
    public interface ICimXmlConverter
    {
        /// <summary>
        /// Convert
        /// </summary>
        IEnumerable<OutgoingResult> Convert(IEnumerable<DataResult> results, JobCompletedEvent messageData);
    }
}
