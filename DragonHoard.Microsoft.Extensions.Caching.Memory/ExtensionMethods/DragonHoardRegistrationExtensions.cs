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
using Microsoft.Extensions.Caching.Memory;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Reg extensions
    /// </summary>
    public static class DragonHoardMemoryRegistrationExtensions
    {
        /// <summary>
        /// Adds the dragon hoard.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection? AddMemoryCacheHoard(this IServiceCollection? services) => services.AddMemoryCacheHoard(options => options.ExpirationScanFrequency = TimeSpan.FromMinutes(1));

        /// <summary>
        /// Adds the dragon hoard.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="setupAction">The setup action.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection? AddMemoryCacheHoard(this IServiceCollection? services, Action<MemoryCacheOptions> setupAction)
        {
            if (services.Exists<MemoryCache>())
                return services;
            return services?.AddDragonHoard()
                ?.AddOptions()
                ?.AddSingleton<MemoryCache>()
                ?.AddMemoryCache()
                ?.Configure(setupAction);
        }

        /// <summary>
        /// Registers the dragon hoard.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The configuration object.</returns>
        public static ICanisterConfiguration? RegisterMemoryCacheHoard(this ICanisterConfiguration? bootstrapper) => bootstrapper?.AddAssembly(typeof(DragonHoardMemoryRegistrationExtensions).Assembly).RegisterDragonHoard();
    }
}