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

namespace Energinet.DataHub.Aggregation.Coordinator.Domain.Types
{
    // Keep aligned with the equivalent enum in Python:
    // https://github.com/Energinet-DataHub/geh-aggregations/blob/main/source/databricks/geh_stream/codelists/settlement_method.py
   public static class SettlementMethodType
    {
        public static string FlexSettledEbix => "D01";

        public static string Profiled => "E01";

        public static string NonProfiled => "E02";

        public static string FlexSettledNbs => "E15";

        public static string Ignored => string.Empty;
    }
}
