using System;

namespace DragonHoard.InMemory
{
    /// <summary>
    /// Cache entry
    /// </summary>
    internal class CacheEntry : IDisposable
    {
        /// <summary>
        /// Gets the absolute expiration relative to now.
        /// </summary>
        /// <value>The absolute expiration relative to now.</value>
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; }

        /// <summary>
        /// Gets the last accessed.
        /// </summary>
        /// <value>The last accessed.</value>
        public DateTimeOffset LastAccessed { get; set; }

        /// <summary>
        /// Gets the size.
        /// </summary>
        /// <value>The size.</value>
        public long? Size { get; set; }

        /// <summary>
        /// Gets the sliding expiration.
        /// </summary>
        /// <value>The sliding expiration.</value>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>The value.</value>
        public object? Value { get; set; }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting
        /// unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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
            if (Value is null)
                return;
            if (Value is IDisposable disposable)
                disposable.Dispose();
            Value = null;
        }
    }
}