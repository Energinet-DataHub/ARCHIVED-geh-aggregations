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

namespace GreenEnergyHub.Aggregation.Application.Utilities
{
    /// <summary>
    /// this class takes care of parsing the input path coming from the databricks job
    /// </summary>
    public static class InputStringParser
    {
        public static string ParseJobPath(string path)
        {
            // source is like {0}/{1}/{2}.json.snappy
            var folderSplit = path.Split('/')[2];
            var pathSplit = folderSplit.Split('.')[0];
            return pathSplit;
        }
    }
}
