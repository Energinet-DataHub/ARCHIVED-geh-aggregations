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
using System.Linq;
using System.Threading.Tasks;
using DAL;
using Domain.DTOs;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace IntegrationEventListener
{
    public class MeteringPointEventListenerFunction
    {
        private readonly IMeteringPointRepository _meteringPointRepository;

        public MeteringPointEventListenerFunction(IMeteringPointRepository meteringPointRepository)
        {
            _meteringPointRepository = meteringPointRepository;
        }

        [FunctionName("Create")]
        public async Task<IActionResult> Create(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MeteringPointCreatedEvent>(requestBody);
            var list = data.GetObjectsAfterMutate(new List<IReplayableObject>(), data.EffectiveDate).Select(x => (MeteringPoint)x).ToList();
            await _meteringPointRepository.AddOrUpdateMeteringPoints(list);

            return new OkResult();
        }

        [FunctionName("Connect")]
        public async Task<IActionResult> Connect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MeteringPointConnectedEvent>(requestBody);
            var existingMeteringPoints = await _meteringPointRepository.GetByIdAndDateAsync(data.Id, data.EffectiveDate)
                .ConfigureAwait(false);
            var list = data.GetObjectsAfterMutate(
                existingMeteringPoints.Select(x => (IReplayableObject)x).ToList(),
                data.EffectiveDate).Select(x => (MeteringPoint)x).OrderBy(x => x.FromDate).ToList();
            await _meteringPointRepository.AddOrUpdateMeteringPoints(list);

            return new OkResult();
        }

        [FunctionName("Disconnect")]
        public async Task<IActionResult> Disconnect(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MeteringPointDisconnectedEvent>(requestBody);
            var existingMeteringPoints = await _meteringPointRepository.GetByIdAndDateAsync(data.Id, data.EffectiveDate)
                .ConfigureAwait(false);
            var list = data.GetObjectsAfterMutate(
                existingMeteringPoints.Select(x => (IReplayableObject)x).ToList(),
                data.EffectiveDate).Select(x => (MeteringPoint)x).OrderBy(x => x.FromDate).ToList();
            await _meteringPointRepository.AddOrUpdateMeteringPoints(list);

            return new OkResult();
        }

        [FunctionName("ChangeSettlementMethod")]
        public async Task<IActionResult> ChangeSettlementMethod(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var data = JsonConvert.DeserializeObject<MeteringPointSettlementMethodChangedEvent>(requestBody);
            var existingMeteringPoints = await _meteringPointRepository.GetByIdAndDateAsync(data.Id, data.EffectiveDate)
                .ConfigureAwait(false);
            var list = data.GetObjectsAfterMutate(
                existingMeteringPoints.Select(x => (IReplayableObject)x).ToList(),
                data.EffectiveDate).Select(x => (MeteringPoint)x).OrderBy(x => x.FromDate).ToList();
            await _meteringPointRepository.AddOrUpdateMeteringPoints(list);

            return new OkResult();
        }
    }
}
