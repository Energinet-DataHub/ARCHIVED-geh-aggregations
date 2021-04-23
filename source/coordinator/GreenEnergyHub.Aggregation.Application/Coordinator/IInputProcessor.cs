using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.Types;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// Input processor
    /// </summary>
    public interface IInputProcessor
    {
        /// <summary>
        /// This method processes the input,finds the appropriate dispatch strategy which then dispatches it
        /// </summary>
        /// <param name="nameOfAggregation"></param>
        /// <param name="blobStream"></param>
        /// <param name="token"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <param name="cancellationToken"></param>
        Task ProcessInputAsync(
            string nameOfAggregation,
            Stream blobStream,
            ProcessType token,
            string startTime,
            string endTime,
            CancellationToken cancellationToken);
    }
}
