using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    /// <summary>
    /// Provides CRUD access to metadata
    /// </summary>
    public interface IMetaDataDataAccess
    {
        /// <summary>
        /// Insert Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateJobAsync(Job job);

        /// <summary>
        /// Update Job
        /// </summary>
        /// <param name="job"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateJobAsync(Job job);

        /// <summary>
        /// Insert job
        /// </summary>
        /// <param name="result"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task CreateResultItemAsync(Result result);

        /// <summary>
        /// Update job
        /// </summary>
        /// <param name="result"></param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        Task UpdateResultItemAsync(Result result);
    }
}
