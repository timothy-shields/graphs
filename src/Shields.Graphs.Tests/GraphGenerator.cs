using Combinatorics.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs.Tests
{
    public static class GraphGenerator
    {
        private static Func<T, IEnumerable<U>> Function<T, U>(Dictionary<T, IEnumerable<U>> lookup)
        {
            return key =>
            {
                IEnumerable<U> result;
                if (lookup.TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    return Enumerable.Empty<U>();
                }
            };
        }

        public static IEnumerable<IGraphDescriptor<T, T>> AllDirectedAcyclicGraphs<T>(IList<T> nodes)
        {
            var E = new Combinations<T>(nodes, 2).Where(e => e.Count == 2).ToList();
            foreach (var F in Enumerable.Range(0, E.Count + 1).SelectMany(k => new Combinations<IList<T>>(E, k)))
            {
                yield return GraphDescriptor.Create(u => u, Function(F.GroupBy(e => e[0]).ToDictionary(g => g.Key, g => g.Select(e => e[1]))));
            }
        }
    }
}
