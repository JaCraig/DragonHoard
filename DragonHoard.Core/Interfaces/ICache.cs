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

using System;

namespace DragonHoard.Core.Interfaces
{
    /// <summary>
    /// Cache interface
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public interface ICache : IDisposable
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        string Name { get; }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>A copy of this cache.</returns>
        ICache Clone();

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <typeparam name="TOption">The type of the option.</typeparam>
        /// <param name="options">The options to use for the cache.</param>
        /// <returns>A copy of this cache.</returns>
        ICache Clone<TOption>(TOption options);

        /// <summary>
        /// Compacts the cache by the specified percentage.
        /// </summary>
        /// <param name="percentage">The percentage.</param>
        void Compact(double percentage);

        /// <summary>
        /// Gets the by tag.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="tag">The tag.</param>
        /// <returns>The IEnumerable of items associated with the tag.</returns>
        TValue[] GetByTag<TValue>(string tag);

        /// <summary>
        /// Removes the object associated with the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        void Remove(object key);

        /// <summary>
        /// Removes the by tag.
        /// </summary>
        /// <param name="tag">The tag.</param>
        void RemoveByTag(string tag);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>The value sent in.</returns>
        TValue Set<TValue>(object key, TValue value);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="absoluteExpiration">The absolute expiration.</param>
        /// <returns>The value sent in.</returns>
        TValue Set<TValue>(object key, TValue value, DateTimeOffset absoluteExpiration);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="expirationRelativeToNow">The expiration relative to now.</param>
        /// <param name="sliding">if set to <c>true</c> [sliding] expiration.</param>
        /// <returns>The value sent in.</returns>
        TValue Set<TValue>(object key, TValue value, TimeSpan expirationRelativeToNow, bool sliding = false);

        /// <summary>
        /// Sets the specified key/value pair in the cache.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <param name="cacheEntryOptions">The cache entry options.</param>
        /// <returns>The value sent in.</returns>
        TValue Set<TValue>(object key, TValue value, CacheEntryOptions cacheEntryOptions);

        /// <summary>
        /// Tries to get the value based on the key.
        /// </summary>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>True if it is successful, false otherwise</returns>
        bool TryGetValue<TValue>(object key, out TValue value);
    }
}