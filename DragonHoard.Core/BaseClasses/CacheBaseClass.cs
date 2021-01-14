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

using DragonHoard.Core.Interfaces;
using DragonHoard.Core.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;

namespace DragonHoard.Core.BaseClasses
{
    /// <summary>
    /// Cache base class
    /// </summary>
    public abstract class CacheBaseClass : ICache
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheBaseClass"/> class.
        /// </summary>
        protected CacheBaseClass()
        {
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public abstract string Name { get; }

        /// <summary>
        /// Gets the index of the tags.
        /// </summary>
        /// <value>The index of the tags.</value>
        private ManyToManyIndex TagIndex { get; } = new ManyToManyIndex();

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this cache.</returns>
        public abstract ICache Clone();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <typeparam name="TOption">The type of the option.</typeparam>
        /// <param name="options">The options to use for the cache.</param>
        /// <returns>A copy of this cache.</returns>
        public abstract ICache Clone<TOption>(TOption options);

        /// <summary>
        /// Compacts the cache by the specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        public abstract void Compact(double percentage);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Gets the by tag.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="tag">The tag.</param>
        /// <returns>The IEnumerable of items associated with the tag.</returns>
        public TValue[] GetByTag<TValue>(string tag)
        {
            if (tag is null)
                return Array.Empty<TValue>();
            lock (LockObject)
            {
                if (!TagIndex.TryGetValue(tag.GetHashCode(StringComparison.Ordinal), out var Keys))
                    return Array.Empty<TValue>();

                var ReturnValues = new TValue[Keys.Length];
                for (int i = 0; i < Keys.Length; i++)
                {
                    if (TryGetValue<TValue>(Keys[i], out var ReturnValue))
                        ReturnValues[i] = ReturnValue;
                }
                return ReturnValues;
            }
        }

        /// <summary>
        /// Removes the object associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        public void Remove(object key)
        {
            lock (LockObject)
            {
                TagIndex.Remove(key);
                RemoveByKey(key);
            }
        }

        /// <summary>
        /// Removes the by tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public void RemoveByTag(string tag)
        {
            if (tag is null)
                return;
            var TagHashCode = tag.GetHashCode(StringComparison.Ordinal);
            if (!TagIndex.TryGetValue(TagHashCode, out _))
                return;
            lock (LockObject)
            {
                if (!TagIndex.TryGetValue(TagHashCode, out var Keys))
                    return;
                for (int i = 0; i < Keys.Length; i++)
                {
                    var Key = Keys[i];
                    RemoveByKey(Key);
                    TagIndex.Remove(Key);
                }
                TagIndex.Remove(TagHashCode);
            }
        }

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value sent in.</returns>
        public abstract TValue Set<TValue>(object key, TValue value);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <returns>The value sent in.</returns>
        public abstract TValue Set<TValue>(object key, TValue value, DateTimeOffset absoluteExpiration);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirationRelativeToNow">The expiration relative to now.</param>
        /// <param name="sliding">if set to <c>true</c> [sliding] expiration.</param>
        /// <returns>The value sent in.</returns>
        public abstract TValue Set<TValue>(object key, TValue value, TimeSpan expirationRelativeToNow, bool sliding = false);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheEntryOptions">The cache entry options.</param>
        /// <returns>The value sent in.</returns>
        public TValue Set<TValue>(object key, TValue value, CacheEntryOptions cacheEntryOptions)
        {
            cacheEntryOptions.Tags ??= Array.Empty<string>();
            lock (LockObject)
            {
                if (cacheEntryOptions.Tags.Length > 0)
                {
                    TagIndex.Remove(key);
                    TagIndex.Add(key, cacheEntryOptions.Tags.Select(tag => tag.GetHashCode(StringComparison.Ordinal)).ToArray());
                }
                return SetWithOptions(key, value, cacheEntryOptions);
            }
        }

        /// <summary>
        /// Tries to get the value based on the key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if it is successful, false otherwise</returns>
        public abstract bool TryGetValue<TValue>(object key, out TValue value);

        /// <summary>
        /// Called when an item is removed from the index.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="reason">The reason.</param>
        /// <param name="state">The state.</param>
        protected void EvictionCallback(object key, object? value, EvictionReason reason, object state)
        {
            if (EvictionReason.Expired == reason || reason == EvictionReason.Capacity || reason == EvictionReason.TokenExpired)
                TagIndex.Remove(key);
        }

        /// <summary>
        /// Removes the items by the key.
        /// </summary>
        /// <param name="key">The key.</param>
        protected abstract void RemoveByKey(object key);

        /// <summary>
        /// Sets the value with the options sent in.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheEntryOptions">The cache entry options.</param>
        /// <returns>The value sent in.</returns>
        protected abstract TValue SetWithOptions<TValue>(object key, TValue value, CacheEntryOptions cacheEntryOptions);
    }
}