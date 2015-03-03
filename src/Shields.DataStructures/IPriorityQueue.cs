using System;

namespace Shields.DataStructures
{
    public interface IPriorityQueue<TKey, TValue, THandle>
        where THandle : IKeyValueHandle<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        THandle GetMin();
        THandle Insert(TKey key, TValue value);
        void Remove(THandle handle);
        void DecreaseKey(THandle handle, TKey key);
    }
}
