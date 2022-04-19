using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;

namespace Energinet.DataHub.Aggregations.Infrastructure.Middleware
{
    public class FunctionInvocationLoggingMiddleware : IFunctionsWorkerMiddleware
    {
        private readonly ILoggerFactory _loggerFactory;

        public FunctionInvocationLoggingMiddleware([NotNull] ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
        {
            var functionEndpointName = context.FunctionDefinition.Name;
            var logger = _loggerFactory.CreateLogger(functionEndpointName);

            logger.LogInformation("Function {FunctionName} started to process a request with invocation ID {InvocationId}", functionEndpointName, context.InvocationId);
            await next(context).ConfigureAwait(false);
            logger.LogInformation("Function {FunctionName} ended to process a request with invocation ID {InvocationId}", functionEndpointName, context.InvocationId);
        }
    }
}
