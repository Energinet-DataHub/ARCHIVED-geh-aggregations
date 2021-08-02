﻿// Copyright 2020 Energinet DataHub A/S
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

using Google.Protobuf;
using GreenEnergyHub.Aggregation.Application.Coordinator.Interfaces;
using GreenEnergyHub.Aggregation.Domain;
using GreenEnergyHub.Aggregation.Domain.DTOs;
using GreenEnergyHub.Messaging.Transport;

namespace GreenEnergyHub.Messaging.Protobuf
{
    /// <summary>
    /// Maps an object to proto buf <see cref="IMessage"/>
    /// </summary>
    public abstract class ProtobufOutboundMapper
    {
        /// <summary>
        /// Map application message to protobuf
        /// </summary>
        /// <param name="obj">Object to map</param>
        /// <param name="type">Type of message</param>
        /// <returns>Proto buf message</returns>
        public abstract IMessage Convert(IOutboundMessage obj, string type);
    }
}
