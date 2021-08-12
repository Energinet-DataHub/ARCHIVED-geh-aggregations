using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GreenEnergyHub.Aggregation.Application.Utilities;
using Xunit;

namespace GreenEnergyHub.Aggregation.Tests
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
