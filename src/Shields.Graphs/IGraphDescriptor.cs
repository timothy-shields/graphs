using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    /// <summary>
    /// An object that describes how to navigate a graph.
    /// </summary>
    /// <typeparam name="T">The type of a node.</typeparam>
    /// <typeparam name="K">The type of a node key.</typeparam>
    public interface IGraphDescriptor<T, K>
    {
        /// <summary>
        /// Gets the key of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The key.</returns>
        K Key(T node);
        
        /// <summary>
        /// Gets the adjacent nodes of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The adjacent nodes.</returns>
        IEnumerable<T> Next(T node);
    }
}
