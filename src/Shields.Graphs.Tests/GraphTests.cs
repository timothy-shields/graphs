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

        [TestMethod]
        public void OrderTopologicallyWorks()
        {
            var topsort = Graph.OrderTopologically(
                A(2, 3, 5, 7, 8, 9, 10, 11),
                n => n,
                Function(new Dictionary<int, IEnumerable<int>>
                {
                    { 7, A(11, 8) },
                    { 5, A(11) },
                    { 3, A(8, 10) },
                    { 11, A(2, 9, 10) },
                    { 8, A(9) },
                    //{ 9, A(5) }
                })).ToList();

            topsort = topsort;
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
