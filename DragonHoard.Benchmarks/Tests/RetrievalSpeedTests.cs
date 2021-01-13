using BenchmarkDotNet.Attributes;
using DragonHoard.Core;
using DragonHoard.Core.Interfaces;
using DragonHoard.InMemory;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DragonHoard.Benchmarks.Tests
{
    [MemoryDiagnoser, HtmlExporter]
    public class RetrievalSpeedTests
    {
        private ICache IMemoryCacheCache { get; set; }

        private ICache InMemoryCache { get; set; }

        private Random Rand { get; set; }

        [Benchmark(Baseline = true)]
        public void InMemory()
        {
            InMemoryCache.TryGetValue("Testing", out object Value);
        }

        [Benchmark]
        public void MicrosoftMemory()
        {
            IMemoryCacheCache.TryGetValue("Testing", out object Value);
        }

        [GlobalSetup]
        public void Setup()
        {
            Rand = new Random();
            var Services = new ServiceCollection().AddOptions()
                .Configure<InMemoryCacheOptions>(options => { options.ScanFrequency = TimeSpan.FromSeconds(10); })
                .Configure<MemoryCacheOptions>(options => { options.ExpirationScanFrequency = TimeSpan.FromSeconds(10); });
            Services.AddCanisterModules(x => x.RegisterInMemoryHoard().RegisterMemoryCacheHoard());
            InMemoryCache = Canister.Builder.Bootstrapper.Resolve<Cache>().GetOrAddCache("In Memory");
            IMemoryCacheCache = Canister.Builder.Bootstrapper.Resolve<Cache>().GetOrAddCache("Microsoft.Extensions.Caching.Memory");

            InMemoryCache.Set("Testing", new { A = 1 });
            IMemoryCacheCache.Set("Testing", new { A = 1 });
        }
    }
}