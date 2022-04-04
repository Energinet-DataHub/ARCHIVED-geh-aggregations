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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Energinet.DataHub.Aggregation.Coordinator.CoordinatorFunction
{
    public static class DependencyInjectionExtensions
    {
        public static void RegisterAllTypes<T>(
            this IServiceCollection services,
            IEnumerable<Assembly> assemblies,
            ServiceLifetime lifetime = ServiceLifetime.Transient)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var typesFromAssemblies =
                assemblies.SelectMany(a => a.DefinedTypes.Where(x => x.ImplementedInterfaces.Contains(typeof(T))));
            foreach (var type in typesFromAssemblies)
            {
                services.Add(new ServiceDescriptor(typeof(T), type, lifetime));
            }
        }

        public static IServiceCollection AddSingletonsByConvention(
            this IServiceCollection services,
            Assembly assembly,
            Func<Type, bool> predicate)
            => services.AddSingletonsByConvention(assembly, predicate, predicate);

        private static IServiceCollection AddSingletonsByConvention(
            this IServiceCollection services,
            Assembly assembly,
            Func<Type, bool> interfacePredicate,
            Func<Type, bool> implementationPredicate)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly));
            }

            var interfaces = assembly.ExportedTypes
                .Where(x => x.IsInterface && interfacePredicate(x))
                .ToList();
            var implementations = assembly.ExportedTypes
                .Where(x => !x.IsInterface && !x.IsAbstract && implementationPredicate(x))
                .ToList();
            foreach (var @interface in interfaces)
            {
                var implementation = implementations.FirstOrDefault(x => @interface.IsAssignableFrom(x));
                if (implementation == null)
                {
                    continue;
                }

                services.AddSingleton(@interface, implementation);
            }

            return services;
        }
    }
}
