using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Application.Interfaces
{
    /// <summary>
    /// IIntegrationEvent
    /// </summary>
    public interface IIntegrationEvent
    {
        /// <summary>
        /// MeteringPointId
        /// </summary>
        string MeteringPointId { get; }

        /// <summary>
        /// EffectiveDate
        /// </summary>
        Instant EffectiveDate { get; init; }
    }
}
