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

using System.Collections.Generic;
using Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Application.Converters
{
    /// <summary>
    /// Interface for converting list of aggregation results to CIM/XML documents
    /// </summary>
    public interface ICimXmlConverter
    {
        /// <summary>
        /// Convert list of ResultData to list of OutgoingResult, which contains cim/xml
        /// </summary>
        /// <param name="results">List of ResultData</param>
        /// <param name="messageData">JobCompletedEvent</param>
        /// <returns>List of OutgoingResult</returns>
        IEnumerable<OutgoingResult> Convert(IEnumerable<ResultData> results, JobCompletedEvent messageData);
    }
}
