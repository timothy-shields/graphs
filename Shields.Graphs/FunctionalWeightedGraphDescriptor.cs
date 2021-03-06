﻿using System;
using System.Collections.Generic;

namespace Shields.Graphs
{
    /// <summary>
    /// A functional implementation of <see cref="IWeightedGraphDescriptor&lt;TNode, TKey&gt;"/>.
    /// </summary>
    /// <typeparam name="TNode">The type of a node.</typeparam>
    /// <typeparam name="TKey">The type of a node key.</typeparam>
    internal class FunctionalWeightedGraphDescriptor<TNode, TKey> : IWeightedGraphDescriptor<TNode, TKey>
    {
        private readonly Func<TNode, TKey> key;
        private readonly Func<TNode, IEnumerable<IWeighted<TNode>>> next;

        /// <summary>
        /// Constructs a <see cref="FunctionalGraphDescriptor"/>.
        /// </summary>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        public FunctionalWeightedGraphDescriptor(Func<TNode, TKey> key, Func<TNode, IEnumerable<IWeighted<TNode>>> next)
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
        public TKey Key(TNode node)
        {
            return key(node);
        }

        /// <summary>
        /// Gets the adjacent nodes of a node, with their edge weights.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>The adjacent nodes and their corresponding edge weights.</returns>
        public IEnumerable<IWeighted<TNode>> Next(TNode node)
        {
            return next(node);
        }
    }
}
