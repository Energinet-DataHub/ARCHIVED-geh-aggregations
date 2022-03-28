using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.MasterData
{
    /// <summary>
    /// This interface represents an object that can participate in the replay functionality
    /// </summary>
    public interface IReplayableObject
    {
        /// <summary>
        /// Valid from date
        /// </summary>
        Instant FromDate { get; set; }

        /// <summary>
        /// Valid to date
        /// </summary>
        Instant ToDate { get; set; }

        /// <summary>
        /// RowId of object
        /// </summary>
        public Guid RowId { get; set; }

        /// <summary>
        /// Creates a shallow copy / clone of this object
        /// </summary>
        /// <returns>a clone of the object</returns>
        T ShallowCopy<T>()
            where T : IReplayableObject;
    }
}
