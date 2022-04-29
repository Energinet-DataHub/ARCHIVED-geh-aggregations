// Copyright 2020 Energinet DataHub A/S
//
// Licensed under the Apache License, Version 2.0 (the "License2");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.IO;
using System.Linq;

namespace Energinet.DataHub.Aggregation.Coordinator.Tests
{
    public static class Paths
    {
        /// <summary>
        /// Given a relative sub path in the repository returns the absolute path.
        ///
        /// Implementation assumes that the current .NET solution is located somewhere beneath [repo]/source
        /// </summary>
        public static string GetAbsoluteRepoPath(string subPath)
        {
            string startupPath = AppDomain.CurrentDomain.BaseDirectory;
            var pathItems = startupPath.Split(Path.DirectorySeparatorChar);

            // Get repo root folder as parent of "source" folder
            // - we can not safely anticipate that is has the default name geh-aggregations
            var pos = pathItems.Reverse().ToList().FindIndex(x => string.Equals("source", x));
            string projectPath = string.Join(Path.DirectorySeparatorChar.ToString(), pathItems.Take(pathItems.Length - pos - 1));

            // Return sub path in repo
            return Path.Combine(projectPath, subPath);
        }
    }
}
