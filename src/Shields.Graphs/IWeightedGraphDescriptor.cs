using System.Collections.Generic;

namespace Shields.Graphs
{
    /// <summary>
    /// An object that describes how to navigate a weighted graph.
    /// </summary>
    /// <typeparam name="TNode">The type of a node.</typeparam>
    /// <typeparam name="TKey">The type of a node key.</typeparam>
    public interface IWeightedGraphDescriptor<TNode, TKey>
    {
        /// <summary>
        /// Gets the key of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The key.</returns>
        TKey Key(TNode node);
        
        /// <summary>
        /// Gets the adjacent nodes of a node, with their edge weights.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The adjacent nodes and their corresponding edge weights.</returns>
        IEnumerable<IWeighted<TNode>> Next(TNode node);
    }
}
