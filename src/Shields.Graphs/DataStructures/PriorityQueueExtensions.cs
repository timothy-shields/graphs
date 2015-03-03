using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs.DataStructures
{
    public static class PriorityQueueExtensions
    {
        public static bool TryDecreaseKey<TKey, TValue, THandle>(
            this IPriorityQueue<TKey, TValue, THandle> priorityQueue,
            THandle handle, TKey key)
            where THandle : IHandle<TKey, TValue>
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
