using System;

namespace Shields.DataStructures
{
    public static class PriorityQueueExtensions
    {
        public static THandle ExtractMin<TKey, TValue, THandle>(
            this IPriorityQueue<TKey, TValue, THandle> priorityQueue)
            where THandle : IKeyValueHandle<TKey, TValue>
            where TKey : IComparable<TKey>
        {
            var handle = priorityQueue.GetMin();
            priorityQueue.Remove(handle);
            return handle;
        }

        public static bool TryDecreaseKey<TKey, TValue, THandle>(
            this IPriorityQueue<TKey, TValue, THandle> priorityQueue,
            THandle handle, TKey key)
            where THandle : IKeyValueHandle<TKey, TValue>
            where TKey : IComparable<TKey>
        {
            if (key.CompareTo(handle.Key) < 0)
            {
                priorityQueue.DecreaseKey(handle, key);
                return true;
            }
            return false;
        }
    }
}
