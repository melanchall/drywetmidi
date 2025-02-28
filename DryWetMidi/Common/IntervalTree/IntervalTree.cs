using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class IntervalTree<TKey, TValue> : RedBlackTree<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : IInterval<TKey>
    {
        #region Constructor

        public IntervalTree()
            : base()
        {
        }

        public IntervalTree(IEnumerable<TValue> values)
            : base(values, v => v.Start)
        {
        }

        #endregion

        #region Methods

        public RedBlackTreeCoordinate<TKey, TValue> Add(TValue value)
        {
            return Add(value.Start, value);
        }

        public IEnumerable<RedBlackTreeCoordinate<TKey, TValue>> Search(TKey point)
        {
            var queue = new Queue<RedBlackTreeNode<TKey, TValue>>();
            queue.Enqueue(_root);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();

                if (IsVoid(node) || node.Tree != this)
                    continue;

                if (point.CompareTo((TKey)node.Data) >= 0)
                    continue;

                queue.Enqueue(node.Left);

                for (var valueNode = node.Values.First; valueNode != null; valueNode = valueNode.Next)
                {
                    var interval = valueNode.Value;
                    if (interval.Start.CompareTo(point) < 0 && interval.End.CompareTo(point) > 0)
                        yield return new RedBlackTreeCoordinate<TKey, TValue>(node, valueNode);
                }

                if (point.CompareTo(node.Key) <= 0)
                    continue;

                queue.Enqueue(node.Right);
            }
        }

        protected override void OnValueAdded(RedBlackTreeCoordinate<TKey, TValue> coordinate, TValue value)
        {
            base.OnValueAdded(coordinate, value);

            var end = value.End;
            if (!UpdateMaxByNewValue(coordinate.TreeNode, end))
                return;

            UpdateMaxUp(coordinate.TreeNode);
        }

        protected override void OnValueRemoved(RedBlackTreeNode<TKey, TValue> node)
        {
            base.OnValueRemoved(node);

            if (UpdateMax(node))
                UpdateMaxUp(node);
        }

        protected override void OnRotated(
            RedBlackTreeNode<TKey, TValue> x,
            RedBlackTreeNode<TKey, TValue> y)
        {
            base.OnRotated(x, y);

            UpdateMax(x);

            if (((TKey)x.Data).CompareTo((TKey)y.Data) > 0)
                y.Data = (TKey)x.Data;
        }

        protected override void OnTransplanted(RedBlackTreeNode<TKey, TValue> node)
        {
            base.OnTransplanted(node);

            UpdateMax(node);
            UpdateMaxUp(node);
        }

        public void UpdateMaxUp(RedBlackTreeNode<TKey, TValue> node)
        {
            while (true)
            {
                var parent = node.Parent;
                if (IsVoid(parent) || !UpdateMax(parent))
                    break;

                node = parent;
            }
        }

        private bool UpdateMaxByNewValue(RedBlackTreeNode<TKey, TValue> node, TKey end)
        {
            var data = node.Data;
            if (data == null)
                node.Data = end;
            else if (((TKey)data).CompareTo(end) < 0)
                node.Data = end;
            else
                return false;

            return true;
        }

        public bool UpdateMax(RedBlackTreeNode<TKey, TValue> node)
        {
            if (!node.Values.Any())
                return false;

            var result = node.Values.Max(v => v.End);

            var left = node.Left;
            if (!IsVoid(left))
            {
                var leftMax = (TKey)left.Data;
                if (leftMax.CompareTo(result) > 0)
                    result = leftMax;
            }

            var right = node.Right;
            if (!IsVoid(right))
            {
                var rightMax = (TKey)right.Data;
                if (rightMax.CompareTo(result) > 0)
                    result = rightMax;
            }

            if (node.Data.Equals(result))
                return false;

            node.Data = result;
            return true;
        }

        #endregion
    }
}
