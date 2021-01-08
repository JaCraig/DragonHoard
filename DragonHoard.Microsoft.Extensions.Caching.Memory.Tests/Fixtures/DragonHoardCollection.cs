using Xunit;

namespace DragonHoard.InMemory.Tests.Fixtures
{
    /// <summary>
    /// Collection
    /// </summary>
    /// <seealso cref="Xunit.ICollectionFixture{DragonHoard.InMemory.Tests.Fixtures.DragonHoardFixture}"/>
    [CollectionDefinition("DragonHoardCollection")]
    public class DragonHoardCollection : ICollectionFixture<DragonHoardFixture>
    {
    }
}