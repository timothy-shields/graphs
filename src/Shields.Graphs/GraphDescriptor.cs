using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    public static class GraphDescriptor
    {
        public static IGraphDescriptor<T, K> Create<T, K>(Func<T, K> key, Func<T, IEnumerable<T>> next)
        {
            return new FunctionalGraphDescriptor<T, K>(key, next);
        }
    }
}
