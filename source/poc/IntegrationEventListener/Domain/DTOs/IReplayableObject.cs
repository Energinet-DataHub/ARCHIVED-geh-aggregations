using System;

namespace Domain.DTOs
{
    /// <summary>
    /// This interface represents an object that can participate in the replay functionality
    /// </summary>
    public interface IReplayableObject
    {
        /// <summary>
        /// Valid from date
        /// </summary>
        DateTime FromDate { get; set; }

        /// <summary>
        /// Valid to date
        /// </summary>
        DateTime ToDate { get; set; }

        /// <summary>
        /// RowId of object
        /// </summary>
        public Guid RowId { get; set; }

        /// <summary>
        /// Creates a shallow copy / clone of this object
        /// </summary>
        /// <returns>a clone of the object</returns>
        IReplayableObject ShallowCopy();
    }
}
