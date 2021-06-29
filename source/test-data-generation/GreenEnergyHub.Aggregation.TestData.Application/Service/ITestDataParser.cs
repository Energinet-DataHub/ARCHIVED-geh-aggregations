using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GreenEnergyHub.Aggregation.TestData.Application.Service
{
    /// <summary>
    /// Provides the interface for code that handles a particular test data source file
    /// </summary>
    public interface ITestDataParser
    {
        /// <summary>
        /// Lets the generator service know what filename this parser can handle
        /// </summary>
        string FileNameICanHandle { get; set; }

        /// <summary>
        /// The main code handling the parsing of the filename I can handle
        /// </summary>
        /// <param name="stream"></param>
        public void Parse(Stream stream);
    }
}
