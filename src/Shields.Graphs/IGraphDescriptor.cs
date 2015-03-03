using System.Collections.Generic;

namespace Shields.Graphs
{
    /// <summary>
    /// An object that describes how to navigate a graph.
    /// </summary>
    /// <typeparam name="TNode">The type of a node.</typeparam>
    /// <typeparam name="TKey">The type of a node key.</typeparam>
    public interface IGraphDescriptor<TNode, TKey>
    {
        /// <summary>
        /// Gets the key of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The key.</returns>
        TKey Key(TNode node);
        
        /// <summary>
        /// Gets the adjacent nodes of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The adjacent nodes.</returns>
        IEnumerable<TNode> Next(TNode node);
    }
}
