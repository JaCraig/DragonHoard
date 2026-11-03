# DragonHoard

[![.NET Publish](https://github.com/JaCraig/DragonHoard/actions/workflows/dotnet-publish.yml/badge.svg)](https://github.com/JaCraig/DragonHoard/actions/workflows/dotnet-publish.yml)
[![License: Apache-2.0](https://img.shields.io/badge/License-Apache--2.0-blue.svg)](./LICENSE)

DragonHoard is a .NET in-memory caching library with a small API for storing,
retrieving, expiring, and grouping cached values. Choose its lightweight in-memory
provider or a provider backed by `Microsoft.Extensions.Caching.Memory`.

## Why DragonHoard?

- Register caches through `Microsoft.Extensions.DependencyInjection`.
- Create named caches for separate workloads.
- Set absolute or sliding expiration, priority, size, and tags per entry.
- Retrieve or remove entries by key or tag, and compact a cache when needed.
- Target .NET 8, .NET 9, and .NET 10.

DragonHoard is an in-process cache. It does not persist entries or synchronize them
between application instances.

## Choose a provider

Install one provider package for your application. Both include the core API as a
dependency.

| Provider               | NuGet package                                                                                                                       | Use when                                                                    |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------- |
| DragonHoard in-memory  | [`DragonHoard.InMemory`](https://www.nuget.org/packages/DragonHoard.InMemory)                                                       | You want DragonHoard's built-in in-memory cache.                            |
| Microsoft memory cache | [`DragonHoard.Microsoft.Extensions.Caching.Memory`](https://www.nuget.org/packages/DragonHoard.Microsoft.Extensions.Caching.Memory) | You want DragonHoard's API backed by `Microsoft.Extensions.Caching.Memory`. |

## Install

Install the provider you chose with the .NET CLI:

```sh
dotnet add package DragonHoard.InMemory
```

Or install the Microsoft memory cache provider:

```sh
dotnet add package DragonHoard.Microsoft.Extensions.Caching.Memory
```

## Quick start

This example registers the in-memory provider, creates the default cache, and stores
and retrieves a value. Replace `AddInMemoryHoard()` with `AddMemoryCacheHoard()` to
use the Microsoft memory cache provider.

```csharp
using DragonHoard.Core;
using DragonHoard.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddInMemoryHoard();

using var serviceProvider = services.BuildServiceProvider();
var cacheManager = serviceProvider.GetRequiredService<Cache>();
var cache = cacheManager.GetOrAddCache()!;

cache.Set("greeting", "Hello, cache!");
if (cache.TryGetValue<string>("greeting", out var greeting))
{
    Console.WriteLine(greeting);
}
```

The `Cache` manager is registered as a singleton. It provides access to the default
cache and additional named caches. Resolve it through dependency injection rather
than creating a new manager for each operation.

## Common operations

### Create a named cache

Use a descriptive name to keep unrelated data in separate caches:

```csharp
ICache productCache = cacheManager.GetOrAddCache("products")!;
```

### Set expiration and tags

```csharp
cache.Set("product:42", product, new CacheEntryOptions
{
    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
    Tags = ["products", "featured"]
});
```

Entries can also use `AbsoluteExpiration`, `SlidingExpiration`, `Priority`, and
`Size` options. The in-memory provider supports capacity and scan-frequency
configuration through `AddInMemoryHoard`:

```csharp
services.AddInMemoryHoard(options =>
{
    options.MaxCacheSize = 10_000;
    options.ScanFrequency = TimeSpan.FromMinutes(2);
});
```

### Remove or retrieve entries

```csharp
cache.Remove("product:42");
bool found = cache.TryGetValue<Product>("product:42", out var product);

cache.RemoveByTag("featured");
Product[] featuredProducts = cache.GetByTag<Product>("featured");
```

### Compact a cache

`Compact` removes a fraction of entries according to the provider's eviction
policy. For DragonHoard's in-memory provider, expired entries are removed first;
remaining entries are considered from lowest to highest priority, then by expiration
and last-access time.

```csharp
cache.Compact(0.25); // Ask the provider to remove about 25% of its entries.
```

## Expiration and provider behavior

Expiration and eviction details depend on the provider. The in-memory provider uses
a one-minute scan frequency by default. Expired values should not be treated as
available after expiration, but background cleanup timing can depend on cache
activity. `Compact` also follows the selected provider's eviction policy.

## Develop and test

The repository contains the library, an example application, tests, and BenchmarkDotNet
benchmarks. A .NET 10 SDK is needed to build and test the full solution; the library
packages target .NET 8, .NET 9, and .NET 10.

```sh
git clone https://github.com/JaCraig/DragonHoard.git
cd DragonHoard
dotnet build DragonHoard.sln
dotnet test DragonHoard.sln
```

Run the example or benchmarks with:

```sh
dotnet run --project DragonHoard.Example
dotnet run -c Release --project DragonHoard.Benchmarks
```

Benchmark results depend on the machine, runtime, and workload. Run the benchmarks
yourself to compare current results rather than relying on historical numbers.

## Documentation and help

- [API reference](./docfx_project/index.md)
- [Example application](./DragonHoard.Example/Program.cs)
- [Contributing guide](./CONTRIBUTING.md)
- [Security policy](./SECURITY.md)
- [Report a bug or request a feature](https://github.com/JaCraig/DragonHoard/issues)

## License

DragonHoard is licensed under the [Apache License 2.0](./LICENSE).
