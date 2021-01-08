using DragonHoard.Core;
using Xunit;

namespace TestHelpers
{
    /// <summary>
    /// Test base class
    /// </summary>
    [Collection("DragonHoardCollection")]
    public abstract class TestBaseClass
    {
        /// <summary>
        /// Gets the cache.
        /// </summary>
        /// <value>The cache.</value>
        protected static Cache Cache => Canister.Builder.Bootstrapper.Resolve<Cache>();
    }
}