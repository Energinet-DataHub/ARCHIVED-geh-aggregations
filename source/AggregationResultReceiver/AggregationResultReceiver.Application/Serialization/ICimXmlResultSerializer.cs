using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AggregationResultReceiver.Domain;

namespace AggregationResultReceiver.Application.Serialization
{
    /// <summary>
    /// Interface for generating CIM/XML streams based on aggregation results
    /// </summary>
    public interface ICimXmlResultSerializer
    {
        /// <summary>
        /// Serialize aggregation result to an XML stream using a CIM/XML schema
        /// </summary>
        /// <param name="results">Aggregation results</param>
        /// <param name="stream">Stream to resulting CIM/XML</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task SerializeToStreamAsync(IEnumerable<ResultData> results, Stream stream);
    }
}
