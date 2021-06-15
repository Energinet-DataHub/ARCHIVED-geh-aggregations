using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;

namespace GreenEnergyHub.Aggregation.Infrastructure
{
    /// <summary>
    /// Provides CRUD access to metadata
    /// </summary>
    public interface IMetaDataDataAccess
    {
        Task CreateJobAsync(Job job);

        Task UpdateJobAsync(Job job);

        Task CreateResultItemAsync(Result result);

        Task UpdateResultItemAsync(Result result);
    }
}
