using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GreenEnergyHub.Aggregation.Application.Interfaces
{
    /// <summary>
    /// This provides an interface for the event hub
    /// </summary>
    public interface IEventHubService : IDisposable
    {
        /// <summary>
        /// This sends a message onto the eventhub
        /// </summary>
        /// <param name="msg"></param>
        /// <returns>Task</returns>
        Task SendEventHubMessageAsync(byte[] msg);
    }
}
