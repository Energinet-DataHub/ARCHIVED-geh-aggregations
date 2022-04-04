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

using System.ComponentModel;
using Energinet.DataHub.Aggregation.Coordinator.Application.Utilities;
using Xunit;

namespace Energinet.DataHub.Aggregation.Coordinator.Tests
{
    public enum TestEnum
    {
        [Description("zero")]
        Zero = 0,
        [Description("one")]
        One = 1,
    }

    [Trait("Category", "Component")]
    public class EnumExtensionTests
    {
        [Fact]
        public void Check_getting_description_from_enum()
        {
            var first = TestEnum.Zero;
            Assert.Equal("zero", first.GetDescription());
            Assert.NotEqual("one", first.GetDescription());
        }
    }
}
