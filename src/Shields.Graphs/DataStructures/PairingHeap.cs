using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shields.Graphs.DataStructures
{
    public class PairingHeap<TKey, TValue> : IPriorityQueue<TKey, TValue, PairingHeap<TKey, TValue>.Handle>
        where TKey : IComparable<TKey>
    {
        private Node root;
        private int count;

        public PairingHeap()
        {
            this.root = null;
            this.count = 0;
        }

        public bool IsEmpty
        {
            get { return root == null; }
        }

        public int Count
        {
            get { return count; }
        }

        private static Node Pair(Node n1, Node n2)
        {
            if (n1 == null)
            {
                return n2;
            }
            if (n2 == null)
            {
                return n1;
            }
            if (n1.left != null || n2.left != null)
            {
                throw new Exception("Can only pair roots.");
            }
            if (n1.key.CompareTo(n2.key) < 0)
            {
                var temp = n1;
                n1 = n2;
                n2 = temp;
            }
            //n1 becomes the first child of n2.
            var c = n2.firstChild;
            n1.left = n2;
            n1.right = c;
            if (c != null)
            {
                c.left = n1;
            }
            n2.firstChild = n1;
            return n2;
        }

        private static void SpliceOut(Node n)
        {
            if (n.left == null)
            {
                return;
            }
            if (n.left.firstChild == n)
            {
                //n is the first child of its parent.
                var p = n.left;
                if (n.right == null)
                {
                    //n is the only child of p.
                    p.firstChild = null;
                }
                else
                {
                    //n has a right sibling.
                    var r = n.right;
                    r.left = p;
                    p.firstChild = r;
                }
            }
            else
            {
                //n is not the first child of its parent.
                var l = n.left;
                var r = n.right;
                l.right = r;
                if (r != null)
                {
                    r.left = l;
                }
            }
            n.left = null;
            n.right = null;
        }

        private static Node DeleteRoot(Node n)
        {
            if (n.left != null)
            {
                throw new Exception("Invalid call to DeleteRoot.");
            }
            if (n.firstChild == null)
            {
                return null;
            }
            if (n.firstChild.right != null)
            {
                //n has at least two children.
                var c1 = n.firstChild;
                var c2 = c1.right;
                SpliceOut(c1);
                SpliceOut(c2);
                return Pair(Pair(c1, c2), DeleteRoot(n));
            }
            //n has a single child.
            Node c = n.firstChild;
            c.left = null;
            n.firstChild = null;
            return c;
        }

        public Handle Insert(TKey key, TValue value)
        {
            count++;
            var node = new Node(key, value);
            if (root == null)
            {
                root = node;
            }
            else
            {
                root = Pair(root, node);
            }
            return node.handle;
        }

        public void Delete(Handle handle)
        {
            if (!handle.IsActive)
            {
                throw new Exception("Tried to use inactive handle.");
            }
            count--;
            if (handle.node == root)
            {
                root = DeleteRoot(root);
            }
            else
            {
                SpliceOut(handle.node);
                root = Pair(root, DeleteRoot(handle.node));
            }
            handle.IsActive = false;
        }

        public void DecreaseKey(Handle handle, TKey key)
        {
            if (!handle.IsActive)
            {
                throw new Exception("Tried to use inactive handle.");
            }
            if (key.CompareTo(handle.Key) > 0)
            {
                throw new Exception("Attempted to increase key.");
            }
            handle.node.key = key;
            if (handle.node != root)
            {
                SpliceOut(handle.node);
                root = Pair(root, handle.node);
            }
        }

        public Handle GetMin()
        {
            if (IsEmpty)
            {
                throw new Exception("Cannot get from empty pairing heap.");
            }
            return root.handle;
        }

        public Handle ExtractMin()
        {
            if (IsEmpty)
            {
                throw new Exception("Cannot extract from empty pairing heap.");
            }
            Handle h = root.handle;
            Delete(h);
            return h;
        }

        public IEnumerable<Handle> ActiveHandles()
        {
            return GetHandles(root);
        }

        private IEnumerable<Handle> GetHandles(Node n)
        {
            if (n != null)
            {
                yield return n.handle;
                for (var c = n.firstChild; c != null; c = c.right)
                {
                    foreach (var h in GetHandles(c))
                    {
                        yield return h;
                    }
                }
            }
        }

        public class Handle : IHandle<TKey, TValue>
        {
            internal Node node;
            public TKey Key { get { return node.key; } }
            public TValue Value { get { return node.value; } }
            public bool IsActive { get; internal set; }

            internal Handle(Node node)
            {
                this.node = node;
                this.IsActive = true;
            }
        }

        internal class Node
        {
            internal TKey key;
            internal TValue value;
            internal Node left;
            internal Node right;
            internal Node firstChild;
            internal Handle handle;
            internal Node(TKey key, TValue value)
            {
                this.key = key;
                this.value = value;
                this.left = null;
                this.right = null;
                this.firstChild = null;
                this.handle = new PairingHeap<TKey, TValue>.Handle(this);
            }
        }
    }
}