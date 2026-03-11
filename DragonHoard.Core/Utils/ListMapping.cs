using System.Collections.Generic;

namespace DragonHoard.Core.Utils
{
    /// <summary>
    /// Maps a key to a list of data
    /// </summary>
    internal class ListMapping<TKey, TValue>
        where TKey : notnull
    {
        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object _LockObject = new();

        /// <summary>
        /// Container holding the data
        /// </summary>
        private Dictionary<TKey, List<TValue>> Items { get; } = [];

        /// <summary>
        /// Adds an item to the mapping
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="values">The values.</param>
        public void Add(TKey key, params TValue[] values)
        {
            values ??= [];
            if (values.Length == 0)
                return;
            lock (_LockObject)
            {
                if (!Items.TryGetValue(key, out List<TValue>? ReturnValues))
                {
                    ReturnValues = [];
                    Items.Add(key, ReturnValues);
                }
                ReturnValues.AddRange(values);
            }
        }

        /// <summary>
        /// Clears all items from the listing
        /// </summary>
        public void Clear()
        {
            lock (_LockObject)
            {
                Items.Clear();
            }
        }

        /// <summary>
        /// Remove a list of items associated with a key
        /// </summary>
        /// <param name="key">Key to use</param>
        /// <returns>True if the key is found, false otherwise</returns>
        public bool Remove(TKey key)
        {
            lock (_LockObject)
            {
                return Items.Remove(key);
            }
        }

        /// <summary>
        /// Removes a key value pair from the list mapping
        /// </summary>
        /// <param name="key">Key to remove</param>
        /// <param name="values">The values to remove.</param>
        /// <returns>True if it is removed, false otherwise</returns>
        public bool Remove(TKey key, params TValue[] values)
        {
            values ??= [];
            if (values.Length == 0)
                return false;
            lock (_LockObject)
            {
                if (!Items.TryGetValue(key, out List<TValue>? TempItems))
                    return false;
                var ReturnValue = false;
                for (var X = 0; X < values.Length; ++X)
                {
                    ReturnValue |= TempItems.Remove(values[X]);
                }
                if (TempItems.Count == 0)
                    _ = Items.Remove(key);
                return ReturnValue;
            }
        }

        /// <summary>
        /// Replaces the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        public void Replace(TKey key, params TValue[] values)
        {
            values ??= [];
            if (values.Length == 0)
                return;
            lock (_LockObject)
            {
                var ReturnValues = new List<TValue>();
                Items[key] = ReturnValues;
                ReturnValues.AddRange(values);
            }
        }

        /// <summary>
        /// Tries to get the value associated with the key
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="value">The values getting</param>
        /// <returns>True if it was able to get the value, false otherwise</returns>
        public bool TryGetValue(TKey key, out TValue[] value)
        {
            lock (_LockObject)
            {
                if (Items.TryGetValue(key, out List<TValue>? TempValue))
                {
                    value = [.. TempValue];
                    return true;
                }
                value = [];
                return false;
            }
        }
    }
}