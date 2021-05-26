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

        Task UpdateAsync(Job job);

        Task CreateResultItemAsync(string resultId, Result result);

        Task UpdateResultItemAsync(Result result);
    }
}
