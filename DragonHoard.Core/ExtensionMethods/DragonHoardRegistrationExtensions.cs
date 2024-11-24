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
using DragonHoard.Core;
using DragonHoard.Core.Interfaces;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Reg extensions
    /// </summary>
    public static class DragonHoardRegistrationExtensions
    {
        /// <summary>
        /// Adds the dragon hoard.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>The service collection</returns>
        public static IServiceCollection? AddDragonHoard(this IServiceCollection? services)
        {
            if (services.Exists<Cache>())
                return services;
            return services?.AddSingleton<Cache>()
                .AddAllSingleton<ICache>();
        }

        /// <summary>
        /// Registers the dragon hoard.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The configuration object.</returns>
        public static ICanisterConfiguration? RegisterDragonHoard(this ICanisterConfiguration? bootstrapper) => bootstrapper?.AddAssembly(typeof(DragonHoardRegistrationExtensions).Assembly);
    }
}