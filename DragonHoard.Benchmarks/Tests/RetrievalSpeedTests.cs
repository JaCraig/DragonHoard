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

        [Benchmark(Baseline = true)]
        public void InMemory()
        {
            InMemoryCache.TryGetValue("Testing", out object _);
        }

        [Benchmark]
        public void MicrosoftMemory()
        {
            IMemoryCacheCache.TryGetValue("Testing", out object _);
        }

        [GlobalSetup]
        public void Setup()
        {
            var Services = new ServiceCollection().AddOptions()
                .Configure<InMemoryCacheOptions>(options => options.ScanFrequency = TimeSpan.FromSeconds(10))
                .Configure<MemoryCacheOptions>(options => options.ExpirationScanFrequency = TimeSpan.FromSeconds(10));
            var ServiceProvider = Services.AddCanisterModules(x => x.RegisterInMemoryHoard().RegisterMemoryCacheHoard()).BuildServiceProvider();
            InMemoryCache = ServiceProvider.GetService<Cache>().GetOrAddCache("In Memory");
            IMemoryCacheCache = ServiceProvider.GetService<Cache>().GetOrAddCache("Microsoft.Extensions.Caching.Memory");

            InMemoryCache.Set("Testing", new { A = 1 });
            IMemoryCacheCache.Set("Testing", new { A = 1 });
        }
    }
}