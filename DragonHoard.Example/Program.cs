using DragonHoard.Core;
using Microsoft.Extensions.DependencyInjection;

namespace DragonHoard.Example
{
    /// <summary>
    /// Example class.
    /// </summary>
    internal class ExampleClass
    {
        /// <summary>
        /// Gets or sets a.
        /// </summary>
        /// <value>
        /// a.
        /// </value>
        public int A { get; set; }
    }

    /// <summary>
    /// Example program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            // Create our service provider.
            var ServiceProvider = new ServiceCollection().AddCanisterModules()?.BuildServiceProvider();

            // Get our cache provider.
            var CacheProvider = ServiceProvider?.GetService<Cache>();

            if (CacheProvider is null)
                return;

            // Get our cache. This will create a new cache if one doesn't exist with the name "Default".
            var Cache = CacheProvider.GetOrAddCache();

            if (Cache is null)
                return;

            // Add our data to the cache.
            Cache.Set("Test", new ExampleClass { A = 12 });

            // Check if our data is in the cache and retrieve it.
            Console.WriteLine("Data found: {0}", Cache.TryGetValue("Test", out ExampleClass value));
            Console.WriteLine("Our stored data: {0}", value.A);
        }
    }
}