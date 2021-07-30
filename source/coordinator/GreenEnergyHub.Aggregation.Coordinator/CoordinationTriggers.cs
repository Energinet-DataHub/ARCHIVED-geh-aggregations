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

using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Domain.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using NodaTime.Text;

namespace GreenEnergyHub.Aggregation.CoordinatorFunction
{
    public class CoordinationTriggers
    {
        private readonly ICoordinatorService _coordinatorService;

        public CoordinationTriggers(ICoordinatorService coordinatorService)
        {
            _coordinatorService = coordinatorService;
        }

        [OpenApiIgnore]
        [OpenApiOperation(operationId: "snapshotReceiver", Summary = "Receives Snapshot path", Visibility = OpenApiVisibilityType.Internal)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Something went wrong. Check the app insight logs.")]
        [FunctionName("SnapshotReceiver")]
        public static async Task<OkResult> SnapshotReceiverAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log)
        {
            log.LogInformation("We entered SnapshotReceiverAsync");
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            try
            {
                // Handle gzip replies
                var decompressedReqBody = await DecompressedReqBodyAsync(req).ConfigureAwait(false);

                var resultId = req.Headers["result-id"].First();

                log.LogInformation("We decompressed snapshot result and are ready to handle");
                log.LogInformation(decompressedReqBody);
            }
            catch (Exception e)
            {
                log.LogError(e, "A generic error occured in SnapshotReceiverAsync");
                throw;
            }

            return new OkResult();
        }

