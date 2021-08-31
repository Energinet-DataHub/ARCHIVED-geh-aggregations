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
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
//using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.Extensions.Logging;
using NodaTime;
//using Microsoft.OpenApi.Models;
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

        //[OpenApiIgnore]
        //[OpenApiOperation(operationId: "snapshotReceiver", Summary = "Receives Snapshot path", Visibility = OpenApiVisibilityType.Internal)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Something went wrong. Check the app insight logs.")]
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

            var errors = GetJobDataFromQueryString(
                req,
                out var beginTime,
                out var endTime,
                out var jobType,
                out var jobOwnerString,
                out var persist,
                out var resolution,
                out var gridArea);

            var processVariantString = ParseProcessVariantString(req, errors);

            //TODO this might need to be an enum too
            var processVariant = processVariantString;

            if (errors.Any())
            {
                return await JsonResultAsync(req, errors).ConfigureAwait(false);
            }

            var jobId = Guid.NewGuid();
            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
#pragma warning disable CS4014

            _coordinatorService.StartDataPreparationJobAsync(jobId, jobType, jobOwnerString, beginTime, endTime, persist, resolution, gridArea, processVariant, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the wholesale job");
            return await JsonResultAsync(req, new { JobId = jobId, errors }).ConfigureAwait(false);
        }

        //[OpenApiOperation(operationId: "kickStartJob", Summary = "Kickstarts the aggregation job", Description = "This will start up the databricks cluster if it is not running and then start a job", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(
        //    "beginTime",
        //    In = ParameterLocation.Query,
        //    Required = true,
        //    Type = typeof(string),
        //    Summary = "Begin time",
        //    Description = "Start time of aggregation window for example 2020-01-01T00:00:00Z",
        //    Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(
        //    "endTime",
        //    In = ParameterLocation.Query,
        //    Required = true,
        //    Type = typeof(string),
        //    Summary = "End time in UTC",
        //    Description = "End Time of the aggregation window for example 2020-01-01T00:59:59Z",
        //    Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(
        //    "processType",
        //    In = ParameterLocation.Query,
        //    Required = true,
        //    Type = typeof(string),
        //    Summary = "Process type",
        //    Description = "For example D03 or D04",
        //    Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(
        //    "persist",
        //    In = ParameterLocation.Query,
        //    Required = false,
        //    Type = typeof(bool),
        //    Summary = "Should basis data be persisted?",
        //    Description = "If true the aggregation job will persist the basis data as a dataframe snapshot, defaults to false",
        //    Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(
        //    "resolution",
        //    In = ParameterLocation.Query,
        //    Required = false,
        //    Type = typeof(string),
        //    Summary = "Window resolution",
        //    Description = "For example 15 minutes or 60 minutes",
        //    Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiResponseWithoutBody(HttpStatusCode.OK, Description = "When the job was started in the background correctly")]
        //[OpenApiResponseWithoutBody(HttpStatusCode.InternalServerError, Description = "Something went wrong. Check the app insight logs")]
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

            var errors = GetJobDataFromQueryString(
                req,
                out var beginTime,
                out var endTime,
                out var jobType,
                out var jobOwnerString,
                out var persist,
                out var resolution,
                out var gridArea);
            var jobId = Guid.NewGuid();

            if (errors.Any())
            {
                return await JsonResultAsync(req, errors).ConfigureAwait(false);
            }

            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
#pragma warning disable CS4014
            _coordinatorService.StartAggregationJobAsync(jobId, jobType, jobOwnerString, beginTime, endTime, persist, resolution, gridArea, CancellationToken.None).ConfigureAwait(false);
