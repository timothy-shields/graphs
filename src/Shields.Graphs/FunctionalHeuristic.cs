using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    public class FunctionalHeuristic<T> : IHeuristic<T>
    {
        private readonly Func<T, double> evaluate;

        public FunctionalHeuristic(Func<T, double> evaluate, bool isConsistent)
        {
            this.evaluate = evaluate;
            this.IsConsistent = isConsistent;
        }

        public double Evaluate(T node)
        {
            return evaluate(node);
        }

        public bool IsConsistent { get; private set; }
    }
}
