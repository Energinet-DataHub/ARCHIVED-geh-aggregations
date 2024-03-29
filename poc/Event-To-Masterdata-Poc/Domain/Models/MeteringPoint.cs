﻿// // Copyright 2020 Energinet DataHub A/S
// //
// // Licensed under the Apache License, Version 2.0 (the "License2");
// // you may not use this file except in compliance with the License.
// // You may obtain a copy of the License at
// //
// //     http://www.apache.org/licenses/LICENSE-2.0
// //
// // Unless required by applicable law or agreed to in writing, software
// // distributed under the License is distributed on an "AS IS" BASIS,
// // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// // See the License for the specific language governing permissions and
// // limitations under the License.

using System;
using Domain.DTOs;

namespace Domain.Models
{
    public class MeteringPoint : IReplayableObject
    {
        public Guid RowId { get; set; }

        public string Id { get; set; } = string.Empty;

        public ConnectionState ConnectionState { get; set; }

        public SettlementMethod? SettlementMethod { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ToDate { get; set; }

        public IReplayableObject ShallowCopy()
        {
            return (IReplayableObject)MemberwiseClone();
        }
    }
}
