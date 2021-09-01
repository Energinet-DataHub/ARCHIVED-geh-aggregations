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
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NodaTime;
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

        [Function("SnapshotReceiver")]
        public static async Task<HttpResponseData> SnapshotReceiverAsync(
            [HttpTrigger(AuthorizationLevel.Function, "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(SnapshotReceiverAsync));
            log.LogInformation("We entered SnapshotReceiverAsync");
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            try
            {
                // Handle gzip replies
                var decompressedReqBody = await DecompressedReqBodyAsync(req).ConfigureAwait(false);

                log.LogInformation("We decompressed snapshot result and are ready to handle");
                log.LogInformation(decompressedReqBody);
            }
            catch (Exception e)
            {
                log.LogError(e, "A generic error occured in SnapshotReceiverAsync");
                throw;
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        [Function("KickStartDataPreparationJobAsync")]
        public async Task<HttpResponseData> KickStartDataPreparationJobAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequestData req,
            FunctionContext context)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var log = context.GetLogger(nameof(KickStartDataPreparationJobAsync));

            var errors = GetSnapshotDataFromQueryString(
                req,
                out var fromDate,
                out var toDate,
                out var createdDate,
                out var gridAreas);

            if (errors.Any())
            {
                return await JsonResultAsync(req, errors).ConfigureAwait(false);
            }

            var jobId = Guid.NewGuid();
            var snapshotId = Guid.NewGuid();
            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
#pragma warning disable CS4014

            _coordinatorService.StartDataPreparationJobAsync(snapshotId, jobId, fromDate, toDate, createdDate, gridAreas, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the wholesale job");
            return await JsonResultAsync(req, new { SnapshotId = snapshotId, errors }).ConfigureAwait(false);
        }

        [Function("KickStartJob")]
        public async Task<HttpResponseData> KickStartJobAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequestData req,
            FunctionContext context)
        {
            var log = context.GetLogger(nameof(KickStartJobAsync));

            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var errors = GetAggregationDataFromQueryString(
                req,
                out var jobType,
                out var jobOwnerString,
                out var resolution,
                out var snapshotId);
            var jobId = Guid.NewGuid();

            if (errors.Any())
            {
                return await JsonResultAsync(req, errors).ConfigureAwait(false);
            }

            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
#pragma warning disable CS4014
            _coordinatorService.StartAggregationJobAsync(jobId, snapshotId, jobType, jobOwnerString, resolution, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the aggregation job");
            return await JsonResultAsync(req, new { JobId = jobId }).ConfigureAwait(false);
        }

        [Function("KickStartWholesaleJob")]
        public async Task<HttpResponseData> KickStartWholesaleJobAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]
            HttpRequestData req,
            FunctionContext context)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var log = context.GetLogger(nameof(KickStartWholesaleJobAsync));

            var errors = GetWholesaleDataFromQueryString(
                req,
                out var jobType,
                out var jobOwnerString,
                out var processVariant,
                out var snapshotId);

            if (errors.Any())
            {
                return await JsonResultAsync(req, errors).ConfigureAwait(false);
            }

            var jobId = Guid.NewGuid();
            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
#pragma warning disable CS4014

            _coordinatorService.StartWholesaleJobAsync(jobId, snapshotId, jobType, jobOwnerString, processVariant, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the wholesale job");
            return await JsonResultAsync(req, new { JobId = jobId, errors }).ConfigureAwait(false);
        }

        //[OpenApiIgnore]
        //[OpenApiOperation(operationId: "resultReceiver", Summary = "Receives Result path", Visibility = OpenApiVisibilityType.Internal)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Something went wrong. Check the app insight logs")]
        [Function("ResultReceiver")]
        public async Task<HttpResponseData> ResultReceiverAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous,  "post")]
            HttpRequestData req,
            FunctionContext context)
        {
            if (req is null)
            {
                throw new ArgumentNullException(nameof(req));
            }

            var log = context.GetLogger(nameof(ResultReceiverAsync));
            log.LogInformation("We entered ResultReceiverAsync");

            try
            {
                // Handle gzip replies
                var decompressedReqBody = await DecompressedReqBodyAsync(req).ConfigureAwait(false);

                // Validate request headers contain expected keys
                ParseAndValidateResultReceiverHeaders(req, out var resultId, out var processType, out var reqStartTime, out var reqEndTime);

                var startTime = InstantPattern.General.Parse(reqStartTime).GetValueOrThrow();
                var endTime = InstantPattern.General.Parse(reqEndTime).GetValueOrThrow();

                log.LogInformation("We decompressed result and are ready to handle");

                // Because this call does not need to be awaited, execution of the current method
                // continues and we can return the result to the caller immediately
#pragma warning disable CS4014
                _coordinatorService.HandleResultAsync(decompressedReqBody, resultId, processType, startTime, endTime, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014
            }
            catch (Exception e)
            {
                log.LogError(e, "A generic error occured in ResultReceiverAsync");
                throw;
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private static async Task<HttpResponseData> JsonResultAsync(HttpRequestData req, object obj)
        {
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(obj).ConfigureAwait(false);
            return response;
        }

        private static async Task<string> DecompressedReqBodyAsync(HttpRequestData req)
        {
            string decompressedReqBody;
            if (req.Headers.Contains("Content-Encoding") && req.Headers.GetValues("Content-Encoding").Contains("gzip"))
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

        private static void ParseAndValidateResultReceiverHeaders(HttpRequestData req, out string resultId, out string processType, out string reqStartTime, out string reqEndTime)
        {
            var queryDictionary = req.Headers.ToDictionary(h => h.Key, h => h.Value.First());

            if (!queryDictionary.ContainsKey("result-id"))
            {
                throw new ArgumentException("Header {result-id} missing");
            }

            resultId = queryDictionary["result-id"];

            if (!queryDictionary.ContainsKey("process-type"))
            {
                throw new ArgumentException("Header {process-type} missing");
            }

            processType = queryDictionary["process-type"];

            if (!queryDictionary.ContainsKey("start-time"))
            {
                throw new ArgumentException("Header {start-time} missing");
            }

            reqStartTime = queryDictionary["start-time"];

            if (!queryDictionary.ContainsKey("end-time"))
            {
                throw new ArgumentException("Header {end-time} missing");
            }

            reqEndTime = queryDictionary["end-time"];
        }

        private static List<string> GetSnapshotDataFromQueryString(HttpRequestData req, out Instant fromDate, out Instant toDate, out Instant createdDate, out string gridAreas)
        {
            var errorList = new List<string>();
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            if (!InstantPattern.General.Parse(queryDictionary["fromDate"]).TryGetValue(Instant.MinValue, out fromDate))
            {
                errorList.Add("Could not parse fromDate correctly");
            }

            if (!InstantPattern.General.Parse(queryDictionary["toDate"]).TryGetValue(Instant.MinValue, out toDate))
            {
                errorList.Add("Could not parse toDate correctly");
            }

            if (!InstantPattern.General.Parse(queryDictionary["createdDate"]).TryGetValue(Instant.MinValue, out createdDate))
            {
                errorList.Add("Could not parse createdDate correctly");
            }

            if (!queryDictionary.ContainsKey("gridAreas"))
            {
                errorList.Add($"gridAreas should be present as key but can be empty");
            }

            gridAreas = queryDictionary["gridAreas"];

            return errorList;
        }

        private static List<string> GetAggregationDataFromQueryString(HttpRequestData req, out JobTypeEnum jobType, out string jobOwnerString, out string resolution, out Guid snapshotId)
        {
            var errorList = new List<string>();
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            string jobTypeString = queryDictionary["jobType"];

            if (jobTypeString == null)
            {
                errorList.Add("no jobType specified");
            }

            if (!Enum.TryParse(jobTypeString, out jobType))
            {
                errorList.Add($"Could not parse jobType {jobTypeString} to JobTypeEnum");
            }

            jobOwnerString = queryDictionary["jobOwner"];

            if (jobOwnerString == null)
            {
                errorList.Add("no jobOwner specified");
            }

            string resolutionString = queryDictionary["resolution"];
            if (string.IsNullOrWhiteSpace(resolutionString))
            {
                resolutionString = "60 minutes";
            }

            resolution = resolutionString;

            snapshotId = new Guid(queryDictionary["snapshotId"]);

            return errorList;
        }

        private static List<string> GetWholesaleDataFromQueryString(HttpRequestData req, out JobTypeEnum jobType, out string jobOwnerString, out string processVariant, out Guid snapshotId)
        {
            var errorList = new List<string>();
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            string jobTypeString = queryDictionary["jobType"];

            if (jobTypeString == null)
            {
                errorList.Add("no jobType specified");
            }

            if (!Enum.TryParse(jobTypeString, out jobType))
            {
                errorList.Add($"Could not parse jobType {jobTypeString} to JobTypeEnum");
            }

            jobOwnerString = queryDictionary["jobOwner"];

            if (jobOwnerString == null)
            {
                errorList.Add("no jobOwner specified");
            }

            processVariant = queryDictionary["processVariant"];

            snapshotId = new Guid(queryDictionary["snapshotId"]);

            return errorList;
        }
    }
}