#pragma warning restore CS4014

            log.LogInformation("We kickstarted the aggregation job");
            return await JsonResultAsync(req, new { JobId = jobId }).ConfigureAwait(false);
        }

        //[OpenApiOperation(operationId: "kickStartWholesaleJob", Summary = "Kickstarts the wholesale job", Description = "This will start up the databricks cluster if it is not running and then start a job", Visibility = OpenApiVisibilityType.Important)]
        //[OpenApiParameter(
        //    "beginTime",
        //    In = ParameterLocation.Query,
        //    Required = true,
        //    Type = typeof(string),
        //    Summary = "Begin time",
        //    Description = "Start time of wholesale window for example 2020-01-01T00:00:00Z",
        //    Visibility = OpenApiVisibilityTy.Important)]
        //[OpenApiParameter(
        //    "endTime",
        //    In = ParameterLocation.Query,
        //    Required = true,
        //    Type = typeof(string),
        //    Summary = "End time in UTC",
        //    Description = "End Time ofhe wholesale window for exame 2020-01-01T00:59:59Z",
        //  Visibility = OpenApiVisibilityTypImportant)]
        //[OpenApiPameter(
        //    "processType",
        //   In = ParameterLocation.Que,
        //    Required = true,
        //    Type = typeof(string),
        //    Summary = "Press type",
        //    Description = "For example D05 or D3,
        //    Visibility = enApiVisibilityType.Impoant)]
        //[OpenApiParameter(
        //  "processVariant",
        //  In = ParameterLocation.Query,
        //  Required = true,
        //    Type = ypeof(string),
        //    Summary = "Process variant",
        //    Description = "For example01, D02, or D03",
        //    Visibility = OpenApiVisibilitype.Important)]
        //[OpApiParameter(
        //    "rsist",
        //    In = ParameterLocationuery,
        //    Required = lse,
        //    Type = typeof(bool
        //    Summary = "Should basis da be persisted?",
        //    Description = "If truehe wholesale job will persist the basis data as a dataframe apshot, defaults to false",
        //   Visibility = OpenApisibilityType.Important)]
        //[OpenApiRponseWithoutBody(HttpStatusCodOK, Description = "When the job was arted in the background correctly")]
        //[OpenApiResponseWithoutBody(HttpStatusCode.InternalServerror, Description = "Something went wrong. Check the app insit logs")]
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

            var errors = GetJobDataFromQueryString(
                req,
                out var beginTime,
                out var endTime,
                out var jobType,
                out var jobOwnerString,
                out var persist,
                out var resolution,
                out var gridArea);

            var processVariantString = ParseProcessVariantString(req, errors);

            //TODO this might need to be an enum too
            var processVariant = processVariantString;

            if (errors.Any())
            {
                return await JsonResultAsync(req, errors).ConfigureAwait(false);
            }

            var jobId = Guid.NewGuid();
            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
#pragma warning disable CS4014

            _coordinatorService.StartWholesaleJobAsync(jobId, jobType, jobOwnerString, beginTime, endTime, persist, resolution, gridArea, processVariant, CancellationToken.None).ConfigureAwait(false);
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

        private static string ParseProcessVariantString(HttpRequestData req, List<string> errors)
        {
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            string processVariantString = queryDictionary["processVariant"];

            if (processVariantString == null)
            {
                errors.Add("no processVariant specified");
            }

            return processVariantString;
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

        private static List<string> GetJobDataFromQueryString(HttpRequestData req, out Instant beginTime, out Instant endTime, out JobTypeEnum jobType, out string jobOwnerString, out bool persist, out string resolution, out string gridArea)
        {
            var errorList = new List<string>();
            var queryDictionary = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(req.Url.Query);

            if (!InstantPattern.General.Parse(queryDictionary["beginTime"]).TryGetValue(Instant.MinValue, out beginTime))
            {
                errorList.Add("Could not parse beginTime correctly");
            }

            if (!InstantPattern.General.Parse(queryDictionary["endTime"]).TryGetValue(Instant.MinValue, out endTime))
            {
                errorList.Add("Could not parse endTime correctly");
            }

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

            if (!bool.TryParse(queryDictionary["persist"], out persist))
            {
                errorList.Add($"Could not parse value {nameof(persist)}");
            }

            string resolutionString = queryDictionary["resolution"];
            if (string.IsNullOrWhiteSpace(resolutionString))
            {
                resolutionString = "60 minutes";
            }

            resolution = resolutionString;

            if (!queryDictionary.ContainsKey("gridArea"))
            {
                errorList.Add($"gridArea should be present as key but can be empty");
            }

            gridArea = queryDictionary["gridArea"];

            return errorList;
        }
    }
}
