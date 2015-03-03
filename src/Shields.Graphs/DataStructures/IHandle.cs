using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shields.Graphs.DataStructures
{
    public interface IHandle<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        TKey Key { get; }
        TValue Value { get; }
        bool IsActive { get; }
    }
}
