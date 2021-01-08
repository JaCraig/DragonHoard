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
using System;
using System.Collections.Generic;
using System.Linq;

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
        public InMemoryCache()
        {
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public override string Name { get; } = "In Memory";

        /// <summary>
        /// Internal cache
        /// </summary>
        private Dictionary<int, object>? InternalCache { get; set; } = new Dictionary<int, object>();

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this cache.</returns>
        public override ICache Clone()
        {
            return new InMemoryCache();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            if (InternalCache is null)
                return;
            foreach (var Item in InternalCache.Values.OfType<IDisposable>())
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
            lock (LockObject)
            {
                InternalCache[key.GetHashCode()] = value;
            }
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
            return Set(key, value);
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
            return Set(key, value);
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
            lock (LockObject)
            {
                ReturnValue = InternalCache.TryGetValue(key.GetHashCode(), out var TempValue);
                value = (TValue)TempValue;
            }
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
            lock (LockObject)
            {
                InternalCache.Remove(key.GetHashCode());
            }
        }
    }
}