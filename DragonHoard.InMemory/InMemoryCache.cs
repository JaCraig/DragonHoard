/*
Copyright 2021 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using DragonHoard.Core;
using DragonHoard.Core.BaseClasses;
using DragonHoard.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DragonHoard.InMemory
{
    /// <summary>
    /// In memory cache
    /// </summary>
    /// <seealso cref="ICache"/>
    public class InMemoryCache : CacheBaseClass
    {
        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object _LockObject = new();

        /// <summary>
        /// The current size
        /// </summary>
        private long _CurrentSize;

        /// <summary>
        /// Gets or sets the last scan.
        /// </summary>
        /// <value>The last scan.</value>
        private DateTimeOffset _LastScan;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryCache"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public InMemoryCache(IEnumerable<IOptions<InMemoryCacheOptions>> options)
        {
            Options = options.FirstOrDefault()?.Value ?? InMemoryCacheOptions.Default;
            if (Options.ScanFrequency == default)
                Options.ScanFrequency = TimeSpan.FromMinutes(1);
            _LastScan = DateTimeOffset.UtcNow;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name { get; } = "In Memory";

        /// <summary>
        /// Internal cache
        /// </summary>
        private Dictionary<int, CacheEntry>? InternalCache { get; set; } = [];

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private InMemoryCacheOptions Options { get; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this cache.</returns>
        public override ICache Clone() => new InMemoryCache([Options]);

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <typeparam name="TOption">The type of the option.</typeparam>
        /// <param name="options">The options to use for the cache.</param>
        /// <returns>A copy of this cache.</returns>
        public override ICache Clone<TOption>(TOption options) => new InMemoryCache([options as IOptions<InMemoryCacheOptions> ?? Options]);

        /// <summary>
        /// Compacts the specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public override void Compact(double percentage)
        {
            if (InternalCache is null)
                return;
            var CurrentCount = InternalCache.Count;
            var ItemsToRemove = (int)Math.Round(CurrentCount * percentage, MidpointRounding.AwayFromZero);
            var TargetCount = CurrentCount - ItemsToRemove;

            if (TargetCount == CurrentCount)
                return;

            DateTimeOffset CurrentTime = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                CacheEntry[] Items = [.. InternalCache.Values];
                var EntriesToRemove = new List<CacheEntry>();
                var LowPriorty = new List<CacheEntry>();
                var NormalPriorty = new List<CacheEntry>();
                var HighPriorty = new List<CacheEntry>();
                for (var I = 0; I < Items.Length; I++)
                {
                    CacheEntry Item = Items[I];
                    if (!CheckValid(Item, CurrentTime))
                    {
                        Item.Invalid = true;
                        EntriesToRemove.Add(Item);
                        --CurrentCount;
                    }
                    else
                    {
                        switch (Item.Priority)
                        {
                            case CachePriority.Low:
                            {
                                LowPriorty.Add(Item);
                                break;
                            }

                            case CachePriority.Normal:
                            {
                                NormalPriorty.Add(Item);
                                break;
                            }

                            case CachePriority.High:
                            {
                                HighPriorty.Add(Item);
                                break;
                            }
                        }
                    }
                }
                CurrentCount = ExpirePriorityBucket(CurrentCount, TargetCount, EntriesToRemove, LowPriorty);
                CurrentCount = ExpirePriorityBucket(CurrentCount, TargetCount, EntriesToRemove, NormalPriorty);
                CurrentCount = ExpirePriorityBucket(CurrentCount, TargetCount, EntriesToRemove, HighPriorty);
                foreach (CacheEntry Item in EntriesToRemove)
                {
                    _CurrentSize -= Item.Size ?? 0;
                    _ = InternalCache.Remove(Item.Key);
                    EvictionCallback(Item.Key, Item.Value, EvictionReason.Expired, this);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (InternalCache is null)
                return;
            foreach (CacheEntry Item in InternalCache.Values)
            {
                Item.Dispose();
            }
            InternalCache.Clear();
            InternalCache = null;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value sent in.</returns>
        public override TValue Set<TValue>(object key, TValue value)
        {
            if (InternalCache is null)
                return value;
            DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                CacheEntry Entry = GetOrCreateEntry(key, value, UtcNow);
            }
            ScanForItemsToRemove(UtcNow);
            return value;
        }

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <returns>The value sent in.</returns>
        public override TValue Set<TValue>(object key, TValue value, DateTimeOffset absoluteExpiration)
        {
            if (InternalCache is null)
                return value;
            DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                CacheEntry Entry = GetOrCreateEntry(key, value, UtcNow);
                Entry.AbsoluteExpiration = absoluteExpiration;
            }
            ScanForItemsToRemove(UtcNow);
            return value;
        }

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirationRelativeToNow">The expiration relative to now.</param>
        /// <param name="sliding">if set to <c>true</c> [sliding] expiration.</param>
        /// <returns>The value sent in.</returns>
        public override TValue Set<TValue>(object key, TValue value, TimeSpan expirationRelativeToNow, bool sliding = false)
        {
            if (InternalCache is null)
                return value;
            DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                CacheEntry Entry = GetOrCreateEntry(key, value, UtcNow);
                if (sliding)
                    Entry.SlidingExpiration = expirationRelativeToNow;
                else
                    Entry.AbsoluteExpiration = UtcNow + expirationRelativeToNow;
            }
            ScanForItemsToRemove(UtcNow);
            return value;
        }

        /// <summary>
        /// Tries to get the value based on the key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if it is successful, false otherwise</returns>
        public override bool TryGetValue<TValue>(object key, out TValue value)
        {
            if (InternalCache is null)
            {
                value = default!;
                return false;
            }
            bool ReturnValue;
            DateTimeOffset CurrentTime = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                ReturnValue = InternalCache.TryGetValue(key.GetHashCode(), out CacheEntry? TempValue);

                if (ReturnValue)
                {
                    if (CheckValid(TempValue, CurrentTime) && TempValue is not null)
                    {
                        value = (TValue)TempValue.Value!;
                        TempValue.LastAccessed = CurrentTime;
                    }
                    else
                    {
                        ReturnValue = false;
                        value = default!;
                    }
                }
                else
                {
                    value = default!;
                }
            }
            ScanForItemsToRemove(CurrentTime);
            return ReturnValue;
        }

        /// <summary>
        /// Removes the items by the key.
        /// </summary>
        /// <param name="key">The key.</param>
        protected override void RemoveByKey(object key)
        {
            if (InternalCache is null)
                return;
            DateTimeOffset CurrentTime = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                var HashKey = key.GetHashCode();
                if (InternalCache.TryGetValue(HashKey, out CacheEntry? Current))
                {
                    _ = InternalCache.Remove(HashKey);
                    _CurrentSize -= Current.Size ?? 0;
                }
            }
            ScanForItemsToRemove(CurrentTime);
        }

        /// <summary>
        /// Sets the value with the options sent in.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheEntryOptions">The cache entry options.</param>
        /// <returns>The value sent in.</returns>
        protected override TValue SetWithOptions<TValue>(object key, TValue value, CacheEntryOptions cacheEntryOptions)
        {
            if (InternalCache is null)
                return value;
            var ExceedsSize = Options.MaxCacheSize.HasValue && cacheEntryOptions.Size.HasValue && cacheEntryOptions.Size + _CurrentSize > Options.MaxCacheSize;
            DateTimeOffset UtcNow = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                CacheEntry Entry = GetOrCreateEntry(key, value, UtcNow);
                if (cacheEntryOptions.AbsoluteExpiration.HasValue)
                    Entry.AbsoluteExpiration = cacheEntryOptions.AbsoluteExpiration;
                if (cacheEntryOptions.AbsoluteExpirationRelativeToNow.HasValue)
                    Entry.AbsoluteExpiration = UtcNow + cacheEntryOptions.AbsoluteExpirationRelativeToNow;
                if (cacheEntryOptions.SlidingExpiration.HasValue)
                    Entry.SlidingExpiration = cacheEntryOptions.SlidingExpiration;
                if (cacheEntryOptions.Size.HasValue)
                {
                    Entry.Size = cacheEntryOptions.Size;
                    _CurrentSize += Entry.Size.Value;
                }
                Entry.Priority = cacheEntryOptions.Priority;
            }
            ScanForItemsToRemove(UtcNow);
            if (ExceedsSize)
            {
                Compact(Options.CompactionPercentage ?? 0.1);
            }
            return value;
        }

        /// <summary>
        /// Checks to see if the entry is still valid.
        /// </summary>
        /// <param name="tempValue">The temporary value.</param>
        /// <param name="currentTime">The current time.</param>
        /// <returns>True if it is, false otherwise.</returns>
        private static bool CheckValid(CacheEntry? tempValue, DateTimeOffset currentTime)
        {
            if (tempValue is null || tempValue.Invalid)
            {
                return false;
            }

            if (!tempValue.AbsoluteExpiration.HasValue && !tempValue.SlidingExpiration.HasValue)
            {
                return true;
            }

            if (tempValue.AbsoluteExpiration.HasValue && tempValue.AbsoluteExpiration <= currentTime)
            {
                tempValue.Invalid = true;
                return false;
            }
            if (tempValue.SlidingExpiration.HasValue && tempValue.LastAccessed + tempValue.SlidingExpiration <= currentTime)
            {
                tempValue.Invalid = true;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Expires the priority bucket.
        /// </summary>
        /// <param name="currentCount">The current count.</param>
        /// <param name="targetCount">The target count.</param>
        /// <param name="entriesToRemove">The entries to remove.</param>
        /// <param name="bucket">The bucket.</param>
        /// <returns>The number of items left.</returns>
        private static int ExpirePriorityBucket(int currentCount, int targetCount, List<CacheEntry> entriesToRemove, List<CacheEntry> bucket)
        {
            if (targetCount >= currentCount)
                return currentCount;
            foreach (CacheEntry? Item in bucket.Where(x => x.AbsoluteExpiration.HasValue && !x.Invalid).OrderBy(x => x.AbsoluteExpiration))
            {
                Item.Invalid = true;
                entriesToRemove.Add(Item);
                --currentCount;
                if (targetCount >= currentCount)
                    return currentCount;
            }
            foreach (CacheEntry? Item in bucket.Where(x => x.SlidingExpiration.HasValue && !x.Invalid).OrderBy(x => x.SlidingExpiration))
            {
                Item.Invalid = true;
                entriesToRemove.Add(Item);
                --currentCount;
                if (targetCount >= currentCount)
                    return currentCount;
            }
            foreach (CacheEntry? Item in bucket.Where(x => !x.Invalid).OrderBy(x => x.LastAccessed))
            {
                Item.Invalid = true;
                entriesToRemove.Add(Item);
                --currentCount;
                if (targetCount >= currentCount)
                    return currentCount;
            }
            return currentCount;
        }

        /// <summary>
        /// Gets or creates the entry.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="currentTime">The current time.</param>
        /// <returns>The entry</returns>
        private CacheEntry GetOrCreateEntry<TValue>(object key, TValue value, DateTimeOffset currentTime)
        {
            if (InternalCache is null)
                return CacheEntry.Empty;
            var HashKey = key.GetHashCode();
            if (InternalCache.TryGetValue(HashKey, out CacheEntry? Current))
            {
                if (Current.Value is IDisposable Disposable)
                    Disposable.Dispose();
                Current.LastAccessed = currentTime;
                Current.AbsoluteExpiration = null;
                Current.SlidingExpiration = null;
                Current.Invalid = false;
                Current.Size = 0;
            }
            else
            {
                Current = new CacheEntry { LastAccessed = currentTime };
                InternalCache[HashKey] = Current;
            }
            Current.Key = HashKey;
            Current.Value = value;
            return Current;
        }

        /// <summary>
        /// Scans for items to remove.
        /// </summary>
        private void ScanForItemsToRemove(DateTimeOffset currentTime)
        {
            if (_LastScan + Options.ScanFrequency > currentTime)
            {
                return;
            }

            _LastScan = currentTime;
            _ = Task.Factory.StartNew(state => ScanForItemsToRemove((InMemoryCache?)state), this, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        /// <summary>
        /// Scans for items to remove.
        /// </summary>
        /// <param name="cache">The state.</param>
        private void ScanForItemsToRemove(InMemoryCache? cache)
        {
            if (cache?.InternalCache is null)
                return;
            DateTimeOffset CurrentTime = DateTimeOffset.UtcNow;
            lock (_LockObject)
            {
                CacheEntry[] Items = [.. cache.InternalCache.Values.Where(x => !CheckValid(x, CurrentTime))];
                for (var I = 0; I < Items.Length; I++)
                {
                    CacheEntry Item = Items[I];
                    if (Item.Value is IDisposable Disposable)
                    {
                        Disposable.Dispose();
                    }
                    cache._CurrentSize -= Item.Size ?? 0;
                    _ = cache.InternalCache.Remove(Item.Key);
                    cache.EvictionCallback(Item.Key, Item.Value, EvictionReason.Expired, cache);
                }
            }
        }
    }
}