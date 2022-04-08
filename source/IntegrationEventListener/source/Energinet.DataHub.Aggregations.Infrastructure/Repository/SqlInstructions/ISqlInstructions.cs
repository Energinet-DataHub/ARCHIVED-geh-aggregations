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

using Energinet.DataHub.Aggregations.Domain.MasterData;

namespace Energinet.DataHub.Aggregations.Infrastructure.Repository.SqlInstructions
{
    /// <summary>
    /// This interface describes the insert and update script used on the master data object
    /// </summary>
    /// <typeparam name="T">a master data object</typeparam>
    internal interface ISqlInstructions<T>
        where T : IMasterDataObject
    {
        /// <summary>
        /// The SQL string for updating the master data object
        /// </summary>
        string UpdateSql { get; }

        /// <summary>
        /// The SQL string for inserting the master data object
        /// </summary>
        string InsertSql { get;  }

        /// <summary>
        /// The SQL string getting objects by their Id
        /// </summary>
        string GetSql { get; }

        /// <summary>
        /// An anonymous object with the parameters used in the SQL script when updating
        /// </summary>
        /// <param name="masterDataObject"></param>
        /// <returns>An anonymous object </returns>
        object UpdateParameters(T masterDataObject);

        /// <summary>
        /// An anonymous object with the parameters used in the SQL script when inserting
        /// </summary>
        /// <param name="masterDataObject"></param>
        /// <returns>An anonymous object </returns>
        object InsertParameters(T masterDataObject);
    }
}
