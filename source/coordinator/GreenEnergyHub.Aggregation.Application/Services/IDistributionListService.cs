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

namespace GreenEnergyHub.Aggregation.Application.Services
{
    /// <summary>
    /// A service for getting distribution lists
    /// </summary>
    public interface IDistributionListService
    {
        /// <summary>
        /// Translate a grid area code to a RecipientPartyID_mRID or DELEGATIONS if it exists
        /// </summary>
        /// <param name="gridAreaCode"></param>
        /// <returns>string with Id</returns>
        public string GetDistributionItem(string gridAreaCode);
    }
}
