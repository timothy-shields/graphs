using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shields.Graphs.DataStructures
{
    public interface IPriorityQueue<TKey, TValue, THandle>
        where THandle : IHandle<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        THandle Insert(TKey key, TValue value);
        void Delete(THandle handle);
        void DecreaseKey(THandle handle, TKey key);
        THandle GetMin();
        THandle ExtractMin();
    }
}
