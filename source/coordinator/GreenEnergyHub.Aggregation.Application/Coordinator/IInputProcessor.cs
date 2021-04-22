using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// Input processor
    /// </summary>
    public interface IInputProcessor
    {
        /// <summary>
        /// This method processes the input and finds the appropriate dispatch strategy
        /// </summary>
        /// <param name="nameOfAggregation"></param>
        /// <param name="blobStream"></param>
        /// <param name="cancellationToken"></param>
        Task ProcessInputAsync(string nameOfAggregation, Stream blobStream, CancellationToken cancellationToken);
    }
}
