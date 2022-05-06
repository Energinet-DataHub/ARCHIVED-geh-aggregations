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
using Energinet.DataHub.Aggregation.Coordinator.Application.Coordinator;
using Energinet.DataHub.Aggregation.Coordinator.Domain.DTOs.Metadata.Enums;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using NodaTime;
using NodaTime.Text;

namespace Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction
{
    public class CoordinationTriggers
    {
        private readonly ICoordinatorService _coordinatorService;

        public CoordinationTriggers(ICoordinatorService coordinatorService)
        {
            _coordinatorService = coordinatorService;
        }

        [Function(CoordinatorFunctionNames.SnapshotReceiver)]
        public async Task<HttpResponseData> SnapshotReceiverAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
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

                var queryDictionary = req.Headers.ToDictionary(h => h.Key, h => h.Value.First());

                if (!queryDictionary.ContainsKey("snapshot-id"))
                {
                    throw new ArgumentException("Header {snapshot-id} missing");
                }

                var snapshotId = new Guid(queryDictionary["snapshot-id"]);
#pragma warning disable CS4014
                _coordinatorService.UpdateSnapshotPathAsync(snapshotId, decompressedReqBody);
#pragma warning restore CS4014
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

        [Function(CoordinatorFunctionNames.PreparationJob)]
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
            _coordinatorService.StartDataPreparationJobAsync(jobId, snapshotId, fromDate, toDate, gridAreas, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the wholesale job");
            return await JsonResultAsync(req, new { SnapshotId = snapshotId, JobId = jobId, errors }).ConfigureAwait(false);
        }

        [Function(CoordinatorFunctionNames.AggregationJob)]
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
                out var processType,
                out var isSimulation,
                out var owner,
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
            _coordinatorService.StartAggregationJobAsync(jobId, snapshotId, processType, isSimulation, owner, resolution, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the aggregation job");
            return await JsonResultAsync(req, new { JobId = jobId }).ConfigureAwait(false);
        }

        [Function(CoordinatorFunctionNames.WholesaleJob)]
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
                out var processType,
                out var isSimulation,
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

            _coordinatorService.StartWholesaleJobAsync(jobId, snapshotId, processType, isSimulation, jobOwnerString, processVariant, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the wholesale job");
            return await JsonResultAsync(req, new { JobId = jobId, errors }).ConfigureAwait(false);
        }

        [Function(CoordinatorFunctionNames.ResultReceiver)]
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
                ParseAndValidateResultReceiverHeaders(req, out var jobId);

                // var job = await _coordinatorService.GetJobAsync(jobId).ConfigureAwait(false);
                log.LogInformation("We decompressed result and are ready to handle");
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

        private static void ParseAndValidateResultReceiverHeaders(HttpRequestData req, out string jobId)
        {
            var queryDictionary = req.Headers.ToDictionary(h => h.Key, h => h.Value.First());

            if (!queryDictionary.ContainsKey("job-id"))
            {
                throw new ArgumentException("Header {job-id} missing");
            }

            jobId = queryDictionary["job-id"];
        }

        private static List<string> GetSnapshotDataFromQueryString(HttpRequestData req, out Instant fromDate, out Instant toDate, out string gridAreas)
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

            if (!queryDictionary.ContainsKey("gridAreas"))
            {
                errorList.Add($"gridAreas should be present as key but can be empty");
            }

            gridAreas = queryDictionary["gridAreas"];

            return errorList;
        }

        private static List<string> GetAggregationDataFromQueryString(HttpRequestData req, out JobProcessTypeEnum processType, out bool isSimulation, out string owner, out ResolutionEnum resolution, out Guid snapshotId)
        {
            var errorList = new List<string>();
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            string processTypeString = queryDictionary["processType"];

            if (!Enum.TryParse(processTypeString, out processType))
            {
                errorList.Add($"Could not parse processType {processTypeString} to JobProcessTypeEnum");
            }

            string isSimulationString = queryDictionary["isSimulation"];
            if (!bool.TryParse(isSimulationString, out isSimulation))
            {
                errorList.Add("isSimulation is not a boolean");
            }

            owner = queryDictionary["owner"];

            if (owner == null)
            {
                errorList.Add("no owner specified");
            }

            string resolutionString = queryDictionary["resolution"];
            if (!Enum.TryParse(resolutionString, out resolution))
            {
                errorList.Add("Could not parse resolution");
            }

            string snapshotIdString = queryDictionary["snapshotId"];

            if (!Guid.TryParse(snapshotIdString, out snapshotId))
            {
                errorList.Add("no snapshotId specified");
            }

            return errorList;
        }

        private static List<string> GetWholesaleDataFromQueryString(HttpRequestData req, out JobProcessTypeEnum processType, out bool isSimulation, out string ownerString, out JobProcessVariantEnum processVariant, out Guid snapshotId)
        {
            var errorList = new List<string>();
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            string processTypeString = queryDictionary["processType"];

            if (!Enum.TryParse(processTypeString, out processType))
            {
                errorList.Add("Could not parse processType to enum value");
            }

            string isSimulationString = queryDictionary["isSimulation"];
            if (!bool.TryParse(isSimulationString, out isSimulation))
            {
                errorList.Add("Could not parse isSimulation to a boolean");
            }

            ownerString = queryDictionary["owner"];

            if (string.IsNullOrWhiteSpace(ownerString))
            {
                errorList.Add("No owner specified");
            }

            var processVariantString = queryDictionary["processVariant"];
            if (!Enum.TryParse(processVariantString, out processVariant))
            {
                errorList.Add($"Could not parse processVariant to enum value");
            }

            string snapshotIdString = queryDictionary["snapshotId"];

            if (!Guid.TryParse(snapshotIdString, out snapshotId))
            {
                errorList.Add("no snapshotId specified");
            }

            return errorList;
        }
    }
}
