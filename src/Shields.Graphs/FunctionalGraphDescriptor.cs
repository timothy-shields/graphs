using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shields.Graphs
{
    /// <summary>
    /// A functional implementation of <see cref="IGraphDescriptor&lt;T, K&gt;"/>.
    /// </summary>
    /// <typeparam name="T">The type of a node.</typeparam>
    /// <typeparam name="K">The type of a node key.</typeparam>
    public class FunctionalGraphDescriptor<T, K> : IGraphDescriptor<T, K>
    {
        private readonly Func<T, K> key;
        private readonly Func<T, IEnumerable<T>> next;

        /// <summary>
        /// Constructs a <see cref="FunctionalGraphDescriptor"/>.
        /// </summary>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        public FunctionalGraphDescriptor(Func<T, K> key, Func<T, IEnumerable<T>> next)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (next == null)
            {
                throw new ArgumentNullException("next");
            }
            this.key = key;
            this.next = next;
        }

        /// <summary>
        /// Gets the key of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The key.</returns>
        public K Key(T node)
        {
            return key(node);
        }
        
        /// <summary>
        /// Gets the adjacent nodes of a node.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The adjacent nodes.</returns>
        public IEnumerable<T> Next(T node)
        {
            return next(node);
        }
    }
}
