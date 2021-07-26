using System;
using System.Globalization;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.Extensions
{
    public static class StringExtensions
    {
        public static string ToISO8601DateTimeString(this string datetime)
        {
            var raw = DateTime.Parse(datetime, CultureInfo.InvariantCulture);

            var instant = raw.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture);
            return instant;
        }
    }
}
