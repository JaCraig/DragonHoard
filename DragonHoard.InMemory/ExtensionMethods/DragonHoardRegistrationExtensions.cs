/*
Copyright 2021 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Canister.Interfaces;
using DragonHoard.InMemory;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Reg extensions
    /// </summary>
    public static class DragonHoardInMemoryRegistrationExtensions
    {
        /// <summary>
        /// Adds the dragon hoard.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection? AddInMemoryHoard(this IServiceCollection? services)
        {
            return services.AddInMemoryHoard(options => options.ScanFrequency = TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Adds the in memory hoard.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection? AddInMemoryHoard(this IServiceCollection? services, Action<InMemoryCacheOptions> setupAction)
        {
            if (services is null)
                return services;
            services.AddDragonHoard();
            services.AddSingleton<InMemoryCache>();
            services.Configure(setupAction);
            return services;
        }

        /// <summary>
        /// Registers the dragon hoard.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The configuration object.</returns>
        public static ICanisterConfiguration? RegisterInMemoryHoard(this ICanisterConfiguration? bootstrapper)
        {
            return bootstrapper?.AddAssembly(typeof(DragonHoardInMemoryRegistrationExtensions).Assembly).RegisterDragonHoard();
        }
    }
}