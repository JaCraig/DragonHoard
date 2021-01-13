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
        /// Initializes a new instance of the <see cref="InMemoryCache"/> class.
        /// </summary>
        /// <param name="memoryCache">The memory cache.</param>
        public InMemoryCache(IEnumerable<IOptions<InMemoryCacheOptions>> options)
        {
            Options = (options.FirstOrDefault() ?? InMemoryCacheOptions.Default);
            if (Options.Value.ScanFrequency == default)
                Options = InMemoryCacheOptions.Default;
            LastScan = DateTimeOffset.UtcNow;
            ScanFrequency = Options.Value.ScanFrequency;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name { get; } = "In Memory";

        /// <summary>
        /// Internal cache
        /// </summary>
        private Dictionary<int, CacheEntry>? InternalCache { get; set; } = new Dictionary<int, CacheEntry>();

        /// <summary>
        /// Gets the options.
        /// </summary>
        /// <value>The options.</value>
        private IOptions<InMemoryCacheOptions> Options { get; }

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// Gets or sets the last scan.
        /// </summary>
        /// <value>The last scan.</value>
        private DateTimeOffset LastScan;

        /// <summary>
        /// The scan frequency
        /// </summary>
        private TimeSpan ScanFrequency;

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this cache.</returns>
        public override ICache Clone()
        {
            return new InMemoryCache(new IOptions<InMemoryCacheOptions>[] { Options });
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (InternalCache is null)
                return;
            foreach (var Item in InternalCache.Values)
            {
                Item.Dispose();
            }
            InternalCache.Clear();
            InternalCache = null;
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
            var UTCNow = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                var entry = GetOrCreateEntry(key, value, UTCNow);
            }
            ScanForItemsToRemove(UTCNow);
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
            var UTCNow = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                var entry = GetOrCreateEntry(key, value, UTCNow);
                entry.AbsoluteExpiration = absoluteExpiration;
            }
            ScanForItemsToRemove(UTCNow);
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
            var UTCNow = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                var entry = GetOrCreateEntry(key, value, UTCNow);
                if (sliding)
                    entry.SlidingExpiration = expirationRelativeToNow;
                else
                    entry.AbsoluteExpiration = UTCNow + expirationRelativeToNow;
            }
            ScanForItemsToRemove(UTCNow);
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
                value = default;
                return false;
            }
            bool ReturnValue;
            var CurrentTime = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                ReturnValue = InternalCache.TryGetValue(key.GetHashCode(), out var TempValue);

                if (ReturnValue)
                {
                    if (CheckValid(TempValue, CurrentTime))
                    {
                        value = (TValue)TempValue.Value;
                        TempValue.LastAccessed = CurrentTime;
                    }
                    else
                    {
                        ReturnValue = false;
                        value = default;
                    }
                }
                else
                    value = default;
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
            var CurrentTime = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                var HashKey = key.GetHashCode();
                if (InternalCache.TryGetValue(HashKey, out var current))
                {
                    InternalCache.Remove(HashKey);
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
            var UTCNow = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                var entry = GetOrCreateEntry(key, value, UTCNow);
                if (cacheEntryOptions.AbsoluteExpiration.HasValue)
                    entry.AbsoluteExpiration = cacheEntryOptions.AbsoluteExpiration;
                if (cacheEntryOptions.AbsoluteExpirationRelativeToNow.HasValue)
                    entry.AbsoluteExpiration = UTCNow + cacheEntryOptions.AbsoluteExpirationRelativeToNow;
                if (cacheEntryOptions.SlidingExpiration.HasValue)
                    entry.SlidingExpiration = cacheEntryOptions.SlidingExpiration;
            }
            ScanForItemsToRemove(UTCNow);
            return value;
        }

        /// <summary>
        /// Checks to see if the entry is still valid.
        /// </summary>
        /// <param name="tempValue">The temporary value.</param>
        /// <param name="currentTime">The current time.</param>
        /// <returns>True if it is, false otherwise.</returns>
        private bool CheckValid(CacheEntry tempValue, DateTimeOffset currentTime)
        {
            if (tempValue.Invalid)
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
        /// Gets or creates the entry.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="currentTime">The current time.</param>
        /// <returns>The entry</returns>
        private CacheEntry GetOrCreateEntry<TValue>(object key, TValue value, DateTimeOffset currentTime)
        {
            var HashKey = key.GetHashCode();
            if (InternalCache.TryGetValue(HashKey, out var current))
            {
                if (current.Value is IDisposable disposable)
                    disposable.Dispose();
                current.LastAccessed = currentTime;
                current.AbsoluteExpiration = null;
                current.SlidingExpiration = null;
                current.Invalid = false;
                current.Size = 0;
            }
            else
            {
                current = new CacheEntry { LastAccessed = currentTime };
                InternalCache[HashKey] = current;
            }
            current.Key = HashKey;
            current.Value = value;
            return current;
        }

        /// <summary>
        /// Scans for items to remove.
        /// </summary>
        private void ScanForItemsToRemove(DateTimeOffset currentTime)
        {
            if (LastScan + ScanFrequency > currentTime)
                return;
            LastScan = currentTime;
            Task.Factory.StartNew(state => ScanForItemsToRemove((InMemoryCache)state), this, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
        }

        /// <summary>
        /// Scans for items to remove.
        /// </summary>
        /// <param name="cache">The state.</param>
        private void ScanForItemsToRemove(InMemoryCache cache)
        {
            var CurrentTime = DateTimeOffset.UtcNow;
            lock (LockObject)
            {
                var Items = cache.InternalCache.Values.Where(x => !CheckValid(x, CurrentTime)).ToArray();
                for (int i = 0; i < Items.Length; i++)
                {
                    var Item = Items[i];
                    if (Item.Value is IDisposable disposable)
                    {
                        disposable.Dispose();
                    }
                    cache.InternalCache.Remove(Item.Key);
                    cache.EvictionCallback(Item.Key, Item.Value, EvictionReason.Expired, cache);
                }
            }
        }
    }
}