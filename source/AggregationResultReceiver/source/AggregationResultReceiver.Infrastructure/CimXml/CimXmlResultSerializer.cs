using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AggregationResultReceiver.Application.Serialization;
using AggregationResultReceiver.Domain;

namespace AggregationResultReceiver.Infrastructure.CimXml
{
    public class CimXmlResultSerializer : ICimXmlResultSerializer
    {
        public Task SerializeToStreamAsync(IEnumerable<ResultData> results, Stream stream)
        {
            throw new System.NotImplementedException();
        }
    }
}
