using AutoFixture.Xunit2;

namespace Energinet.DataHub.Aggregations.Tests.Attributes
{
    public class InlineAutoMoqDataAttribute : InlineAutoDataAttribute
    {
        public InlineAutoMoqDataAttribute(params object[] objects)
            : base(new AutoMoqDataAttribute(), objects) { }
    }
}
