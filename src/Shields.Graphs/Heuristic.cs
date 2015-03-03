using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    public static class Heuristic
    {
        public static IHeuristic<T> Create<T>(Func<T, double> evaluate, bool isConsistent)
        {
            return new FunctionalHeuristic<T>(evaluate, isConsistent);
        }
    }
}
