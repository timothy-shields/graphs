using System;
using System.Collections.Generic;
using System.Linq;

namespace Shields.Graphs
{
    /// <summary>
    /// Provides functional graph algorithms.
    /// </summary>
    public static class Graph
    {
        /// <summary>
        /// Performs a breadth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="visited">The set of node keys which have already been visited. It will be updated during the method call.</param>
        /// <returns>The breadth-first traversal of nodes.</returns>
        private static IEnumerable<T> BreadthFirstTraversal<T, K>(IEnumerable<T> sources, Func<T, K> key, Func<T, IEnumerable<T>> next, HashSet<K> visited)
        {
            var queue = new Queue<T>(sources);
            while (queue.Any())
            {
                var u = queue.Dequeue();
                if (visited.Add(key(u)))
                {
                    yield return u;
                }
                foreach (var v in next(u))
                {
                    if (!visited.Contains(key(v)))
                    {
                        queue.Enqueue(v);
                    }
                }
            }
        }

        /// <summary>
        /// Performs a breadth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <returns>The breadth-first traversal of nodes.</returns>
        public static IEnumerable<T> BreadthFirstTraversal<T, K>(this IEnumerable<T> sources, Func<T, K> key, Func<T, IEnumerable<T>> next)
        {
            return BreadthFirstTraversal(sources, key, next, new HashSet<K>());
        }

        /// <summary>
        /// Performs a breadth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="descriptor">The object describing how to navigate the graph.</param>
        /// <returns>The breadth-first traversal of nodes.</returns>
        public static IEnumerable<T> BreadthFirstTraversal<T, K>(this IEnumerable<T> sources, IGraphDescriptor<T, K> descriptor)
        {
            return BreadthFirstTraversal(sources, descriptor.Key, descriptor.Next);
        }

        /// <summary>
        /// A stack frame in the depth-first traversal state.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        private class DepthFirstTraversalState<T>
        {
            public T Node;
            public IEnumerator<T> NextEnumerator;
        }

        /// <summary>
        /// Performs a depth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="visited">The set of node keys which have already been visited. It will be updated during the method call.</param>
        /// <returns>The depth-first traversal of nodes.</returns>
        private static IEnumerable<T> DepthFirstTraversal<T, K>(IEnumerable<T> sources, Func<T, K> key, Func<T, IEnumerable<T>> next, HashSet<K> visited)
        {
            var stack = new Stack<DepthFirstTraversalState<T>>();
            try
            {
                foreach (var source in sources)
                {
                    stack.Push(new DepthFirstTraversalState<T> { Node = source });
                    while (stack.Any())
                    {
                        var state = stack.Peek();
                        if (state.NextEnumerator == null)
                        {
                            if (visited.Add(key(state.Node)))
                            {
                                yield return state.Node;
                                state.NextEnumerator = next(state.Node).GetEnumerator();
                            }
                            else
                            {
                                stack.Pop();
                            }
                        }
                        else
                        {
                            if (state.NextEnumerator.MoveNext())
                            {
                                if (!visited.Contains(key(state.NextEnumerator.Current)))
                                {
                                    stack.Push(new DepthFirstTraversalState<T> { Node = state.NextEnumerator.Current });
                                }
                            }
                            else
                            {
                                state.NextEnumerator.Dispose();
                                stack.Pop();
                            }
                        }
                    }
                }
            }
            finally
            {
                while (stack.Any())
                {
                    var state = stack.Pop();
                    if (state.NextEnumerator != null)
                    {
                        state.NextEnumerator.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Performs a depth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <returns>The depth-first traversal of nodes.</returns>
        public static IEnumerable<T> DepthFirstTraversal<T, K>(this IEnumerable<T> sources, Func<T, K> key, Func<T, IEnumerable<T>> next)
        {
            return DepthFirstTraversal(sources, key, next, new HashSet<K>());
        }

        /// <summary>
        /// Performs a depth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="descriptor">The object describing how to navigate the graph.</param>
        /// <returns>The depth-first traversal of nodes.</returns>
        public static IEnumerable<T> DepthFirstTraversal<T, K>(this IEnumerable<T> sources, IGraphDescriptor<T, K> descriptor)
        {
            return DepthFirstTraversal(sources, descriptor.Key, descriptor.Next);
        }

        private static IEnumerable<T> Return<T>(T item)
        {
            yield return item;
        }

        /// <summary>
        /// Gets the connected components of a graph. For digraphs use <see cref="StronglyConnectedComponents"/>.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="nodes">The set of nodes.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <returns>The connected components of the graph.</returns>
        public static IEnumerable<IEnumerable<T>> ConnectedComponents<T, K>(this IEnumerable<T> nodes, Func<T, K> key, Func<T, IEnumerable<T>> next)
        {
            var visited = new HashSet<K>();
            foreach (var node in nodes)
            {
                if (visited.Contains(key(node)))
                {
                    continue;
                }
                yield return BreadthFirstTraversal(Return(node), key, next, visited).ToList();
            }
        }

        /// <summary>
        /// Gets the connected components of a graph. For digraphs use <see cref="StronglyConnectedComponents"/>.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="nodes">The set of nodes.</param>
        /// <param name="descriptor">The object describing how to navigate the graph.</param>
        /// <returns>The connected components of the graph.</returns>
        public static IEnumerable<IEnumerable<T>> ConnectedComponents<T, K>(this IEnumerable<T> nodes, IGraphDescriptor<T, K> descriptor)
        {
            return ConnectedComponents(nodes, descriptor.Key, descriptor.Next);
        }
    }
}
