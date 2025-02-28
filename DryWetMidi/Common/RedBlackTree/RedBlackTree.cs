using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal class RedBlackTree<TKey, TValue> : IEnumerable<TValue>
        where TKey : IComparable<TKey>
    {
        #region Fields

        protected RedBlackTreeNode<TKey, TValue> _root = RedBlackTreeNode<TKey, TValue>.Void;

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

        #region Properties

        public int Count { get; private set; }

        #endregion

        #region Methods

        public void Clear()
        {
            _root = RedBlackTreeNode<TKey, TValue>.Void;
        }

        public RedBlackTreeNode<TKey, TValue> GetNodeByKey(TKey key)
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

        public IEnumerable<RedBlackTreeCoordinate<TKey, TValue>> GetCoordinatesByKey(TKey key)
        {
            var node = GetNodeByKey(key);
            if (IsVoid(node))
                yield break;

            for (var n = node.Values.First; n != null; n = n.Next)
            {
                yield return new RedBlackTreeCoordinate<TKey, TValue>(node, n);
            }
        }

        public IEnumerable<TValue> GetValuesByKey(TKey key)
        {
            return GetCoordinatesByKey(key).Select(n => n.Value);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetCoordinate(TKey key, TValue value)
        {
            return GetCoordinatesByKey(key).FirstOrDefault(n => n.Value.Equals(value));
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMinimumCoordinate()
        {
            return GetMinimumCoordinate(_root);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMinimumCoordinate(RedBlackTreeNode<TKey, TValue> startNode)
        {
            while (!IsVoid(startNode?.Left))
                startNode = startNode.Left;

            var result = NodeOrNull(startNode);
            return result != null
                ? new RedBlackTreeCoordinate<TKey, TValue>(result, result.Values.First)
                : null;
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMaximumCoordinate()
        {
            return GetMaximumCoordinate(_root);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMaximumCoordinate(RedBlackTreeNode<TKey, TValue> startNode)
        {
            while (!IsVoid(startNode?.Right))
                startNode = startNode.Right;

            var result = NodeOrNull(startNode);
            return result != null
                ? new RedBlackTreeCoordinate<TKey, TValue>(result, result.Values.Last)
                : null;
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetNextCoordinate(RedBlackTreeCoordinate<TKey, TValue> coordinate)
        {
            if (coordinate == null || IsVoid(coordinate.TreeNode))
                return null;

            var nextElement = coordinate.NodeElement.Next;
            if (nextElement != null)
                return new RedBlackTreeCoordinate<TKey, TValue>(coordinate.TreeNode, nextElement);

            var node = coordinate.TreeNode;

            var right = node.Right;
            if (!IsVoid(right))
                return GetMinimumCoordinate(right);

            var nextNode = node.Parent;

            while (!IsVoid(nextNode))
            {
                if (node == nextNode.Left)
                    return new RedBlackTreeCoordinate<TKey, TValue>(nextNode, nextNode.Values.First);

                node = nextNode;
                nextNode = node.Parent;
            }

            return IsVoid(nextNode)
                ? null
                : new RedBlackTreeCoordinate<TKey, TValue>(nextNode, nextNode.Values.First);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetPreviousCoordinate(RedBlackTreeCoordinate<TKey, TValue> coordinate)
        {
            if (coordinate == null || IsVoid(coordinate.TreeNode))
                return null;

            var previousElement = coordinate.NodeElement.Previous;
            if (previousElement != null)
                return new RedBlackTreeCoordinate<TKey, TValue>(coordinate.TreeNode, previousElement);

            var node = coordinate.TreeNode;

            var left = node.Left;
            if (!IsVoid(left))
                return GetMaximumCoordinate(left);

            var previousNode = node.Parent;

            while (!IsVoid(previousNode))
            {
                if (node == previousNode.Right)
                    return new RedBlackTreeCoordinate<TKey, TValue>(previousNode, previousNode.Values.Last);

                node = previousNode;
                previousNode = node.Parent;
            }

            return IsVoid(previousNode)
                ? null
                : new RedBlackTreeCoordinate<TKey, TValue>(previousNode, previousNode.Values.Last);
        }

        public RedBlackTreeCoordinate<TKey, TValue> Add(TKey key, TValue value)
        {
            var x = _root;
            var y = RedBlackTreeNode<TKey, TValue>.Void;

            while (x != RedBlackTreeNode<TKey, TValue>.Void)
            {
                y = x;

                var compareResult = key.CompareTo(x.Key);
                if (compareResult < 0)
                    x = x.Left;
                else if (compareResult > 0)
                    x = x.Right;
                else
                {
                    Count++;
                    var existingNode = new RedBlackTreeCoordinate<TKey, TValue>(x, x.Values.AddLast(value));
                    OnValueAdded(existingNode, value);
                    return existingNode;
                }
            }

            var z = new RedBlackTreeNode<TKey, TValue>(key, y) { Tree = this };
            var result = new RedBlackTreeCoordinate<TKey, TValue>(z, z.Values.AddLast(value));

            if (y == RedBlackTreeNode<TKey, TValue>.Void)
                _root = z;
            else if (key.CompareTo(y.Key) < 0)
                y.Left = z;
            else
                y.Right = z;

            z.Left = RedBlackTreeNode<TKey, TValue>.Void;
            z.Right = RedBlackTreeNode<TKey, TValue>.Void;
            z.Color = RedBlackTreeNodeColor.Red;

            OnValueAdded(result, value);
            InsertFixup(z);

            Count++;
            return result;
        }

        public bool Remove(RedBlackTreeCoordinate<TKey, TValue> coordinate)
        {
            if (coordinate == null || coordinate.NodeElement.List == null)
                return false;

            var node = coordinate.TreeNode;
            if (IsVoid(node) || node.Tree != this)
                return false;

            node.Values.Remove(coordinate.NodeElement);
            if (node.Values.Count > 0)
            {
                OnValueRemoved(node);
                return true;
            }

            return Remove(node);
        }

        public bool Remove(RedBlackTreeNode<TKey, TValue> node)
        {
            if (IsVoid(node) || node.Tree != this)
                return false;

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
                y = GetMinimumCoordinate(node.Right).TreeNode;
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

            OnTransplanted(y);

            if (yOriginalColor == RedBlackTreeNodeColor.Black)
                RemoveFixup(x);

            node.Tree = null;
            Count--;

            return true;
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetLastCoordinateBelowThreshold(TKey threshold)
        {
            var node = _root;

            while (!IsVoid(node))
            {
                var compareResult = threshold.CompareTo(node.Key);
                if (compareResult == 0)
                    return GetPreviousCoordinate(new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.First));

                var nextNode = compareResult > 0 ? node.Right : node.Left;

                if (IsVoid(nextNode))
                {
                    if (compareResult > 0)
                        return new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.Last);
                    else if (compareResult < 0)
                        return GetPreviousCoordinate(new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.First));
                }

                node = nextNode;
            }

            return null;
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetFirstCoordinateAboveThreshold(TKey threshold)
        {
            var node = _root;

            while (!IsVoid(node))
            {
                var compareResult = threshold.CompareTo(node.Key);
                if (compareResult == 0)
                    return GetNextCoordinate(new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.Last));

                var nextNode = compareResult > 0 ? node.Right : node.Left;

                if (IsVoid(nextNode))
                {
                    if (compareResult > 0)
                        return GetNextCoordinate(new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.Last));
                    else if (compareResult < 0)
                        return new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.First);
                }

                node = nextNode;
            }

            return null;
        }

        internal IEnumerable<RedBlackTreeCoordinate<TKey, TValue>> GetAllCoordinates()
        {
            var node = GetMinimumCoordinate();
            if (node == null || IsVoid(node.TreeNode))
                yield break;

            do
            {
                yield return node;
            }
            while ((node = GetNextCoordinate(node)) != null);
        }

        internal RedBlackTreeNode<TKey, TValue> GetRoot()
        {
            return NodeOrNull(_root);
        }

        protected virtual void OnValueAdded(
            RedBlackTreeCoordinate<TKey, TValue> coordinate,
            TValue value)
        {
        }

        protected virtual void OnValueRemoved(
            RedBlackTreeNode<TKey, TValue> node)
        {
        }

        protected virtual void OnRotated(
            RedBlackTreeNode<TKey, TValue> x,
            RedBlackTreeNode<TKey, TValue> y)
        {
        }

        protected virtual void OnTransplanted(RedBlackTreeNode<TKey, TValue> node)
        {
        }

        protected bool IsVoid(RedBlackTreeNode<TKey, TValue> node)
        {
            return node == null || node == RedBlackTreeNode<TKey, TValue>.Void;
        }

        private RedBlackTreeNode<TKey, TValue> NodeOrNull(RedBlackTreeNode<TKey, TValue> node)
        {
            return IsVoid(node) ? null : node;
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

        private void RemoveFixup(RedBlackTreeNode<TKey, TValue> x)
        {
            while (x != _root && x.Color == RedBlackTreeNodeColor.Black)
            {
                if (x == x.Parent.Left)
                {
                    var w = x.Parent.Right;
                    if (IsVoid(w))
                        break;

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
                    if (IsVoid(w))
                        break;

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

            OnRotated(x, y);
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

            OnRotated(x, y);
        }

        #endregion

        #region IEnumerable<TValue>

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var node in GetAllCoordinates())
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
