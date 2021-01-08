using Microsoft.Extensions.DependencyInjection;
using System;

namespace DragonHoard.InMemory.Tests.Fixtures
{
    /// <summary>
    /// Dragon hoard fixture
    /// </summary>
    /// <seealso cref="System.IDisposable"/>
    public class DragonHoardFixture : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DragonHoardFixture"/> class.
        /// </summary>
        public DragonHoardFixture()
        {
            if (Canister.Builder.Bootstrapper is null)
            {
                new ServiceCollection().AddCanisterModules(configure => configure.AddAssembly(typeof(DragonHoardFixture).Assembly).RegisterInMemoryHoard());
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
        }
    }
}