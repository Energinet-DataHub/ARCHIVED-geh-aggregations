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
using Energinet.DataHub.Aggregations.Domain.MasterData;
using NodaTime;

namespace Energinet.DataHub.Aggregations.Domain.MeteringPoints
{
    public class MeteringPoint : IMasterDataObject
    {
        public Guid RowId { get; set; }

        public string MeteringPointId { get; set; } = null!;

        public ConnectionState ConnectionState { get; set; }

        public SettlementMethod? SettlementMethod { get; set; }

        public MeteringPointType MeteringPointType { get; set; }

        public Instant FromDate { get; set; }

        public Instant ToDate { get; set; }

        public string GridArea { get; set; } = null!;

        public Resolution Resolution { get; set; }

        public string InGridArea { get; set; } = null!;

        public string OutGridArea { get; set; } = null!;

        public MeteringMethod MeteringMethod { get; set; }

        public string ParentMeteringPointId { get; set; } = null!;

        public Unit Unit { get; set; }

        public Product Product { get; set; }

        public T ShallowCopy<T>()
            where T : IMasterDataObject
        {
            return (T)MemberwiseClone();
        }
    }
}
