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
using System.Threading.Tasks;
using Energinet.DataHub.Aggregations.LocalMessageHub.Functions;
using Energinet.DataHub.MessageHub.Client.DataAvailable;
using Energinet.DataHub.MessageHub.Model.Model;
using Moq;
using Xunit;
using Xunit.Categories;

namespace Energinet.DataHub.Aggregations.LocalMessageHub.Tests.Functions
{
    [UnitTest]
    public class DataAvailableForMessageHubTests
    {
        [Fact]
        public async Task Test_DataAvailable_Success()
        {
            var metaCorrelationId = Guid.NewGuid();
            var metaRecipient = "1234";
            var metaDataUniqueId = Guid.NewGuid();

            var metaData = new Dictionary<string, string>()
            {
                { "correlationId", metaCorrelationId.ToString() },
                { "recipient", metaRecipient },
                { "dataUniqueId", metaDataUniqueId.ToString() },
            };

            var mock = new Mock<IDataAvailableNotificationSender>();
            await BuildTestTask(mock, metaData).ConfigureAwait(false);

            mock.Verify(
                sender => sender.SendAsync(metaCorrelationId.ToString(), BuildDataAvailableDto(metaRecipient, metaDataUniqueId)));
        }

        [Fact]
        public void Test_DataAvailable_Throws_MetaData_IsNull()
        {
            var mock = new Mock<IDataAvailableNotificationSender>();

            Assert.ThrowsAsync<ArgumentNullException>(async () => await BuildTestTask(mock, null).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public void Test_DataAvailable_Throws_CorrelationId_ParseError()
        {
            var metaRecipient = "1234";
            var metaDataUniqueId = Guid.NewGuid();

            var metaData = new Dictionary<string, string>()
            {
                { "recipient", metaRecipient },
                { "dataUniqueId", metaDataUniqueId.ToString() },
            };

            var mock = new Mock<IDataAvailableNotificationSender>();

            Assert.ThrowsAsync<ArgumentException>(async () => await BuildTestTask(mock, metaData).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public void Test_DataAvailable_Throws_DataUniqueId_ParseError()
        {
            var metaCorrelationId = Guid.NewGuid();
            var metaRecipient = "1234";

            var metaData = new Dictionary<string, string>()
            {
                { "correlationId", metaCorrelationId.ToString() },
                { "recipient", metaRecipient },
            };

            var mock = new Mock<IDataAvailableNotificationSender>();

            Assert.ThrowsAsync<ArgumentException>(async () => await BuildTestTask(mock, metaData).ConfigureAwait(false)).ConfigureAwait(false);
        }

        [Fact]
        public void Test_DataAvailable_Throws_Recipient_ParseError()
        {
            var metaCorrelationId = Guid.NewGuid();
            var metaDataUniqueId = Guid.NewGuid();

            var metaData = new Dictionary<string, string>()
            {
                { "correlationId", metaCorrelationId.ToString() },
                { "dataUniqueId", metaDataUniqueId.ToString() },
            };

            var mock = new Mock<IDataAvailableNotificationSender>();

            Assert.ThrowsAsync<ArgumentException>(async () => await BuildTestTask(mock, metaData).ConfigureAwait(false)).ConfigureAwait(false);
        }

        private static Task BuildTestTask(
            Mock<IDataAvailableNotificationSender> mock,
            Dictionary<string, string> metaData)
        {
            var blobStringData = It.IsAny<string>();
            var blobName = It.IsAny<string>();

            var context = new MockedFunctionContext();

            var dataAvailableForMessageHub = new DataAvailableForMessageHub(mock.Object);
            return dataAvailableForMessageHub.RunAsync(blobStringData, blobName, metaData, context.FunctionContext);
        }

        private static DataAvailableNotificationDto BuildDataAvailableDto(string recipientGln, Guid dataId)
        {
            var recipient = new GlobalLocationNumberDto(recipientGln);
            var messageType = new MessageTypeDto("Type");

            return new DataAvailableNotificationDto(dataId, recipient, messageType, DomainOrigin.Aggregations, false, 1);
        }
    }
}
