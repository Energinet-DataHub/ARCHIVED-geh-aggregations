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

using System.ComponentModel;

namespace Energinet.DataHub.Aggregations.AggregationResultReceiver.Domain.Enums
{
    public enum AggregationStep
    {
        [Description("1")]
        One = 0,
        [Description("2")]
        Two = 1,
        [Description("3")]
        Three = 2,
        [Description("10")]
        Ten = 3,
        [Description("11")]
        Eleven = 4,
        [Description("12")]
        Twelve = 5,
        [Description("13")]
        Thirteen = 6,
        [Description("14")]
        Fourteen = 7,
        [Description("15")]
        Fifteen = 8,
        [Description("16")]
        Sixteen = 9,
        [Description("17")]
        Seventeen = 10,
        [Description("18")]
        Eighteen = 11,
        [Description("19")]
        Nineteen = 12,
        [Description("20")]
        Twenty = 13,
        [Description("21")]
        TwentyOne = 14,
    }
}
