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

namespace DragonHoard.Core.Utils
{
    /// <summary>
    /// Two way, many to many index
    /// </summary>
    internal class ManyToManyIndex
    {
        /// <summary>
        /// Gets the second mapping.
        /// </summary>
        /// <value>The second mapping.</value>
        private ListMapping<object, int> KeyToTagMapping { get; } = new ListMapping<object, int>();

        /// <summary>
        /// Gets the first mapping.
        /// </summary>
        /// <value>The first mapping.</value>
        private ListMapping<int, object> TagToKeyMapping { get; } = new ListMapping<int, object>();

        /// <summary>
        /// Adds the specified data to the mapping
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="list">The list.</param>
        public void Add(object key, params int[] list)
        {
            list ??= Array.Empty<int>();
            KeyToTagMapping.Replace(key, list);
            for (int i = 0; i < list.Length; i++)
            {
                TagToKeyMapping.Add(list[i], key);
            }
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if it is removed, false otherwise</returns>
        public bool Remove(int key)
        {
            if (!TagToKeyMapping.TryGetValue(key, out var List))
                return false;
            for (int i = 0; i < List.Length; i++)
            {
                KeyToTagMapping.Remove(List[i], key);
            }
            TagToKeyMapping.Remove(key);
            return true;
        }

        /// <summary>
        /// Removes the specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if it is removed, false otherwise</returns>
        public bool Remove(object key)
        {
            if (!KeyToTagMapping.TryGetValue(key, out var List))
                return false;
            for (int i = 0; i < List.Length; i++)
            {
                TagToKeyMapping.Remove(List[i], key);
            }
            KeyToTagMapping.Remove(key);
            return true;
        }

        /// <summary>
        /// Tries to get the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="values">The values.</param>
        /// <returns>True if it is returned, false otherwise.</returns>
        public bool TryGetValue(int key, out object[] values)
        {
            return TagToKeyMapping.TryGetValue(key, out values);
        }
    }
}