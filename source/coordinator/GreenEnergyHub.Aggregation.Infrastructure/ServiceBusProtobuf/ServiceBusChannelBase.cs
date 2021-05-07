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
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using GreenEnergyHub.Messaging.Transport;
using Microsoft.Extensions.Logging;

namespace GreenEnergyHub.Aggregation.Infrastructure.ServiceBusProtobuf
{
    public abstract class ServiceBusChannelBase<T> : Channel, IAsyncDisposable
    {
        private readonly string _topic;
        private readonly ILogger<T> _logger;
        private readonly ServiceBusClient _client;
        private readonly ServiceBusSender _sender;

        public ServiceBusChannelBase(string connectionString, string topic, ILogger<T> logger)
        {
            _logger = logger;
            _topic = topic;
            // create a Service Bus client
            _client = new ServiceBusClient(connectionString);
            _sender = _client.CreateSender(_topic);

            _logger.LogInformation("ServiceBusClient is created");
        }

        public async ValueTask DisposeAsync()
        {
            await _client.DisposeAsync().ConfigureAwait(false);
            await _sender.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }

        protected override async Task WriteBulkAsync(IEnumerable<byte[]> dataList, CancellationToken cancellationToken = default)
        {
            try
            {
                // create a sender for the queue
                var sw = new Stopwatch();

                var messages = new Queue<ServiceBusMessage>();
                var dl = dataList.ToList();
                foreach (var serviceBusMessage in dl.Select(data => new ServiceBusMessage(new BinaryData(data))))
                {
                    messages.Enqueue(serviceBusMessage);
                }

                _logger.LogInformation("Sending Bulk");
                var messageCount = messages.Count;

                // while all messages are not sent to the Service Bus queue
                while (messages.Count > 0)
                {
                    // start a new batch
                    using var messageBatch = await _sender.CreateMessageBatchAsync(cancellationToken).ConfigureAwait(false);

                    // add the first message to the batch
                    if (messageBatch.TryAddMessage(messages.Peek()))
                    {
                        _logger.LogInformation("Dequeue {count}", messages.Count);

                        // dequeue the message from the .NET queue once the message is added to the batch
                        messages.Dequeue();
                    }
                    else
                    {
                        // if the first message can't fit, then it is too large for the batch
                        throw new Exception($"Message {messageCount - messages.Count} is too large and cannot be sent.");
                    }

                    // add as many messages as possible to the current batch
                    while (messages.Count > 0 && messageBatch.TryAddMessage(messages.Peek()))
                    {
                        _logger.LogInformation("Dequeue2 {count}", messageBatch.Count);

                        // dequeue the message from the .NET queue as it has been added to the batch
                        messages.Dequeue();
                    }

                    // now, send the batch
                    await _sender.SendMessagesAsync(messageBatch, cancellationToken).ConfigureAwait(false);

                    // if there are any remaining messages in the .NET queue, the while loop repeats
                }

                sw.Stop();
                _logger.LogInformation("Done Sending {Count} messages it took {ElapsedMilliseconds} ms", dl.Count, sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Got an error in ServiceBusChannel when trying to write {message}", e.Message);
                throw;
            }
        }

        protected override async Task WriteAsync(byte[] data, CancellationToken cancellationToken = default)
        {
            try
            {
                // create a message that we can send
                var message = new ServiceBusMessage(new BinaryData(data));

                var sw = new Stopwatch();
                _logger.LogInformation($"Sending ");

                // send the message
                sw.Start();
                await _sender.SendMessageAsync(message, cancellationToken).ConfigureAwait(false);
                sw.Stop();
                _logger.LogInformation("Done Sending  it took {ElapsedMilliseconds} ms", sw.ElapsedMilliseconds);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Got an error in ServiceBusChannel when trying to write");
                throw;
            }
        }
    }
}
