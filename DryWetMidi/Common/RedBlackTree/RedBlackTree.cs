using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTree<TKey, TValue> : IEnumerable<TValue>
        where TKey : IComparable<TKey>
        where TValue : IEquatable<TValue>
    {
        #region Fields

        private RedBlackTreeNode<TKey, TValue> _root = RedBlackTreeNode<TKey, TValue>.Void;

        #endregion

        #region Constructor

        public RedBlackTree()
        {
        }

        public RedBlackTree(IEnumerable<TValue> values, Func<TValue, TKey> keySelector)
        {
            foreach (var v in values)
            {
                Add(keySelector(v), v);
            }
        }

        #endregion

        #region Methods

        public RedBlackTreeNode<TKey, TValue> GetRoot()
        {
            return _root;
        }

        public RedBlackTree<TKey, TValue> Clone()
        {
            return new RedBlackTree<TKey, TValue>
            {
                _root = _root.Clone(),
            };
        }

        public IEnumerable<RedBlackTreeNode<TKey, TValue>> EnumerateNodes()
        {
            var node = GetMinimumNode();
            if (IsVoid(node))
                yield break;

            do
            {
                yield return node;
            }
            while ((node = GetNextNode(node)) != null);
        }

        public RedBlackTreeNode<TKey, TValue> GetFirstNode(TKey key)
        {
            var node = _root;

            while (!IsVoid(node) && key.CompareTo(node.Key) != 0)
            {
                if (key.CompareTo(node.Key) < 0)
                    node = node.Left;
                else
                    node = node.Right;
            }

            return !IsVoid(node) ? node : null;
        }

        public IEnumerable<RedBlackTreeNode<TKey, TValue>> SearchNodes(TKey key)
        {
            // TODO: optimize

            var queue = new Queue<RedBlackTreeNode<TKey, TValue>>();
            queue.Enqueue(GetFirstNode(key));

            var visited = new HashSet<RedBlackTreeNode<TKey, TValue>>();

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (IsVoid(node) || node.Key.CompareTo(key) != 0 || !visited.Add(node))
                    continue;

                yield return node;

                queue.Enqueue(GetNextNode(node));
                queue.Enqueue(GetPreviousNode(node));
            }
        }

        public IEnumerable<TValue> GetValues(TKey key)
        {
            return SearchNodes(key).Select(n => n.Value);
        }

        public RedBlackTreeNode<TKey, TValue> GetFirstNode(TKey key, TValue value)
        {
            return SearchNodes(key).FirstOrDefault(n => n.Value.Equals(value));
        }

        public RedBlackTreeNode<TKey, TValue> GetMinimumNode()
        {
            return GetMinimumNode(_root);
        }

        public RedBlackTreeNode<TKey, TValue> GetMinimumNode(RedBlackTreeNode<TKey, TValue> node)
        {
            while (!IsVoid(node?.Left))
                node = node.Left;
            return node;
        }

        public RedBlackTreeNode<TKey, TValue> GetMaximumNode()
        {
            return GetMaximumNode(_root);
        }

        public RedBlackTreeNode<TKey, TValue> GetMaximumNode(RedBlackTreeNode<TKey, TValue> node)
        {
            while (!IsVoid(node?.Right))
                node = node.Right;
            return node;
        }

        public RedBlackTreeNode<TKey, TValue> GetNextNode(RedBlackTreeNode<TKey, TValue> node)
        {
            var right = node.Right;
            if (!IsVoid(right))
                return GetMinimumNode(right);

            var nextNode = node.Parent;

            while (!IsVoid(nextNode))
            {
                if (node == nextNode.Left)
                    return nextNode;

                node = nextNode;
                nextNode = node.Parent;
            }

            return IsVoid(nextNode) ? null : nextNode;
        }

        public RedBlackTreeNode<TKey, TValue> GetPreviousNode(RedBlackTreeNode<TKey, TValue> node)
        {
            var left = node.Left;
            if (!IsVoid(left))
                return GetMaximumNode(left);

            var previousNode = node.Parent;

            while (!IsVoid(previousNode))
            {
                if (node == previousNode.Right)
                    return previousNode;

                node = previousNode;
                previousNode = node.Parent;
            }

            return IsVoid(previousNode) ? null : previousNode;
        }

        public RedBlackTreeNode<TKey, TValue> Add(TKey key, TValue value)
        {
            var node = new RedBlackTreeNode<TKey, TValue>(key, value, null);
            Insert(node);
            return node;
        }

        public void Delete(RedBlackTreeNode<TKey, TValue> node)
        {
            if (IsVoid(node))
                return;

            RedBlackTreeNode<TKey, TValue> x = null;
            var y = node;
            var yOriginalColor = y.Color;
            if (node.Left == RedBlackTreeNode<TKey, TValue>.Void)
            {
                x = node.Right;
                Transplant(node, node.Right);
            }
            else if (node.Right == RedBlackTreeNode<TKey, TValue>.Void)
            {
                x = node.Left;
                Transplant(node, node.Left);
            }
            else
            {
                y = GetMinimumNode(node.Right);
                yOriginalColor = y.Color;
                x = y.Right;
                if (y != node.Right)
                {
                    Transplant(y, y.Right);
                    y.Right = node.Right;
                    y.Right.Parent = y;
                }
                else
                    x.Parent = y;
                Transplant(node, y);
                y.Left = node.Left;
                y.Left.Parent = y;
                y.Color = node.Color;
            }
            if (yOriginalColor == RedBlackTreeNodeColor.Black)
                DeleteFixup(x);
        }

        public RedBlackTreeNode<TKey, TValue> GetLastNodeBelowThreshold(TKey threshold)
        {
            return GetLastNodeBelowThreshold(
                threshold,
                node => node.Key);
        }

        public RedBlackTreeNode<TKey, TValue> GetLastNodeBelowThreshold<TValueKey>(
            TValueKey threshold,
            Func<RedBlackTreeNode<TKey, TValue>, TValueKey> keySelector)
            where TValueKey : IComparable<TValueKey>
        {
            return GetLastNodeBelowThreshold(threshold, keySelector, _root);
        }

        public RedBlackTreeNode<TKey, TValue> GetLastNodeBelowThreshold<TValueKey>(
            TValueKey threshold,
            Func<RedBlackTreeNode<TKey, TValue>, TValueKey> keySelector,
            RedBlackTreeNode<TKey, TValue> node)
            where TValueKey : IComparable<TValueKey>
        {
            if (IsVoid(node))
                return null;

            while (true)
            {
                RedBlackTreeNode<TKey, TValue> nextNode = null;
                if (threshold.CompareTo(keySelector(node)) < 0)
                {
                    if (IsVoid(node.Left))
                    {
                        var prev = GetPreviousNode(node);
                        while (!IsVoid(prev) && keySelector(prev).CompareTo(threshold) == 0)
                            prev = GetPreviousNode(prev);

                        return IsVoid(prev)
                            ? null
                            : prev;
                    }

                    nextNode = node.Left;
                }
                else if (threshold.CompareTo(keySelector(node)) > 0)
                {
                    if (IsVoid(node.Right))
                        return node;

                    nextNode = node.Right;
                }
                else
                {
                    var prev = GetPreviousNode(node);
                    while (!IsVoid(prev) && keySelector(prev).CompareTo(threshold) == 0)
                        prev = GetPreviousNode(prev);

                    return IsVoid(prev)
                        ? null
                        : prev;
                }

                node = nextNode;
                if (IsVoid(node))
                    return null;
            }
        }

        private bool IsVoid(RedBlackTreeNode<TKey, TValue> node)
        {
            return node == null || node == RedBlackTreeNode<TKey, TValue>.Void;
        }

        private void Transplant(RedBlackTreeNode<TKey, TValue> u, RedBlackTreeNode<TKey, TValue> v)
        {
            if (u.Parent == RedBlackTreeNode<TKey, TValue>.Void)
                _root = v;
            else if (u == u.Parent.Left)
                u.Parent.Left = v;
            else
                u.Parent.Right = v;
            v.Parent = u.Parent;
        }

        private void DeleteFixup(RedBlackTreeNode<TKey, TValue> x)
        {
            while (x != _root && x.Color == RedBlackTreeNodeColor.Black)
            {
                if (x == x.Parent.Left)
                {
                    var w = x.Parent.Right;
                    if (w.Color == RedBlackTreeNodeColor.Red)
                    {
                        w.Color = RedBlackTreeNodeColor.Black;
                        x.Parent.Color = RedBlackTreeNodeColor.Red;
                        LeftRotate(x.Parent);
                        w = x.Parent.Right;
                    }
                    if (w.Left.Color == RedBlackTreeNodeColor.Black && w.Right.Color == RedBlackTreeNodeColor.Black)
                    {
                        w.Color = RedBlackTreeNodeColor.Red;
                        x = x.Parent;
                    }
                    else
                    {
                        if (w.Right.Color == RedBlackTreeNodeColor.Black)
                        {
                            w.Left.Color = RedBlackTreeNodeColor.Black;
                            w.Color = RedBlackTreeNodeColor.Red;
                            RightRotate(w);
                            w = x.Parent.Right;
                        }
                        w.Color = x.Parent.Color;
                        x.Parent.Color = RedBlackTreeNodeColor.Black;
                        w.Right.Color = RedBlackTreeNodeColor.Black;
                        LeftRotate(x.Parent);
                        x = _root;
                    }
                }
                else
                {
                    var w = x.Parent.Left;
                    if (w.Color == RedBlackTreeNodeColor.Red)
                    {
                        w.Color = RedBlackTreeNodeColor.Black;
                        x.Parent.Color = RedBlackTreeNodeColor.Red;
                        RightRotate(x.Parent);
                        w = x.Parent.Left;
                    }
                    if (w.Right.Color == RedBlackTreeNodeColor.Black && w.Left.Color == RedBlackTreeNodeColor.Black)
                    {
                        w.Color = RedBlackTreeNodeColor.Red;
                        x = x.Parent;
                    }
                    else
                    {
                        if (w.Left.Color == RedBlackTreeNodeColor.Black)
                        {
                            w.Right.Color = RedBlackTreeNodeColor.Black;
                            w.Color = RedBlackTreeNodeColor.Red;
                            LeftRotate(w);
                            w = x.Parent.Left;
                        }
                        w.Color = x.Parent.Color;
                        x.Parent.Color = RedBlackTreeNodeColor.Black;
                        w.Left.Color = RedBlackTreeNodeColor.Black;
                        RightRotate(x.Parent);
                        x = _root;
                    }
                }
            }
            x.Color = RedBlackTreeNodeColor.Black;
        }

        private void Insert(RedBlackTreeNode<TKey, TValue> z)
        {
            var x = _root;
            var y = RedBlackTreeNode<TKey, TValue>.Void;
            while (x != RedBlackTreeNode<TKey, TValue>.Void)
            {
                y = x;
                if (z.Key.CompareTo(x.Key) < 0)
                    x = x.Left;
                else
                    x = x.Right;
            }
            z.Parent = y;
            if (y == RedBlackTreeNode<TKey, TValue>.Void)
                _root = z;
            else if (z.Key.CompareTo(y.Key) < 0)
                y.Left = z;
            else
                y.Right = z;
            z.Left = RedBlackTreeNode<TKey, TValue>.Void;
            z.Right = RedBlackTreeNode<TKey, TValue>.Void;
            z.Color = RedBlackTreeNodeColor.Red;
            InsertFixup(z);
        }

        private void InsertFixup(RedBlackTreeNode<TKey, TValue> z)
        {
            while (z.Parent.Color == RedBlackTreeNodeColor.Red)
            {
                if (z.Parent == z.Parent.Parent.Left)
                {
                    var y = z.Parent.Parent.Right;
                    if (y.Color == RedBlackTreeNodeColor.Red)
                    {
                        z.Parent.Color = RedBlackTreeNodeColor.Black;
                        y.Color = RedBlackTreeNodeColor.Black;
                        z.Parent.Parent.Color = RedBlackTreeNodeColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Right)
                        {
                            z = z.Parent;
                            LeftRotate(z);
                        }
                        z.Parent.Color = RedBlackTreeNodeColor.Black;
                        z.Parent.Parent.Color = RedBlackTreeNodeColor.Red;
                        RightRotate(z.Parent.Parent);
                    }
                }
                else
                {
                    var y = z.Parent.Parent.Left;
                    if (y.Color == RedBlackTreeNodeColor.Red)
                    {
                        z.Parent.Color = RedBlackTreeNodeColor.Black;
                        y.Color = RedBlackTreeNodeColor.Black;
                        z.Parent.Parent.Color = RedBlackTreeNodeColor.Red;
                        z = z.Parent.Parent;
                    }
                    else
                    {
                        if (z == z.Parent.Left)
                        {
                            z = z.Parent;
                            RightRotate(z);
                        }
                        z.Parent.Color = RedBlackTreeNodeColor.Black;
                        z.Parent.Parent.Color = RedBlackTreeNodeColor.Red;
                        LeftRotate(z.Parent.Parent);
                    }
                }
            }
            _root.Color = RedBlackTreeNodeColor.Black;
        }

        private void LeftRotate(RedBlackTreeNode<TKey, TValue> x)
        {
            var y = x.Right;
            x.Right = y.Left;
            if (y.Left != RedBlackTreeNode<TKey, TValue>.Void)
                y.Left.Parent = x;
            y.Parent = x.Parent;
            if (x.Parent == RedBlackTreeNode<TKey, TValue>.Void)
                _root = y;
            else if (x == x.Parent.Left)
                x.Parent.Left = y;
            else
                x.Parent.Right = y;
            y.Left = x;
            x.Parent = y;
        }

        private void RightRotate(RedBlackTreeNode<TKey, TValue> x)
        {
            var y = x.Left;
            x.Left = y.Right;
            if (y.Right != RedBlackTreeNode<TKey, TValue>.Void)
                y.Right.Parent = x;
            y.Parent = x.Parent;
            if (x.Parent == RedBlackTreeNode<TKey, TValue>.Void)
                _root = y;
            else if (x == x.Parent.Right)
                x.Parent.Right = y;
            else
                x.Parent.Left = y;
            y.Right = x;
            x.Parent = y;
        }

        #endregion

        #region IEnumerable<TValue>

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var node in EnumerateNodes())
            {
                yield return node.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
