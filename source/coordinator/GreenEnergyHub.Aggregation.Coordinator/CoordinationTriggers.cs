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
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Domain.Types;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.CoordinatorFunction
{
    public class CoordinationTriggers
    {
        private readonly ICoordinatorService _coordinatorService;

        public CoordinationTriggers(ICoordinatorService coordinatorService)
        {
            _coordinatorService = coordinatorService;
        }

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

            string beginTime = req.Query["beginTime"];
            string endTime = req.Query["endTime"];
            string processTypeString = req.Query["processType"];

            if (beginTime == null || endTime == null || processTypeString == null)
            {
                return new BadRequestResult();
            }

            if (!Enum.TryParse(processTypeString, out ProcessType processType))
            {
                throw new Exception($"Could not parse process type: {processTypeString} in {nameof(CoordinationTriggers)}");
            }

            // Because this call does not need to be awaited, execution of the current method
            // continues and we can return the result to the caller immediately
            #pragma warning disable CS4014
            _coordinatorService.StartAggregationJobAsync(processType, beginTime, endTime, Guid.NewGuid().ToString(), cancellationToken).ConfigureAwait(false);
            #pragma warning restore CS4014

            log.LogInformation("We kickstarted the job");
            return new OkResult();
        }

        [FunctionName("ResultReceiver")]
        public async Task<OkResult> ResultReceiverAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
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

                var resultId = req.Headers["result-id"].FirstOrDefault();
                var processType = req.Headers["process-type"].FirstOrDefault();
                var startTime = req.Headers["start-time"].FirstOrDefault();
                var endTime = req.Headers["end-time"].FirstOrDefault();

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
    }
}
