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

namespace DragonHoard.Core
{
    /// <summary>
    /// Cache entry options
    /// </summary>
    public class CacheEntryOptions
    {
        /// <summary>
        /// Gets or sets the absolute expiration.
        /// </summary>
        /// <value>The absolute expiration.</value>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets the absolute expiration relative to now.
        /// </summary>
        /// <value>The absolute expiration relative to now.</value>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Gets or sets the priority.
        /// </summary>
        /// <value>The priority.</value>
        public CachePriority Priority { get; set; }

        /// <summary>
        /// Gets or sets the size.
        /// </summary>
        /// <value>The size.</value>
        public long? Size { get; set; }

        /// <summary>
        /// Gets or sets the sliding expiration.
        /// </summary>
        /// <value>The sliding expiration.</value>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets the tags associated with this entry.
        /// </summary>
        /// <value>The tags associated with this entry.</value>
        public string[]? Tags { get; set; }
    }
}