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

using Energinet.DataHub.Aggregations.Infrastructure.Messaging.Registration;
using Energinet.DataHub.MarketRoles.IntegrationEventContracts;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Aggregations.Configuration
{
    internal static class EnergySupplierChangedHandlerConfiguration
    {
        internal static void ConfigureServices(IServiceCollection services)
        {
            services.ReceiveProtobufMessage<EnergySupplierChanged>(configuration =>
                configuration.WithParser(() => EnergySupplierChanged.Parser));
        }
    }
}
