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
using System;
using System.Collections.Generic;
using System.Linq;

namespace DragonHoard.Core
{
    /// <summary>
    /// Cache manager class
    /// </summary>
    /// <seealso cref="IDisposable"/>
    public class Cache : IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Cache"/> class.
        /// </summary>
        /// <param name="caches">The caches.</param>
        /// <exception cref="ArgumentException">
        /// No caches were found in the system. Please register one prior to initializing.
        /// </exception>
        public Cache(IEnumerable<ICache> caches)
        {
            caches ??= Array.Empty<ICache>();
            if (!caches.Any())
                throw new ArgumentException("No caches were found in the system. Please register one prior to initializing.");
            var CacheAssembly = typeof(Cache).Assembly;
            Caches = caches.Where(x => x.GetType().Assembly != CacheAssembly).ToDictionary(x => GetKey(x.Name));
            var DefaultKey = GetKey("Default");
            if (!Caches.TryGetValue(DefaultKey, out var TempCache))
            {
                Caches.Add(DefaultKey, caches.First().Clone());
            }
        }

        /// <summary>
        /// Caches
        /// </summary>
        private Dictionary<int, ICache>? Caches { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Gets the or add cache.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public ICache? GetOrAddCache(string name = "Default")
        {
            if (Caches is null)
                return null;

            var CacheKey = GetKey(name);
            if (Caches.TryGetValue(CacheKey, out var ReturnValue))
                return ReturnValue;

            var DefaultKey = GetKey("Default");
            Caches.TryGetValue(DefaultKey, out ReturnValue);
            ReturnValue = ReturnValue.Clone();
            if (ReturnValue is null)
                return null;

            Caches.Add(CacheKey, ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing">
        /// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release
        /// only unmanaged resources.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            if (Caches is null)
                return;
            foreach (var Cache in Caches)
            {
                Cache.Value.Dispose();
            }
            Caches = null;
        }

        /// <summary>
        /// Gets the key.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The hashed key.</returns>
        private static int GetKey(string? value)
        {
            return value?.GetHashCode(StringComparison.Ordinal) ?? 0;
        }
    }
}