using System;

namespace Shields.Graphs
{
    /// <summary>
    /// A functional implementation of <see cref="IHeuristic&lt;TNode&gt;"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of a node.</typeparam>
    /// <typeparam name="TKey">The type of a node key.</typeparam>
    internal class FunctionalHeuristic<TNode> : IHeuristic<TNode>
    {
        private readonly Func<TNode, double> evaluate;

        public FunctionalHeuristic(Func<TNode, double> evaluate, bool isConsistent)
        {
            this.evaluate = evaluate;
            this.IsConsistent = isConsistent;
        }

        public double Evaluate(TNode node)
        {
            return evaluate(node);
        }

        public bool IsConsistent { get; private set; }
    }
}
