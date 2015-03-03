using Shields.DataStructures;
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
        private static List<TNode> ReconstructPath<TNode, TKey>(Func<TNode, TKey> key, Dictionary<TKey, TNode> came_from, TNode node)
        {
            var path = new List<TNode>();
            path.Add(node);
            while (came_from.TryGetValue(key(node), out node))
            {
                path.Add(node);
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using uniform cost search.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        public static IWeighted<IEnumerable<TNode>> UniformCostSearch<TNode, TKey>(
            this IEnumerable<IWeighted<TNode>> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<IWeighted<TNode>>> next,
            Func<TNode, bool> goal)
        {
            var came_from = new Dictionary<TKey, TNode>();
            var open_lookup = new Dictionary<TKey, PairingHeap<double, TNode>.Handle>();
            var open_queue = new PairingHeap<double, TNode>();
            var closed = new HashSet<TKey>();
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
                    return new Weighted<IEnumerable<TNode>>(path, g_u);
                }
                open_lookup.Remove(key_u);
                open_queue.Remove(handle_u);
                closed.Add(key_u);
                foreach (var uv in next(u))
                {
                    var v = uv.Value;
                    var key_v = key(v);
                    if (closed.Contains(key_v))
                    {
                        continue;
                    }
                    PairingHeap<double, TNode>.Handle v_handle;
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
        /// Gets the minimum-weight path to a goal using uniform cost search.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="descriptor">The object describing how to navigate the weighted graph.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        public static IWeighted<IEnumerable<TNode>> UniformCostSearch<TNode, TKey>(
            this IEnumerable<IWeighted<TNode>> sources,
            IWeightedGraphDescriptor<TNode, TKey> descriptor,
            Func<TNode, bool> goal)
        {
            return UniformCostSearch(sources, descriptor.Key, descriptor.Next, goal);
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using the A* algorithm.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <param name="heuristic">The heuristic function.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        public static IWeighted<IEnumerable<TNode>> AStar<TNode, TKey>(
            this IEnumerable<IWeighted<TNode>> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<IWeighted<TNode>>> next,
            Func<TNode, bool> goal,
            IHeuristic<TNode> heuristic)
        {
            if (heuristic.IsConsistent)
            {
                return AStarConsistent<TNode, TKey>(sources, key, next, goal, heuristic.Evaluate);
            }
            else
            {
                return AStarInconsistent<TNode, TKey>(sources, key, next, goal, heuristic.Evaluate);
            }
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using the A* algorithm.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="descriptor">The object describing how to navigate the weighted graph.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <param name="heuristic">The heuristic function.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        public static IWeighted<IEnumerable<TNode>> AStar<TNode, TKey>(
            this IEnumerable<IWeighted<TNode>> sources,
            IWeightedGraphDescriptor<TNode, TKey> descriptor,
            Func<TNode, bool> goal,
            IHeuristic<TNode> heuristic)
        {
            return AStar(sources, descriptor.Key, descriptor.Next, goal, heuristic);
        }

        private class AStarOpen<TNode>
        {
            public AStarOpen(TNode node, double g)
            {
                this.Node = node;
                this.G = g;
            }

            public PairingHeap<double, AStarOpen<TNode>>.Handle Handle { get; set; }
            public TNode Node { get; private set; }
            public double G { get; set; }
            public double F { get { return Handle.Key; } }
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using the A* algorithm.
        /// </summary>
        /// <remarks>Because the heuristic is known to be consistent, we can use the closed set optimization.</remarks>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <param name="heuristic">The consistent heuristic function.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        private static Weighted<IEnumerable<TNode>> AStarConsistent<TNode, TKey>(
            IEnumerable<IWeighted<TNode>> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<IWeighted<TNode>>> next,
            Func<TNode, bool> goal,
            Func<TNode, double> heuristic)
        {
            var came_from = new Dictionary<TKey, TNode>();
            var open_queue = new PairingHeap<double, AStarOpen<TNode>>();
            var open_lookup = new Dictionary<TKey, PairingHeap<double, AStarOpen<TNode>>.Handle>();
            var closed = new HashSet<TKey>();
            foreach (var source in sources)
            {
                var u = source.Value;
                var key_u = key(u);
                var g_u = source.Weight;
                var f_u = g_u + heuristic(u);
                var open_u = new AStarOpen<TNode>(u, g_u);
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
                    return new Weighted<IEnumerable<TNode>>(path, handle_u.Value.G);
                }
                open_queue.Remove(handle_u);
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
                    PairingHeap<double, AStarOpen<TNode>>.Handle handle_v;
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
                        var open_v = new AStarOpen<TNode>(v, g_v);
                        open_v.Handle = open_queue.Insert(f_v, open_v);
                        open_lookup.Add(key_v, open_v.Handle);
                        came_from[key_v] = u;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the minimum-weight path to a goal using the A* algorithm.
        /// </summary>
        /// <remarks>Because the heuristic is not known to be consistent, we cannot use the closed set optimization.</remarks>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the search, weighted by their initial cost.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="goal">The goal predicate.</param>
        /// <param name="heuristic">The possibly-inconsistent heuristic function.</param>
        /// <returns>The minimum-weight path, or null if none exists.</returns>
        private static IWeighted<IEnumerable<TNode>> AStarInconsistent<TNode, TKey>(
            IEnumerable<IWeighted<TNode>> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<IWeighted<TNode>>> next,
            Func<TNode, bool> goal,
            Func<TNode, double> heuristic)
        {
            var came_from = new Dictionary<TKey, TNode>();
            var open_queue = new PairingHeap<double, AStarOpen<TNode>>();
            var open_lookup = new Dictionary<TKey, PairingHeap<double, AStarOpen<TNode>>.Handle>();
            foreach (var source in sources)
            {
                var u = source.Value;
                var key_u = key(u);
                var g_u = source.Weight;
                var f_u = g_u + heuristic(u);
                var open_u = new AStarOpen<TNode>(u, g_u);
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
                    return new Weighted<IEnumerable<TNode>>(path, handle_u.Value.G);
                }
                open_queue.Remove(handle_u);
                open_lookup.Remove(key_u);
                foreach (var uv in next(u))
                {
                    var v = uv.Value;
                    var key_v = key(v);
                    var g_v = handle_u.Value.G + uv.Weight;
                    var f_v = g_v + heuristic(v);
                    PairingHeap<double, AStarOpen<TNode>>.Handle handle_v;
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
                        var open_v = new AStarOpen<TNode>(v, g_v);
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
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="visited">The set of node keys which have already been visited. It will be updated during the method call.</param>
        /// <returns>The breadth-first traversal of nodes.</returns>
        private static IEnumerable<TNode> BreadthFirstTraversal<TNode, TKey>(
            IEnumerable<TNode> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<TNode>> next,
            HashSet<TKey> visited)
        {
            var queue = new Queue<TNode>(sources);
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
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <returns>The breadth-first traversal of nodes.</returns>
        public static IEnumerable<TNode> BreadthFirstTraversal<TNode, TKey>(
            this IEnumerable<TNode> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<TNode>> next)
        {
            return BreadthFirstTraversal(sources, key, next, new HashSet<TKey>());
        }

        /// <summary>
        /// Performs a breadth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="descriptor">The object describing how to navigate the graph.</param>
        /// <returns>The breadth-first traversal of nodes.</returns>
        public static IEnumerable<TNode> BreadthFirstTraversal<TNode, TKey>(
            this IEnumerable<TNode> sources,
            IGraphDescriptor<TNode, TKey> descriptor)
        {
            return BreadthFirstTraversal(sources, descriptor.Key, descriptor.Next);
        }

        /// <summary>
        /// A stack frame in the depth-first traversal state.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        private class DepthFirstTraversalState<TNode>
        {
            public TNode Node;
            public IEnumerator<TNode> NextEnumerator;
        }

        /// <summary>
        /// Performs a depth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <param name="visited">The set of node keys which have already been visited. It will be updated during the method call.</param>
        /// <returns>The depth-first traversal of nodes.</returns>
        private static IEnumerable<TNode> DepthFirstTraversal<TNode, TKey>(
            IEnumerable<TNode> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<TNode>> next,
            HashSet<TKey> visited)
        {
            var stack = new Stack<DepthFirstTraversalState<TNode>>();
            try
            {
                foreach (var source in sources)
                {
                    stack.Push(new DepthFirstTraversalState<TNode> { Node = source });
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
                                    stack.Push(new DepthFirstTraversalState<TNode> { Node = state.NextEnumerator.Current });
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
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <returns>The depth-first traversal of nodes.</returns>
        public static IEnumerable<TNode> DepthFirstTraversal<TNode, TKey>(
            this IEnumerable<TNode> sources,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<TNode>> next)
        {
            return DepthFirstTraversal(sources, key, next, new HashSet<TKey>());
        }

        /// <summary>
        /// Performs a depth-first traversal of a graph or digraph.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="sources">The set of nodes from which to start the traversal.</param>
        /// <param name="descriptor">The object describing how to navigate the graph.</param>
        /// <returns>The depth-first traversal of nodes.</returns>
        public static IEnumerable<TNode> DepthFirstTraversal<TNode, TKey>(
            this IEnumerable<TNode> sources,
            IGraphDescriptor<TNode, TKey> descriptor)
        {
            return DepthFirstTraversal(sources, descriptor.Key, descriptor.Next);
        }

        private static IEnumerable<TNode> Return<TNode>(TNode item)
        {
            yield return item;
        }

        /// <summary>
        /// Gets the connected components of a graph. For digraphs use <see cref="StronglyConnectedComponents"/>.
        /// </summary>
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="nodes">The set of nodes.</param>
        /// <param name="key">The function which maps a node to its key.</param>
        /// <param name="next">The function which maps a node to its adjacent nodes.</param>
        /// <returns>The connected components of the graph.</returns>
        public static IEnumerable<IEnumerable<TNode>> ConnectedComponents<TNode, TKey>(
            this IEnumerable<TNode> nodes,
            Func<TNode, TKey> key,
            Func<TNode, IEnumerable<TNode>> next)
        {
            var visited = new HashSet<TKey>();
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
        /// <typeparam name="TNode">The type of a node.</typeparam>
        /// <typeparam name="TKey">The type of a node key.</typeparam>
        /// <param name="nodes">The set of nodes.</param>
        /// <param name="descriptor">The object describing how to navigate the graph.</param>
        /// <returns>The connected components of the graph.</returns>
        public static IEnumerable<IEnumerable<TNode>> ConnectedComponents<TNode, TKey>(
            this IEnumerable<TNode> nodes,
            IGraphDescriptor<TNode, TKey> descriptor)
        {
            return ConnectedComponents(nodes, descriptor.Key, descriptor.Next);
        }
    }
}
