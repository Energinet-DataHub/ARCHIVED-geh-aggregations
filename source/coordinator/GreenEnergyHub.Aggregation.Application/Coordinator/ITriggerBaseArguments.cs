using System.Collections.Generic;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// Trigger base arguments
    /// </summary>
    public interface ITriggerBaseArguments
    {
        /// <summary>
        /// Returns base arguments used for databricks job trigger functions
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="processType"></param>
        /// <param name="persist"></param>
        /// <returns>List of strings</returns>
        List<string> GetTriggerBaseArguments(Instant beginTime, Instant endTime, string processType, bool persist);
    }
}
