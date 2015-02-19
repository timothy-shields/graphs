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
        private static Func<T, IEnumerable<T>> CreateNext<T>(Dictionary<T, IEnumerable<T>> lookup)
        {
            return node =>
            {
                IEnumerable<T> result;
                if (lookup.TryGetValue(node, out result))
                {
                    return result;
                }
                else
                {
                    return Enumerable.Empty<T>();
                }
            };
        }

        private static IEnumerable<T> A<T>(params T[] items)
        {
            return items;
        }

        [TestMethod]
        public void DepthFirstTraversalIsLazy()
        {
            var guid = Guid.NewGuid();
            var descriptor = GraphDescriptor.Create<int, int>(n => n, CreateNext(new Dictionary<int, IEnumerable<int>>
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
        public void BreadthFirstTraversalIsCorrect()
        {
            var descriptor = new Mock<IGraphDescriptor<char, int>>();
            
        }
    }
}
