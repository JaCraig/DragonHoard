using DragonHoard.Core;
using DragonHoard.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System.Linq;
using System.Threading.Tasks;
using TestHelpers;
using Xunit;

namespace DragonHoard.InMemory.Tests
{
    public class CacheTests : TestBaseClass<Cache>
    {
        public CacheTests()
        {
            TestObject = new Cache(new ICache[] { new MicrosoftExtensionsCachingMemory.MemoryCache(Canister.Builder.Bootstrapper.Resolve<IMemoryCache>()) });
        }

        [Fact]
        public void Compact()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.Equal(12, TestObject.Set("Test", new { A = 12 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag2" } }).A);
            Assert.Equal(14, TestObject.Set("Test2", new { A = 14 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag3" } }).A);
            Assert.Equal(20, TestObject.Set("Test3", new { A = 20 }, new CacheEntryOptions() { Tags = new string[] { "Tag2", "Tag3" } }).A);
            Assert.True(TestObject.TryGetValue("Test", out object Value));
            Assert.True(TestObject.TryGetValue("Test2", out Value));
            Assert.True(TestObject.TryGetValue("Test3", out Value));
            TestObject.Compact(1);  //Microsoft's implementation seems fubar...
            Assert.True(TestObject.TryGetValue("Test", out Value));
            Assert.True(TestObject.TryGetValue("Test2", out Value));
            Assert.True(TestObject.TryGetValue("Test3", out Value));
        }

        [Fact]
        public void Creation()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.NotNull(TestObject);
            Assert.IsType<MicrosoftExtensionsCachingMemory.MemoryCache>(TestObject);
        }

        [Fact]
        public void Remove()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.Equal(12, TestObject.Set("Test", new { A = 12 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag2" } }).A);
            TestObject.Remove("Test");
            Assert.False(TestObject.TryGetValue("Test", out object Value));
            Assert.Null(Value);
            var Values = TestObject.GetByTag<object>("Tag2").ToArray();
            Assert.Empty(Values);
        }

        [Fact]
        public void RemoveByTag()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.Equal(12, TestObject.Set("Test", new { A = 12 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag2" } }).A);
            Assert.Equal(14, TestObject.Set("Test2", new { A = 14 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag3" } }).A);
            Assert.Equal(20, TestObject.Set("Test2", new { A = 20 }, new CacheEntryOptions() { Tags = new string[] { "Tag2", "Tag3" } }).A);
            TestObject.RemoveByTag("Tag1");
            var Values = TestObject.GetByTag<object>("Tag2").ToArray();
            Assert.Single(Values);
            Assert.Equal(20, ((dynamic)Values[0]).A);
        }

        [Fact]
        public void SetAndGet()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.Equal(12, TestObject.Set("Test", new { A = 12 }).A);
            Assert.True(TestObject.TryGetValue("Test", out object Value));
            dynamic Val = Value;
            Assert.Equal(12, Val.A);
        }

        [Fact]
        public void SetAndGetByTag()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.Equal(12, TestObject.Set("Test", new { A = 12 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag2" } }).A);
            Assert.Equal(14, TestObject.Set("Test2", new { A = 14 }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag3" } }).A);
            Assert.Equal(20, TestObject.Set("Test2", new { A = 20 }, new CacheEntryOptions() { Tags = new string[] { "Tag2", "Tag3" } }).A);
            var Values = TestObject.GetByTag<object>("Tag2").ToArray();
            Assert.Equal(2, Values.Length);
            Assert.Equal(12, ((dynamic)Values[0]).A);
            Assert.Equal(20, ((dynamic)Values[1]).A);
        }

        [Fact]
        public void SetAndGetByTagThreaded()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Parallel.For(0, 100000, x =>
            {
                Assert.Equal(x, TestObject.Set("Test", new { A = x }, new CacheEntryOptions() { Tags = new string[] { "Tag1", "Tag2" } }).A);
                var Values = TestObject.GetByTag<object>("Tag2").ToArray();
                Assert.Single(Values);
            });
        }

        [Fact]
        public void SetAndGetThreaded()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Parallel.For(0, 1000000, x =>
            {
                TestObject.Set("Test", new { A = x });
                Assert.True(TestObject.TryGetValue("Test", out object Value));
                Assert.NotNull(Value);
            });
        }

        [Fact]
        public void SetMultipleTimesGet()
        {
            using var TestObject = Cache.GetOrAddCache().Clone();
            Assert.Equal(12, TestObject.Set("Test", new { A = 12 }).A);
            Assert.Equal(14, TestObject.Set("Test", new { A = 14 }).A);
            Assert.Equal(20, TestObject.Set("Test", new { A = 20 }).A);
            Assert.True(TestObject.TryGetValue("Test", out object Value));
            dynamic Val = Value;
            Assert.Equal(20, Val.A);
        }
    }
}