using System;

namespace Shields.Graphs
{
    /// <summary>
    /// Provides methods for constructing heuristic functions.
    /// </summary>
    public static class Heuristic
    {
        /// <summary>
        /// Creates a heuristic function.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <param name="evaluate">The function that maps a node to its optimistic cost estimate.</param>
        /// <returns>The heuristic function.</returns>
        public static IHeuristic<TNode> Create<TNode>(Func<TNode, double> evaluate)
        {
            return Create(evaluate, false);
        }

        /// <summary>
        /// Creates a heuristic function.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <param name="evaluate">The function that maps a node to its optimistic estimate.</param>
        /// <param name="isConsistent">Does the heuristic function satisfy the triangle inequality?</param>
        /// <returns>The heuristic function.</returns>
        public static IHeuristic<TNode> Create<TNode>(Func<TNode, double> evaluate, bool isConsistent)
        {
            return new FunctionalHeuristic<TNode>(evaluate, isConsistent);
        }

        /// <summary>
        /// For the heuristic function h(n), returns the relaxed heuristic function h'(n) = (1 + amount) * h(n).
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <param name="heuristic">The heuristic function.</param>
        /// <param name="amount">The amount to relax by.</param>
        /// <returns>The relaxed heuristic function.</returns>
        public static IHeuristic<TNode> Relax<TNode>(this IHeuristic<TNode> heuristic, double amount)
        {
            return Relax(heuristic, amount, true);
        }

        /// <summary>
        /// For the heuristic function h(n), returns the relaxed heuristic function h'(n) = (1 + amount) * h(n).
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <param name="heuristic">The heuristic function.</param>
        /// <param name="amount">The amount to relax by.</param>
        /// <param name="maintainConsistency">
        /// Should the consistency of the heuristic be maintained?
        /// Defaults to true since this is usually the desired behavior, even though theoretically this is not always the case.
        /// </param>
        /// <returns>The relaxed heuristic function.</returns>
        public static IHeuristic<TNode> Relax<TNode>(this IHeuristic<TNode> heuristic, double amount, bool maintainConsistency)
        {
            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException("amount", "Must be nonnegative.");
            }
            amount += 1;
            return Create<TNode>(x => amount * heuristic.Evaluate(x), maintainConsistency && heuristic.IsConsistent);
        }
    }
}
