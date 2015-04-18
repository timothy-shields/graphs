
namespace Shields.Graphs
{
    /// <summary>
    /// An object that represents a heuristic function.
    /// </summary>
    /// <typeparam name="TNode">The type of a node.</typeparam>
    public interface IHeuristic<TNode>
    {
        /// <summary>
        /// Evaluates the heuristic function at a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The heuristic function value at the node.</returns>
        double Evaluate(TNode node);

        /// <summary>
        /// Gets whether the heuristic is consistent.
        /// </summary>
        bool IsConsistent { get; }
    }
}
