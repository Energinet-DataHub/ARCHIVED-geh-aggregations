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
using System.Threading.Tasks;
using GreenEnergyHub.Aggregation.TestData.Infrastructure.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.TestData.Infrastructure.CosmosDb
{
    public sealed class MasterDataStorage : IMasterDataStorage, IDisposable
    {
        private const string DatabaseId = "master-data";
        private readonly ILogger<MasterDataStorage> _logger;
        private readonly CosmosClient _client;

        public MasterDataStorage(GeneratorSettings generatorSettings, ILogger<MasterDataStorage> logger)
        {
            _logger = logger;
            if (generatorSettings != null)
            {
                _client = new CosmosClient(generatorSettings.MasterDataStorageConnectionString);
            }
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task WriteAsync<T>(IEnumerable<T> records, string containerName)
            where T : IStoragebleObject
        {
            try
            {
                var container = _client.GetContainer(DatabaseId, containerName);
                var importTasks = new List<Task>();

                if (records != null)
                {
                    foreach (var record in records)
                    {
                        importTasks.Add(container.CreateItemAsync(record)
                            .ContinueWith(
                                response =>
                                {
                                    if (response.IsCompletedSuccessfully)
                                    {
                                        return;
                                    }

                                    var aggExceptions = response.Exception;
                                    if (aggExceptions == null)
                                    {
                                        return;
                                    }

                                    if (aggExceptions.InnerExceptions.FirstOrDefault(innerEx =>
                                        innerEx is CosmosException) is CosmosException cosmosException)
                                    {
                                        _logger.LogError(
                                            "Received {StatusCode} ({Message})",
                                            cosmosException.StatusCode,
                                            cosmosException.Message);
                                    }
                                    else
                                    {
                                        _logger.LogError(
                                            "Exception {Exception}.",
                                            aggExceptions.InnerExceptions.FirstOrDefault());
                                    }
                                }, TaskScheduler.Default));
                    }
                }

                await Task.WhenAll(importTasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not put item in cosmos");
                throw;
            }
        }

        public async Task PurgeContainerAsync(string containerName, string partitionKey)
        {
            var container = _client.GetContainer(DatabaseId, containerName);
            try
            {
                await container.DeleteContainerAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Tried to delete container");
                throw;
            }

            var containerProperties = new ContainerProperties()
            {
                Id = containerName,
                PartitionKeyPath = $"/{partitionKey}",
                IndexingPolicy = new IndexingPolicy()
                {
                    Automatic = false,
                    IndexingMode = IndexingMode.Lazy,
                },
            };

            await _client.GetDatabase(DatabaseId).
                    CreateContainerIfNotExistsAsync(
                        containerProperties,
                        ThroughputProperties.CreateAutoscaleThroughput(4000)).
            ConfigureAwait(false);
        }

        public async Task WriteAsync<T>(T record, string containerName)
            where T : IStoragebleObject
        {
            var container = _client.GetContainer(DatabaseId, containerName);
            await container.CreateItemAsync(record).ConfigureAwait(false);
        }

        public async Task WriteAsync<T>(IAsyncEnumerable<T> records, string containerName)
            where T : IStoragebleObject
        {
            try
            {
                var container = _client.GetContainer(DatabaseId, containerName);
                var importTasks = new List<Task>();

                if (records != null)
                {
                    await foreach (var record in records)
                    {
                        importTasks.Add(container.CreateItemAsync(record)
                            .ContinueWith(
                                response =>
                                {
                                    if (response.IsCompletedSuccessfully)
                                    {
                                        return;
                                    }

                                    var aggExceptions = response.Exception;
                                    if (aggExceptions == null)
                                    {
                                        return;
                                    }

                                    if (aggExceptions.InnerExceptions.FirstOrDefault(innerEx =>
                                        innerEx is CosmosException) is CosmosException cosmosException)
                                    {
                                        _logger.LogError("Received {StatusCode} ({Message})", cosmosException.StatusCode, cosmosException.Message);
                                    }
                                    else
                                    {
                                        _logger.LogError("Exception {Exception}.", aggExceptions.InnerExceptions.FirstOrDefault());
                                    }
                                }, TaskScheduler.Default));
                    }
                }

                await Task.WhenAll(importTasks).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not put item in cosmos");
                throw;
            }
        }
    }
}
