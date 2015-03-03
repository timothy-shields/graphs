using System;

namespace Shields.DataStructures
{
    /// <summary>
    /// An object that has a key and a value.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    public interface IKeyValueHandle<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        /// <summary>
        /// The key.
        /// </summary>
        TKey Key { get; }

        /// <summary>
        /// The value.
        /// </summary>
        TValue Value { get; }

        /// <summary>
        /// Is this object currently in its containing data structure?
        /// </summary>
        bool IsActive { get; }
    }
}
