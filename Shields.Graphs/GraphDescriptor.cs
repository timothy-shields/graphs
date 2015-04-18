using System;
using System.Collections.Generic;

namespace Shields.Graphs
{
    /// <summary>
    /// Provides methods for constructing graph descriptors.
    /// </summary>
    public static class GraphDescriptor
    {
        public static IGraphDescriptor<TNode, TKey> Create<TNode, TKey>(Func<TNode, TKey> key, Func<TNode, IEnumerable<TNode>> next)
        {
            return new FunctionalGraphDescriptor<TNode, TKey>(key, next);
        }
    }
}
