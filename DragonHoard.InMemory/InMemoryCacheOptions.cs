using Microsoft.Extensions.Options;
using System;

namespace DragonHoard.InMemory
{
    /// <summary>
    /// In memory cache options
    /// </summary>
    /// <seealso cref="IOptions{InMemoryCacheOptions}"/>
    public class InMemoryCacheOptions : IOptions<InMemoryCacheOptions>
    {
        /// <summary>
        /// Gets the default.
        /// </summary>
        /// <value>The default.</value>
        public static InMemoryCacheOptions Default => new InMemoryCacheOptions
        {
            ScanFrequency = TimeSpan.FromMinutes(1)
        };

        /// <summary>
        /// Gets or sets the compaction percentage.
        /// </summary>
        /// <value>The compaction percentage.</value>
        public double? CompactionPercentage { get; set; }

        /// <summary>
        /// Gets or sets the maximum size.
        /// </summary>
        /// <value>The maximum size.</value>
        public long? MaxCacheSize { get; set; }

        /// <summary>
        /// Gets or sets the scan frequency.
        /// </summary>
        /// <value>The scan frequency.</value>
        public TimeSpan ScanFrequency { get; set; }

        /// <summary>
        /// Gets the default configured Options instance.
        /// </summary>
        public InMemoryCacheOptions Value => this;
    }
}