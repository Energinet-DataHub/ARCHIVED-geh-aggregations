using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace GreenEnergyHub.Aggregation.TestData.Application.Service
{
    /// <summary>
    /// This provides an interface for the generator Service
    /// </summary>
    public interface IGeneratorService
    {
        /// <summary>
        /// The initial function for handling and dispatching incoming changes
        /// </summary>
        /// <param name="myblob"></param>
        /// <param name="name"></param>
        Task HandleChangedFileAsync(Stream myblob, string name);
    }
}
