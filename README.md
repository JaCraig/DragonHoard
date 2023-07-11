# Dragon Hoard

[![.NET Publish](https://github.com/JaCraig/DragonHoard/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/JaCraig/DragonHoard/actions/workflows/dotnet-publish.yml)

Dragon Hoard is a fast, thread safe, developer friendly in-memory caching service.

## Setting Up the Library

Dragon Hoard can be set up using the ServiceCollection extension depending on the cache you want to set up:

    ServiceCollection.AddInMemoryHoard();

The above code adds the faster/lighter in-memory cache while:

    ServiceCollection.AddMemoryCacheHoard();

That allows the system to wrap Microsoft.Extensions.Caching.Memory.MemoryCache. Note that they are in separate Nuget packages so you'll have to download the one that you want.

## Basic Usage

The main class of interest is the Cache class found in DragonHoard.Core:

    public class ExampleClass
    {
        public ExampleClass(Cache myCache)
        {
            MyCache = myCache;
        }

        private Cache MyCache { get; set; }

        public void SomeMethod()
        {
            var Cache = MyCache.GetOrAddCache("CacheName");
        }
    }

The Cache object has a singleton lifespan and acts as a repository for your various caches. Note that it's generally a good idea to split caches based on their purpose so they can be treated accordingly and to reduce read/write contention. Once you have the ICache object that the GetOrAddCache method returns you have a couple of methods:

    Cache.Set(...) // Used to set a key/value pair in the cache using various settings. You can specify absolute expiration, sliding, priority, size, and any tags that should be associated with the entry.
    Cache.Remove(...) // Used to remove an entry based on the key.
    Cache.TryGetValue(...) // Used to retrieve an entry from the cache based on the key.
    Cache.RemoveByTag(...) // Will remove all entries that have been tagged with the corresponding string.
    Cache.GetByTag(...) // Returns an array containing all entries that were tagged using the string.
    Cache.Compact(...) // Used to remove a percentage of items from the cache.

The cache will clear out invalid items on a scheduled period based on the criteria you set. However the schedule may only kick off after you call one of the above methods depending on the type of cache. Similarly the determination of what gets removed when calling Compact is specific to the individual cache provider. For instance, the in-memory provider uses these steps:

1. Remove items that have expired.
2. Put items into buckets based on priority and then go off this:
   1. Remove items where absolute expiration is set going earliest to latest.
   2. Remove items where sliding expiration is set going earliest to latest.
   3. Remove items based on last accessed date/time.

## Give Me Speed

For those wondering why you'd want to use this over the MemoryCache provided by Microsoft, considering the following using the following setup:

``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1256 (1909/November2018Update/19H2)
Intel Core i7-9850H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.102
  [Host]     : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
  DefaultJob : .NET Core 5.0.2 (CoreCLR 5.0.220.61120, CoreFX 5.0.220.61120), X64 RyuJIT
```

---

Adding and removing an item from the cache is about 4 times faster and uses about 1/5 of the memory overhead:

|          Method |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|        InMemory |   422.7 ns |   7.29 ns |   8.96 ns |   420.4 ns |  1.00 |    0.00 | 0.0401 |     - |     - |     256 B |
| MicrosoftMemory | 2,123.0 ns | 179.06 ns | 527.96 ns | 1,906.8 ns |  3.96 |    0.29 | 0.2060 |     - |     - |    1296 B |

---

Retrieval of an item from the cache is about 50% faster:

|          Method |     Mean |    Error |   StdDev |   Median | Ratio | RatioSD | Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |---------:|---------:|---------:|---------:|------:|--------:|------:|------:|------:|----------:|
|        InMemory | 153.9 ns |  3.06 ns |  7.39 ns | 151.5 ns |  1.00 |    0.00 |     - |     - |     - |         - |
| MicrosoftMemory | 214.9 ns | 14.32 ns | 42.22 ns | 200.0 ns |  1.50 |    0.32 |     - |     - |     - |         - |

---

When creating/storing items at high throughput scenarios, the system is about 5x faster with reduced memory overhead even when aggressive cache cleanup is used:

|          Method |     Mean |     Error |    StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
|---------------- |---------:|----------:|----------:|------:|--------:|-------:|-------:|------:|----------:|
|        InMemory | 1.043 μs | 0.0934 μs | 0.2557 μs |  1.00 |    0.00 | 0.0362 | 0.0134 |     - |     232 B |
| MicrosoftMemory | 4.900 μs | 0.4043 μs | 1.1920 μs |  5.01 |    1.56 | 0.1755 | 0.0458 |     - |    1200 B |

---

And updating an item already in the cache is 8x faster with 12x improvement on memory consumption:

|          Method |       Mean |     Error |    StdDev |     Median | Ratio | RatioSD |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------------- |-----------:|----------:|----------:|-----------:|------:|--------:|-------:|------:|------:|----------:|
|        InMemory |   257.2 ns |  15.47 ns |  45.12 ns |   241.6 ns |  1.00 |    0.00 | 0.0165 |     - |     - |     104 B |
| MicrosoftMemory | 2,041.9 ns | 204.26 ns | 602.26 ns | 1,785.9 ns |  8.00 |    1.93 | 0.1907 |     - |     - |    1200 B |


## Installation

The library is available via Nuget with the package name "DragonHoard.InMemory" or "DragonHoard.Microsoft.Extensions.Caching.Memory". To install it run the following command in the Package Manager Console:

Install-Package DragonHoard.InMemory

or

Install-Package DragonHoard.Microsoft.Extensions.Caching.Memory

## Build Process

In order to build the library you may require the following:

1. Visual Studio 2019

Other than that, just clone the project and you should be able to load the solution and build without too much effort.
