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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.Application.Coordinator;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData;
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Categories;

namespace GreenEnergyHub.Aggregation.Tests
{
    [UnitTest]
    public class CoordinatorServiceTests
    {
        private readonly CoordinatorService _sut;
        private readonly ICalculationEngine _calculationEngine;
        private readonly ITriggerBaseArguments _triggerBaseArguments;
        private readonly IMetaDataDataAccess _metaDataDataAccess;
        private readonly CoordinatorSettings _coordinatorSettings;
        private readonly ILogger<CoordinatorService> _logger;
        private readonly Guid _result1Id = Guid.NewGuid();
        private readonly Guid _result2Id = Guid.NewGuid();
        private readonly Guid _result3Id = Guid.NewGuid();

        public CoordinatorServiceTests()
        {
            _coordinatorSettings = Substitute.For<CoordinatorSettings>();
            _logger = Substitute.For<ILogger<CoordinatorService>>();
            _metaDataDataAccess = Substitute.For<IMetaDataDataAccess>();
            _triggerBaseArguments = Substitute.For<ITriggerBaseArguments>();
            _calculationEngine = Substitute.For<ICalculationEngine>();
            _sut = new CoordinatorService(_coordinatorSettings, _logger, _metaDataDataAccess, _triggerBaseArguments, _calculationEngine);
        }

        [Fact]
        public async Task TestStartAggregationJobAsync_CreateJobAndJobResultsAsync_CreateJobAsync()
        {
            //Arrange + Act
            var jobId = Guid.NewGuid();
            await _sut.StartAggregationJobAsync(jobId, Guid.NewGuid(), JobProcessTypeEnum.Aggregation, false, "owner", ResolutionEnum.Hour, CancellationToken.None).ConfigureAwait(false);

            //Assert
            await _metaDataDataAccess.Received(1).CreateJobAsync(Arg.Is<Job>(x => x.Id == jobId)).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestStartAggregationJobAsync_CreateJobAndJobResultsAsync_GetResultsByTypeAsync()
        {
            //Arrange + Act
            await _sut.StartAggregationJobAsync(Guid.NewGuid(), Guid.NewGuid(), JobProcessTypeEnum.Aggregation, false, "owner", ResolutionEnum.Hour, CancellationToken.None).ConfigureAwait(false);

            //Assert
            await _metaDataDataAccess.Received(1).GetResultsByTypeAsync(Arg.Any<JobTypeEnum>()).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestStartAggregationJobAsync_CreateJobAndJobResultsAsync_CreateJobResultAsync()
        {
            //Arrange + Act
            var resultList = SetUpResults().Where(x => x.Type == JobTypeEnum.Aggregation);
            _metaDataDataAccess.GetResultsByTypeAsync(JobTypeEnum.Aggregation).Returns(Task.FromResult(resultList.AsEnumerable()));
            var sut = new CoordinatorService(_coordinatorSettings, _logger, _metaDataDataAccess, _triggerBaseArguments, _calculationEngine);
            await sut.StartAggregationJobAsync(Guid.NewGuid(), Guid.NewGuid(), JobProcessTypeEnum.Aggregation, false, "owner", ResolutionEnum.Hour, CancellationToken.None).ConfigureAwait(false);

            //Assert
            await _metaDataDataAccess.Received(1).CreateJobResultAsync(Arg.Is<JobResult>(x => x.ResultId == _result1Id)).ConfigureAwait(false);
            await _metaDataDataAccess.Received(1).CreateJobResultAsync(Arg.Is<JobResult>(x => x.ResultId == _result2Id)).ConfigureAwait(false);
            await _metaDataDataAccess.Received(0).CreateJobResultAsync(Arg.Is<JobResult>(x => x.ResultId == _result3Id)).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestStartAggregationJobAsync_GetTriggerAggregationArguments()
        {
            //Arrange + Act
            var jobId = Guid.NewGuid();
            await _sut.StartAggregationJobAsync(jobId, Guid.NewGuid(), JobProcessTypeEnum.Aggregation, false, "owner", ResolutionEnum.Hour, CancellationToken.None).ConfigureAwait(false);

            //Assert
            _triggerBaseArguments.Received(1).GetTriggerAggregationArguments(Arg.Is<Job>(x => x.Id == jobId));
        }

        [Fact]
        public async Task TestStartAggregationJobAsync_CreateAndRunCalculationJobAsync()
        {
            //Arrange + Act
            var jobId = Guid.NewGuid();
            await _sut.StartAggregationJobAsync(jobId, Guid.NewGuid(), JobProcessTypeEnum.Aggregation, false, "owner", ResolutionEnum.Hour, CancellationToken.None).ConfigureAwait(false);

            //Assert
            await _calculationEngine.Received(1).CreateAndRunCalculationJobAsync(Arg.Is<Job>(x => x.Id == jobId), Arg.Any<List<string>>(), Arg.Any<string>(), Arg.Any<CancellationToken>()).ConfigureAwait(false);
        }

        private IEnumerable<Result> SetUpResults()
        {
            var results = new List<Result>();
            results.Add(new Result(_result1Id, "result1", JobTypeEnum.Aggregation, true, 10, ResultGroupingEnum.GridArea));
            results.Add(new Result(_result2Id, "result2", JobTypeEnum.Aggregation, true, 20, ResultGroupingEnum.GridArea));
            results.Add(new Result(_result3Id, "result3", JobTypeEnum.Wholesale, true, 10, ResultGroupingEnum.GridArea));
            return results;
        }
    }
}
