using System;
using System.Collections.Generic;

namespace DragonHoard.Core.Utils
{
    /// <summary>
    /// Maps a key to a list of data
    /// </summary>
    internal class ListMapping<TKey, TValue>
    {
        /// <summary>
        /// Container holding the data
        /// </summary>
        private Dictionary<TKey, List<TValue>> Items { get; } = new Dictionary<TKey, List<TValue>>();

        /// <summary>
        /// The lock object
        /// </summary>
        private readonly object LockObject = new object();

        /// <summary>
        /// Adds an item to the mapping
        /// </summary>
        /// <param name="key">Key value</param>
        /// <param name="values">The values.</param>
        public void Add(TKey key, params TValue[] values)
        {
            values ??= Array.Empty<TValue>();
            if (values.Length == 0)
                return;
            lock (LockObject)
            {
                if (!Items.TryGetValue(key, out var ReturnValues))
                {
                    ReturnValues = new List<TValue>();
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
            lock (LockObject)
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
            lock (LockObject)
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
            values ??= Array.Empty<TValue>();
            if (values.Length == 0)
                return false;
            lock (LockObject)
            {
                if (!Items.TryGetValue(key, out var TempItems))
                    return false;
                var ReturnValue = false;
                for (var x = 0; x < values.Length; ++x)
                {
                    ReturnValue |= TempItems.Remove(values[x]);
                }
                if (TempItems.Count == 0)
                    Items.Remove(key);
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
            values ??= Array.Empty<TValue>();
            if (values.Length == 0)
                return;
            lock (LockObject)
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
            lock (LockObject)
            {
                if (Items.TryGetValue(key, out var TempValue))
                {
                    value = TempValue.ToArray();
                    return true;
                }
                value = Array.Empty<TValue>();
                return false;
            }
        }
    }
}