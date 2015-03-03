using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    /// <summary>
    /// An object that represents a heuristic function.
    /// </summary>
    /// <typeparam name="T">The type of a node.</typeparam>
    public interface IHeuristic<T>
    {
        /// <summary>
        /// Evaluates the heuristic function at a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The heuristic function value at the node.</returns>
        double Evaluate(T node);

        /// <summary>
        /// Gets whether the heuristic is consistent.
        /// </summary>
        bool IsConsistent { get; }
    }
}
