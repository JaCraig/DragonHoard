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

using DragonHoard.Core.BaseClasses;
using DragonHoard.Core.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;

namespace DragonHoard.MicrosoftExtensionsCachingMemory
{
    /// <summary>
    /// In memory cache
    /// </summary>
    /// <seealso cref="ICache"/>
    public class MemoryCache : CacheBaseClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCache"/> class.
        /// </summary>
        /// <param name="memoryCache">The memory cache.</param>
        public MemoryCache(IMemoryCache memoryCache)
        {
            InternalCache = memoryCache;
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name { get; } = "Microsoft.Extensions.Caching.Memory";

        /// <summary>
        /// Gets the internal cache.
        /// </summary>
        /// <value>The internal cache.</value>
        private IMemoryCache? InternalCache { get; set; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this cache.</returns>
        public override ICache Clone()
        {
            return new MemoryCache(InternalCache);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (InternalCache is null)
                return;
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
            var Options = new MemoryCacheEntryOptions();
            Options.RegisterPostEvictionCallback(EvictionCallback);
            InternalCache.Set(key, value, Options);
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
            var Options = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = absoluteExpiration
            };
            Options.RegisterPostEvictionCallback(EvictionCallback);
            InternalCache.Set(key, value, Options);
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
            var Options = new MemoryCacheEntryOptions();
            Options.RegisterPostEvictionCallback(EvictionCallback);
            if (sliding)
                Options.SlidingExpiration = expirationRelativeToNow;
            else
                Options.AbsoluteExpirationRelativeToNow = expirationRelativeToNow;
            InternalCache.Set(key, value, Options);
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
            return InternalCache.TryGetValue(key, out value);
        }

        /// <summary>
        /// Removes the items by the key.
        /// </summary>
        /// <param name="key">The key.</param>
        protected override void RemoveByKey(object key)
        {
            if (InternalCache is null)
                return;
            InternalCache.Remove(key);
        }
    }
}