using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    /// <summary>
    /// Provides methods for constructing weighted graph descriptors.
    /// </summary>
    public static class WeightedGraphDescriptor
    {
        public static IWeightedGraphDescriptor<TNode, TKey> Create<TNode, TKey>(
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<IWeighted<TNode>>> next)
        {
            return new FunctionalWeightedGraphDescriptor<TNode, TKey>(key, next);
        }

        public static IGraphDescriptor<TNode, TKey> AsGraphDescriptor<TNode, TKey>(
            this IWeightedGraphDescriptor<TNode, TKey> descriptor)
        {
            return GraphDescriptor.Create<TNode, TKey>(descriptor.Key, u => descriptor.Next(u).Select(uv => uv.Value));
        }
    }
}
