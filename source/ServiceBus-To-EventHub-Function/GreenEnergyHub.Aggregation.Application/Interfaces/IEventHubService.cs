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
    public interface IEventHubService
    {
        /// <summary>
        /// This sends a message onto the eventhub
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Task</returns>
        Task SendEventHubTestMessageAsync(string message);
    }
}
