using System.Globalization;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Aggregations.Application.Extensions
{
    public static class NodatimeExtensions
    {
        /// <summary>
        /// Converts Instant to string in ISO8601 general format "yyyy-MM-ddTHH:mm:ssZ"
        /// </summary>
        /// <param name="instant"></param>
        /// <returns>String formated in ISO8601 general format</returns>
        public static string ToIso8601GeneralString(this Instant instant)
        {
            return instant.ToString(InstantPattern.General.PatternText, CultureInfo.InvariantCulture);
        }
    }
}
