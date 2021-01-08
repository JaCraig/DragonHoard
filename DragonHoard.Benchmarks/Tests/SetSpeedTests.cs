using BenchmarkDotNet.Attributes;
using DragonHoard.Core;
using DragonHoard.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DragonHoard.Benchmarks.Tests
{
    [MemoryDiagnoser]
    public class SetSpeedTests
    {
        private ICache IMemoryCacheCache { get; set; }

        private ICache InMemoryCache { get; set; }

        [Benchmark(Baseline = true)]
        public void InMemory()
        {
            InMemoryCache.Set("Test", new { A = 1 });
        }

        [Benchmark]
        public void MicrosoftMemory()
        {
            IMemoryCacheCache.Set("Test", new { A = 1 });
        }

        [GlobalSetup]
        public void Setup()
        {
            new ServiceCollection().AddOptions().AddCanisterModules(x => x.RegisterInMemoryHoard().RegisterMemoryCacheHoard());
            InMemoryCache = Canister.Builder.Bootstrapper.Resolve<Cache>().GetOrAddCache("In Memory");
            IMemoryCacheCache = Canister.Builder.Bootstrapper.Resolve<Cache>().GetOrAddCache("Microsoft.Extensions.Caching.Memory");
        }
    }
}