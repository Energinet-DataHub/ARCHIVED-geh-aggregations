using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GreenEnergyHub.Aggregation.Application.Coordinator
{
    /// <summary>
    /// This defines the interface for a dispatch strategy
    /// </summary>
    public interface IDispatchStrategy
    {
        /// <summary>
        /// The name of the strategy. Should match the file name of the result in the databrick aggregation
        /// for example: flex_consumption_df.json.snappy is matched by flex_consumption_df
        /// </summary>
        string FriendlyNameInstance { get; }

        /// <summary>
        /// How should the strategy dispatch?
        /// </summary>
        /// <param name="blobStream"></param>
        /// <param name="cancellationToken"></param>
        Task DispatchAsync(Stream blobStream, CancellationToken cancellationToken);
    }
}