        [OpenApiOperation(operationId: "kickStartJob",  Summary = "Kickstarts the aggregation job", Description = "This will start up the databricks cluster if it is not running and then start a job", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "beginTime",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "Begin time",
            Description = "Start time of aggregation window for example 2020-01-01T00:00:00Z",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "endTime",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "End time in UTC",
            Description = "End Time of the aggregation window for example 2020-01-01T00:59:59Z",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "processType",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "Process type",
            Description = "For example D03 or D04",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "persist",
            In = ParameterLocation.Query,
            Required = false,
            Type = typeof(bool),
            Summary = "Should basis data be persisted?",
            Description = "If true the aggregation job will persist the basis data as a dataframe snapshot, defaults to false",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "resolution",
            In = ParameterLocation.Query,
            Required = false,
            Type = typeof(string),
            Summary = "Window resolution",
            Description = "For example 15 minutes or 60 minutes",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description="When the job was started in the background correctly")]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description="Something went wrong. Check the app insight logs")]
        [FunctionName("KickStartJob")]
        public IActionResult KickStartJob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var beginTime = InstantPattern.General.Parse(req.Query["beginTime"]).GetValueOrThrow();
            var endTime = InstantPattern.General.Parse(req.Query["endTime"]).GetValueOrThrow();

            string processType = req.Query["processType"];
            string resolution = req.Query["resolution"];

            if (!bool.TryParse(req.Query["persist"], out var persist))
            {
                throw new ArgumentException($"Could not parse value {nameof(persist)}");
            }

            if (processType == null)
            {
                return new BadRequestResult();
            }

            if (string.IsNullOrWhiteSpace(resolution))
            {
                resolution = "60 minutes";
            }

            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
            #pragma warning disable CS4014
            _coordinatorService.StartAggregationJobAsync(processType, beginTime, endTime, Guid.NewGuid().ToString(), persist, resolution, cancellationToken).ConfigureAwait(false);
            #pragma warning restore CS4014

            log.LogInformation("We kickstarted the aggregation job");
            return new OkResult();
        }

        [OpenApiOperation(operationId: "kickStartWholesaleJob",  Summary = "Kickstarts the wholesale job", Description = "This will start up the databricks cluster if it is not running and then start a job", Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "beginTime",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "Begin time",
            Description = "Start time of wholesale window for example 2020-01-01T00:00:00Z",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "endTime",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "End time in UTC",
            Description = "End Time of the wholesale window for example 2020-01-01T00:59:59Z",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "processType",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "Process type",
            Description = "For example D05 or D32",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "processVariant",
            In = ParameterLocation.Query,
            Required = true,
            Type = typeof(string),
            Summary = "Process variant",
            Description = "For example D01, D02, or D03",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiParameter(
            "persist",
            In = ParameterLocation.Query,
            Required = false,
            Type = typeof(bool),
            Summary = "Should basis data be persisted?",
            Description = "If true the wholesale job will persist the basis data as a dataframe snapshot, defaults to false",
            Visibility = OpenApiVisibilityType.Important)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK, Description="When the job was started in the background correctly")]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description="Something went wrong. Check the app insight logs")]
        [FunctionName("KickStartWholesaleJob")]
        public IActionResult KickStartWholesaleJob(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequest req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var beginTime = InstantPattern.General.Parse(req.Query["beginTime"]).GetValueOrThrow();
            var endTime = InstantPattern.General.Parse(req.Query["endTime"]).GetValueOrThrow();

            string processType = req.Query["processType"];

            if (processType == null)
            {
                return new BadRequestResult();
            }

            string processVariant = req.Query["processVariant"];

            if (processVariant == null)
            {
                return new BadRequestResult();
            }

            if (!bool.TryParse(req.Query["persist"], out var persist))
            {
                throw new ArgumentException($"Could not parse value {nameof(persist)}");
            }

            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
            #pragma warning disable CS4014

            // TODO: #199 add processVariant to StartWholesaleJobAsync and store it as metadata
            _coordinatorService.StartWholesaleJobAsync(processType, beginTime, endTime, persist, cancellationToken).ConfigureAwait(false);
            #pragma warning restore CS4014

            log.LogInformation("We kickstarted the wholesale job");
            return new OkResult();
        }

        [OpenApiIgnore]
        [OpenApiOperation(operationId: "resultReceiver", Summary = "Receives Result path", Visibility = OpenApiVisibilityType.Internal)]
        [OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        [OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Something went wrong. Check the app insight logs")]

        [FunctionName("ResultReceiver")]
        public async Task<OkResult> ResultReceiverAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)]
            HttpRequest req,
            ILogger log,
            CancellationToken cancellationToken)
        {
            log.LogInformation("We entered ResultReceiverAsync");
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            try
            {
                // Handle gzip replies
                var decompressedReqBody = await DecompressedReqBodyAsync(req).ConfigureAwait(false);

                // Validate request headers contain expected keys
                ValidateRequestHeaders(req.Headers);

                var resultId = req.Headers["result-id"].First();
                var processType = req.Headers["process-type"].First();
                var reqStartTime = req.Headers["start-time"].First();
                var reqEndTime = req.Headers["end-time"].First();

                var startTime = InstantPattern.General.Parse(reqStartTime).GetValueOrThrow();
                var endTime = InstantPattern.General.Parse(reqEndTime).GetValueOrThrow();

                log.LogInformation("We decompressed result and are ready to handle");

                // Because this call does not need to be awaited, execution of the current method
                // continues and we can return the result to the caller immediately
                #pragma warning disable CS4014
                _coordinatorService.HandleResultAsync(decompressedReqBody, resultId, processType, startTime, endTime, cancellationToken).ConfigureAwait(false);
                #pragma warning restore CS4014
            }
            catch (Exception e)
            {
                log.LogError(e, "A generic error occured in ResultReceiverAsync");
                throw;
            }

            return new OkResult();
        }

        private static async Task<string> DecompressedReqBodyAsync(HttpRequest req)
        {
            string decompressedReqBody;
            if (req.Headers.ContainsKey("Content-Encoding") && req.Headers["Content-Encoding"].Contains("gzip"))
            {
                await using var decompressionStream = new GZipStream(req.Body, CompressionMode.Decompress);
                using var sr = new StreamReader(decompressionStream, Encoding.UTF8);
                decompressedReqBody = await sr.ReadToEndAsync().ConfigureAwait(false);
            }
            else
            {
                using var sr = new StreamReader(req.Body);
                decompressedReqBody = await sr.ReadToEndAsync().ConfigureAwait(false);
            }

            return decompressedReqBody;
        }

        private static void ValidateRequestHeaders(IHeaderDictionary reqHeaders)
        {
            if (!reqHeaders.ContainsKey("result-id"))
            {
                throw new ArgumentException("Header {result-id} missing");
            }

            if (!reqHeaders.ContainsKey("process-type"))
            {
                throw new ArgumentException("Header {process-type} missing");
            }

            if (!reqHeaders.ContainsKey("start-time"))
            {
                throw new ArgumentException("Header {start-time} missing");
            }

            if (!reqHeaders.ContainsKey("end-time"))
            {
                throw new ArgumentException("Header {end-time} missing");
            }
        }
    }
}
