using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.Infrastructure.Messaging;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;

namespace Energinet.DataHub.Aggregations.Infrastructure.Middleware
{
    public class CorrelationIdMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ICorrelationContext _correlationContext;

        public CorrelationIdMiddleware(
            ICorrelationContext correlationContext)
        {
            _correlationContext = correlationContext;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var traceContext = TraceContext.Parse(context.TraceContext.TraceParent);

            _correlationContext.SetId(traceContext.TraceId);
            _correlationContext.SetParentId(traceContext.ParentId);

            await next(context).ConfigureAwait(false);
        }
    }
}
