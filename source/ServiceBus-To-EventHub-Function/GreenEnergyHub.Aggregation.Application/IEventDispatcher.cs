using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenEnergyHub.Aggregation.Application
{
    /// <summary>
    /// This dispatches events further down the pipe
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Dispatches the message
        /// </summary>
        /// <param name="msgData"></param>
        /// <param name="eventName">The name of the event</param>
        /// <returns>Task</returns>
        Task DispatchAsync(byte[] msgData, string eventName);
    }
}
