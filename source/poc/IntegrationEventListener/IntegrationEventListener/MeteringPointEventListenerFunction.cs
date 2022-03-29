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
