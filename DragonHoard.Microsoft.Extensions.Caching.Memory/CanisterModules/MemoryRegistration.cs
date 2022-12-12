using Canister.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DragonHoard.MicrosoftExtensionsCachingMemory.CanisterModules
{
    /// <summary>
    /// Memory reg module
    /// </summary>
    /// <seealso cref="IModule"/>
    public class MemoryRegistration : IModule
    {
        /// <summary>
        /// Order to run this in
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Loads the module using the bootstrapper
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        public void Load(IServiceCollection? bootstrapper)
        {
            bootstrapper?.AddSingleton<IMemoryCache, Microsoft.Extensions.Caching.Memory.MemoryCache>();
        }
    }
}