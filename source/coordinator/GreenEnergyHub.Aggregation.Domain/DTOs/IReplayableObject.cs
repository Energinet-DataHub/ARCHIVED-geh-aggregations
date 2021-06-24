using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs
{
    /// <summary>
    /// This interface represents an object that can participate in the replay functionality
    /// </summary>
    public interface IReplayableObject
    {
        /// <summary>
        /// Valid from date
        /// </summary>
        Instant ValidFrom { get; set; }

        /// <summary>
        /// Valid to date
        /// </summary>
        Instant ValidTo { get; set; }

        /// <summary>
        /// Creates a shallow copy / clone of this object
        /// </summary>
        /// <returns>a clone of the object</returns>
        IReplayableObject ShallowCopy();
    }
}
