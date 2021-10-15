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
using GreenEnergyHub.Aggregation.Domain.DTOs.MetaData.Enums;
using NodaTime;

namespace GreenEnergyHub.Aggregation.Domain.DTOs.MetaData
{
    public class Result
    {
        public Result() { }

        public Result(Guid id, string name, JobTypeEnum type, bool convertToXml, int order, ResultGroupingEnum grouping, string? description = null)
        {
            Id = id;
            Name = name;
            Description = description;
            Type = type;
            ConvertToXml = convertToXml;
            Order = order;
            Grouping = grouping;
            CreatedDate = SystemClock.Instance.GetCurrentInstant();
        }

        /// <summary>
        /// Id of result
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Name of result
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Business description of what the result is meant to be, what it consists of and how it is used in a business context
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Type of result ie. Aggregation, Wholesale
        /// </summary>
        public JobTypeEnum Type { get; set; }

        /// <summary>
        /// Boolean value to tell whethe result should be converted to XML
        /// </summary>
        public bool ConvertToXml { get; set; }

        /// <summary>
        /// Order of result, used to define order of calculation in Databricks job
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Grouping of result iw, Neighbour, Grid area, Balance Responsible, Energy Supplier
        /// </summary>
        public ResultGroupingEnum Grouping { get; set; }

        /// <summary>
        /// Date and time when result is created
        /// </summary>
        public Instant CreatedDate { get; set; }

        /// <summary>
        /// Date and time when result is deleted
        /// </summary>
        public Instant? DeletedDate { get; set; }
    }
}
