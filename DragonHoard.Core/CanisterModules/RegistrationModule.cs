﻿/*
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
using DragonHoard.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DragonHoard.Core.CanisterModules
{
    /// <summary>
    /// Registration module
    /// </summary>
    /// <seealso cref="IModule"/>
    public class RegistrationModule : IModule
    {
        /// <summary>
        /// Order to run this in
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Loads the module using the bootstrapper
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IBootstrapper? bootstrapper)
        {
            bootstrapper?.Register<Cache>(ServiceLifetime.Singleton)
                .RegisterAll<ICache>(ServiceLifetime.Singleton);
        }
    }
}