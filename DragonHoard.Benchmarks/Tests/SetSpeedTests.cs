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
    public class SetSpeedTests
    {
        private ICache IMemoryCacheCache { get; set; }

        private ICache InMemoryCache { get; set; }

        private Random Rand { get; set; }

        [Benchmark(Baseline = true)]
        public void InMemory()
        {
            InMemoryCache.Set(Rand.Next(), new { A = 1 }, new CacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1) });
        }

        [Benchmark]
        public void MicrosoftMemory()
        {
            IMemoryCacheCache.Set(Rand.Next(), new { A = 1 }, new CacheEntryOptions() { AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1) });
        }

        [GlobalSetup]
        public void Setup()
        {
            Rand = new Random();
            var Services = new ServiceCollection().AddOptions()
                .Configure<InMemoryCacheOptions>(options => { options.ScanFrequency = TimeSpan.FromSeconds(10); })
                .Configure<MemoryCacheOptions>(options => { options.ExpirationScanFrequency = TimeSpan.FromSeconds(10); });
            var ServiceProvider = Services.AddCanisterModules(x => x.RegisterInMemoryHoard().RegisterMemoryCacheHoard()).BuildServiceProvider();
            InMemoryCache = ServiceProvider.GetService<Cache>().GetOrAddCache("In Memory");
            IMemoryCacheCache = ServiceProvider.GetService<Cache>().GetOrAddCache("Microsoft.Extensions.Caching.Memory");
        }
    }
}