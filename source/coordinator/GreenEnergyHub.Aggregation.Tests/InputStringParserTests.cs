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

using GreenEnergyHub.Aggregation.Application.Utilities;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
{
    [Trait("Category", "Component")]
    public class InputStringParserTests
    {
        [Fact]
        public void Check_Correct_Parsing_Test()
        {
            var path = "Result/2021-04-23_11-39-39/job-id/added_grid_loss_df";
            var result = InputStringParser.ParseJobPath(path);
            Assert.Equal("added_grid_loss_df", result);
        }
    }
}
