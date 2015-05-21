using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Shields.Graphs.Tests
{
    [TestClass]
    public class GraphTests
    {
        private static Func<T, U> F<T, U>(Dictionary<T, U> lookup, Func<U> defaultValue = null)
        {
            return key =>
            {
                if (defaultValue == null)
                {
                    return lookup[key];
                }

                U result;
                if (lookup.TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    return defaultValue();
                }
            };
        }

        private static Func<T, IEnumerable<U>> Function<T, U>(Dictionary<T, IEnumerable<U>> lookup)
        {
            return key =>
            {
                IEnumerable<U> result;
                if (lookup.TryGetValue(key, out result))
                {
                    return result;
                }
                else
                {
                    return Enumerable.Empty<U>();
                }
            };
        }

        private static Func<T, U> CallAtMostOnce<T, U>(Func<T, U> f)
        {
            var set = new HashSet<T>();
            return x =>
            {
                bool added = set.Add(x);
                Assert.IsTrue(added);
                return f(x);
            };
        }

        /// <summary>
        /// Shorthand for Weighted(value, weight) creation.
        /// </summary>
        private static IWeighted<T> W<T>(T value, double weight)
        {
            return new Weighted<T>(value, weight);
        }

        /// <summary>
        /// Shorthand for List&lt;T&gt; creation.
        /// </summary>
        private static List<T> A<T>(params T[] items)
        {
            return items.ToList();
        }

        [TestMethod]
        public void BreadthFirstTraversalCallsNextAtMostOncePerNode()
        {
            Graph.BreadthFirstTraversal(A(0), n => n, CallAtMostOnce(Function(new Dictionary<int, IEnumerable<int>>
            {
                { 0, A(1, 2) },
                { 1, A(2, 3) },
                { 2, A(3) }
            }))).ToList();
        }

        [TestMethod]
        public void DepthFirstTraversalIsLazy()
        {
            var guid = Guid.NewGuid();
            var descriptor = GraphDescriptor.Create(n => n, Function(new Dictionary<int, IEnumerable<int>>
            {
                { 0, A(1, 2) },
                { 1, A(3, 4) },
                { 2, EnumerableEx.Throw<int>(new Exception(guid.ToString())) }
            }));

            var enumerator = Graph.DepthFirstTraversal(A(0), descriptor).GetEnumerator();
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Current);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Current);
            try
            {
                enumerator.MoveNext();
            }
            catch (Exception e)
            {
                Assert.AreEqual(guid.ToString(), e.Message);
                return;
            }
            Assert.Fail();
        }

        [TestMethod]
        public void UniformCostSearch1()
        {
            var path = UniformCostSearch('A', 'F',
                new Dictionary<char, IEnumerable<IWeighted<char>>>
                {
                    { 'A', A(W('B', 4), W('C', 2)) },
                    { 'B', A(W('C', 5), W('D', 10)) },
                    { 'C', A(W('E', 3)) },
                    { 'D', A(W('F', 11)) },
                    { 'E', A(W('D', 4)) }
                });

            CollectionAssert.AreEqual("ACEDF".ToList(), path.Value.ToList());
            Assert.AreEqual(20, path.Weight);
        }

        [TestMethod]
        public void UniformCostSearch2()
        {
            var path = UniformCostSearch('A', 'D',
                new Dictionary<char, IEnumerable<IWeighted<char>>>
                {
                    { 'A', A(W('B', 0), W('C', 1)) },
                    { 'B', A(W('C', 0)) },
                    { 'C', A(W('D', 2)) }
                });

            CollectionAssert.AreEqual("ABCD".ToList(), path.Value.ToList());
            Assert.AreEqual(2, path.Weight);
        }

        [TestMethod]
        public void AStar1()
        {
            var path = AStar(
                'A', 'F',
                new Dictionary<char, IEnumerable<IWeighted<char>>>
                {
                    { 'A', A(W('B', 4), W('C', 2)) },
                    { 'B', A(W('C', 5), W('D', 10)) },
                    { 'C', A(W('E', 3)) },
                    { 'D', A(W('F', 11)) },
                    { 'E', A(W('D', 4)) }
                },
                true, new Dictionary<char, double>
                {
                    { 'A', 20 },
                    { 'B', 21 },
                    { 'C', 18 },
                    { 'D', 11 },
                    { 'E', 15 },
                    { 'F', 0 }
                });

            CollectionAssert.AreEqual("ACEDF".ToList(), path.Value.ToList());
            Assert.AreEqual(20, path.Weight);
        }

        [TestMethod]
        public void AStarFailsIfNotInformedOfInconsistentHeuristic()
        {
            var path = AStar('A', 'D',
                new Dictionary<char, IEnumerable<IWeighted<char>>>
                {
                    { 'A', A(W('B', 0), W('C', 1)) },
                    { 'B', A(W('C', 0)) },
                    { 'C', A(W('D', 2)) }
                },
                true, new Dictionary<char, double>
                {
                    { 'A', 0 },
                    { 'B', 2 },
                    { 'C', 0 },
                    { 'D', 0 }
                });

            CollectionAssert.AreEqual("ACD".ToList(), path.Value.ToList());
            Assert.AreEqual(3, path.Weight);
        }

        [TestMethod]
        public void AStarSucceedsIfInformedOfInconsistentHeuristic()
        {
            var path = AStar('A', 'D',
                new Dictionary<char, IEnumerable<IWeighted<char>>>
                {
                    { 'A', A(W('B', 0), W('C', 1)) },
                    { 'B', A(W('C', 0)) },
                    { 'C', A(W('D', 2)) }
                },
                false, new Dictionary<char, double>
                {
                    { 'A', 0 },
                    { 'B', 2 },
                    { 'C', 0 },
                    { 'D', 0 }
                });

            CollectionAssert.AreEqual("ABCD".ToList(), path.Value.ToList());
            Assert.AreEqual(2, path.Weight);
        }

        //[TestMethod]
        //public void MyTestMethod()
        //{
        //    var sb = new System.Text.StringBuilder();
        //    var nodes = Enumerable.Range(1, 6).ToList();
        //    foreach (var descriptor in GraphGenerator.AllDirectedAcyclicGraphs(nodes))
        //    {
        //        foreach (var u in nodes)
        //        {
        //            sb.Append(u);
        //            sb.Append(':');
        //            foreach (var v in descriptor.Next(u))
        //            {
        //                sb.Append(' ');
        //                sb.Append(v);
        //            }
        //            sb.AppendLine();
        //        }
        //        sb.AppendLine();
        //    }
        //    var s = sb.ToString();
        //    s = s;
        //}

        [TestMethod]
        public void OrderTopologicallyWorks()
        {
            var nodes = A(2, 3, 5, 7, 8, 9, 10, 11);
            var edges = new Dictionary<int, IEnumerable<int>>
            {
                { 7, A(11, 8) },
                { 5, A(11) },
                { 3, A(8, 10) },
                { 11, A(2, 9, 10) },
                { 8, A(9) }
            };

            var topsort = nodes.OrderTopologically(n => n, Function(edges)).ToList();

            CollectionAssert.AreEqual(nodes.OrderBy(n => n).ToList(), topsort.OrderBy(n => n).ToList());
            foreach (var u in edges.Keys)
            {
                foreach (var v in edges[u])
                {
                    Assert.IsTrue(topsort.IndexOf(u) < topsort.IndexOf(v));
                }
            }
        }

        /// <summary>
        /// Property-based test of OrderTopologically. A topsort should contain the same
        /// set of nodes and should satisfy all of the constraints indicated by the edges.
        /// </summary>
        [TestMethod]
        public void OrderTopologicallyWorksOnAllSmallDags()
        {
            for (int order = 0; order <= 6; order++)
            {
                var nodes = Enumerable.Range('A', order).ToList();
                foreach (var descriptor in GraphGenerator.AllDirectedAcyclicGraphs(nodes))
                {
                    var topsort = nodes.OrderTopologically(descriptor).ToList();

                    CollectionAssert.AreEqual(nodes.OrderBy(u => u).ToList(), topsort.OrderBy(u => u).ToList());
                    foreach (var u in nodes)
                    {
                        foreach (var v in descriptor.Next(u))
                        {
                            Assert.IsTrue(topsort.IndexOf(u) < topsort.IndexOf(v));
                        }
                    }
                }
            }
        }

        [TestMethod]
        [ExpectedException(typeof(GraphCycleException))]
        public void OrderTopologicallyThrowsGraphCycleException()
        {
            var nodes = A(2, 3, 5, 7, 8, 9, 10, 11);
            var edges = new Dictionary<int, IEnumerable<int>>
            {
                { 7, A(11, 8) },
                { 5, A(11) },
                { 3, A(8, 10) },
                { 11, A(2, 9, 10) },
                { 8, A(9) },
                { 9, A(5) }
            };

            var topsort = nodes.OrderTopologically(n => n, Function(edges)).ToList();
        }

        private IWeighted<IEnumerable<char>> UniformCostSearch(
            char start, char goal,
            Dictionary<char, IEnumerable<IWeighted<char>>> next)
        {
            var descriptor = WeightedGraphDescriptor.Create(n => n, Function(next));

            return Graph.UniformCostSearch(
                EnumerableEx.Return(new Weighted<char>(start, 0)),
                descriptor.Key,
                descriptor.Next,
                n => n == goal);
        }

        private IWeighted<IEnumerable<char>> AStar(
            char start, char goal,
            Dictionary<char, IEnumerable<IWeighted<char>>> next,
            bool isConsistent, Dictionary<char, double> heuristic)
        {
            var descriptor = WeightedGraphDescriptor.Create(n => n, Function(next));

            return Graph.AStar(
                EnumerableEx.Return(new Weighted<char>(start, 0)),
                descriptor.Key,
                descriptor.Next,
                n => n == goal,
                Heuristic.Create(F(heuristic), isConsistent));
        }
    }
}
