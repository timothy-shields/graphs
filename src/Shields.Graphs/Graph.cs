using Shields.Graphs.DataStructures;
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
        private static List<T> ReconstructPath<K, T>(Func<T, K> key, Dictionary<K, T> came_from, T node)
        {
            var path = new List<T>();
            path.Add(node);
            while (came_from.TryGetValue(key(node), out node))
            {
                path.Add(node);
            }
            path.Reverse();
            return path;
        }

        public static Weighted<IEnumerable<T>> UniformCostSearch<T, K>(this IEnumerable<IWeighted<T>> sources, Func<T, K> key, Func<T, IEnumerable<IWeighted<T>>> next, Func<T, bool> goal)
        {
            var came_from = new Dictionary<K, T>();
            var open_lookup = new Dictionary<K, PairingHeap<double, T>.Handle>();
            var open_queue = new PairingHeap<double, T>();
            var closed = new HashSet<K>();
            foreach (var source in sources)
            {
                var u = source.Value;
                var g_u = source.Weight;
                open_lookup.Add(key(u), open_queue.Insert(g_u, u));
            }
            while (!open_queue.IsEmpty)
            {
                var handle_u = open_queue.GetMin();
                var u = handle_u.Value;
                var key_u = key(u);
                var g_u = handle_u.Key;
                if (goal(u))
                {
                    var path = ReconstructPath(key, came_from, u);
                    return new Weighted<IEnumerable<T>>(path, g_u);
                }
                open_lookup.Remove(key_u);
                open_queue.Delete(handle_u);
                closed.Add(key_u);
                foreach (var uv in next(u))
                {
                    var v = uv.Value;
                    var key_v = key(v);
                    if (closed.Contains(key_v))
                    {
                        continue;
                    }
                    PairingHeap<double, T>.Handle v_handle;
                    var g_uv = g_u + uv.Weight;
                    if (open_lookup.TryGetValue(key_v, out v_handle))
                    {
                        if (open_queue.TryDecreaseKey(v_handle, g_uv))
                        {
                            came_from[key_v] = u;
                        }
                    }
                    else
                    {
                        open_lookup.Add(key_v, open_queue.Insert(g_uv, v));
                        came_from[key_v] = u;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using the A* algorithm.
        /// </summary>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="heuristic">The heuristic function.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        public static IWeighted<IEnumerable<T>> AStar<T, K>(this IEnumerable<Weighted<T>> sources, Func<T, K> key, Func<T, IEnumerable<IWeighted<T>>> next, IHeuristic<T> heuristic, Func<T, bool> goal)
        {
            if (heuristic.IsConsistent)
            {
                return AStarConsistentHeuristic<T, K>(sources, key, next, heuristic.Evaluate, goal);
            }
            else
            {
                return AStarInconsistentHeuristic<T, K>(sources, key, next, heuristic.Evaluate, goal);
            }
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using the A* algorithm.
        /// </summary>
        /// <remarks>Because the heuristic is known to be consistent, we can use the closed set optimization.</remarks>
        /// <typeparam name="T">The type of a node.</typeparam>
        /// <typeparam name="K">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="heuristic">The consistent heuristic function.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        private static Weighted<IEnumerable<T>> AStarConsistentHeuristic<T, K>(
            IEnumerable<Weighted<T>> sources,
            Func<T, K> key,
            Func<T, IEnumerable<IWeighted<T>>> next,
            Func<T, double> heuristic,
            Func<T, bool> goal)
        {
            var came_from = new Dictionary<K, T>();
            var open_queue = new PairingHeap<double, AStarOpen<T>>();
            var open_lookup = new Dictionary<K, PairingHeap<double, AStarOpen<T>>.Handle>();
            var closed = new HashSet<K>();
            foreach (var source in sources)
            {
                var u = source.Value;
                var key_u = key(u);
                var g_u = source.Weight;
                var f_u = g_u + heuristic(u);
                var open_u = new AStarOpen<T>(u, g_u);
                open_u.Handle = open_queue.Insert(f_u, open_u);
                open_lookup.Add(key_u, open_u.Handle);
            }
            while (!open_queue.IsEmpty)
            {
                var handle_u = open_queue.GetMin();
                var u = handle_u.Value.Node;
                var key_u = key(u);
                if (goal(u))
                {
                    var path = ReconstructPath(key, came_from, u);
                    return new Weighted<IEnumerable<T>>(path, handle_u.Value.G);
                }
                open_queue.Delete(handle_u);
                open_lookup.Remove(key_u);
                closed.Add(key_u);
                foreach (var uv in next(u))
                {
                    var v = uv.Value;
                    var key_v = key(v);
                    if (closed.Contains(key_v))
                    {
                        continue;
                    }
                    var g_v = handle_u.Value.G + uv.Weight;
                    var f_v = g_v + heuristic(v);
                    PairingHeap<double, AStarOpen<T>>.Handle handle_v;
                    if (open_lookup.TryGetValue(key_v, out handle_v))
                    {
                        if (open_queue.TryDecreaseKey(handle_v, f_v))
                        {
                            handle_v.Value.G = g_v;
                            came_from[key_v] = u;
                        }
                    }
                    else
                    {
                        var open_v = new AStarOpen<T>(v, g_v);
                        open_v.Handle = open_queue.Insert(f_v, open_v);
                        open_lookup.Add(key_v, open_v.Handle);
                        came_from[key_v] = u;
                    }
                }
            }
            return null;
        }

        private class AStarOpen<T>
        {
            public AStarOpen(T node, double g)
            {
                this.Node = node;
                this.G = g;
            }

            public PairingHeap<double, AStarOpen<T>>.Handle Handle { get; set; }
            public T Node { get; private set; }
            public double G { get; set; }
            public double F { get { return Handle.Key; } }
        }

        private static IWeighted<IEnumerable<T>> AStarInconsistentHeuristic<T, K>(
            IEnumerable<Weighted<T>> sources,
            Func<T, K> key,
            Func<T, IEnumerable<IWeighted<T>>> next,
            Func<T, double> heuristic,
            Func<T, bool> goal)
        {
            var came_from = new Dictionary<K, T>();
            var open_queue = new PairingHeap<double, AStarOpen<T>>();
            var open_lookup = new Dictionary<K, PairingHeap<double, AStarOpen<T>>.Handle>();
            foreach (var source in sources)
            {
                var u = source.Value;
                var key_u = key(u);
                var g_u = source.Weight;
                var f_u = g_u + heuristic(u);
                var open_u = new AStarOpen<T>(u, g_u);
                open_u.Handle = open_queue.Insert(f_u, open_u);
                open_lookup.Add(key_u, open_u.Handle);
            }
            while (!open_queue.IsEmpty)
            {
                var handle_u = open_queue.GetMin();
                var u = handle_u.Value.Node;
                var key_u = key(u);
                if (goal(u))
                {
                    var path = ReconstructPath(key, came_from, u);
                    return new Weighted<IEnumerable<T>>(path, handle_u.Value.G);
                }
                open_queue.Delete(handle_u);
                open_lookup.Remove(key_u);
                foreach (var uv in next(u))
                {
                    var v = uv.Value;
                    var key_v = key(v);
                    var g_v = handle_u.Value.G + uv.Weight;
                    var f_v = g_v + heuristic(v);
                    PairingHeap<double, AStarOpen<T>>.Handle handle_v;
                    if (open_lookup.TryGetValue(key_v, out handle_v))
                    {
                        if (open_queue.TryDecreaseKey(handle_v, f_v))
                        {
                            handle_v.Value.G = g_v;
                            came_from[key_v] = u;
                        }
                    }
                    else
                    {
                        var open_v = new AStarOpen<T>(v, g_v);
                        open_v.Handle = open_queue.Insert(f_v, open_v);
                        open_lookup.Add(key_v, open_v.Handle);
                        came_from[key_v] = u;
                    }
                }
            }
            return null;
        }

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
