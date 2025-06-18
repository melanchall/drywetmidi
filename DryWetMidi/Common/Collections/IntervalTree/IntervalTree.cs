using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class IntervalTree<TKey, TValue> : RedBlackTree<TKey, TValue>
        where TKey : IComparable<TKey>
        where TValue : IInterval<TKey>
    {
        #region Fields

        private bool _blockUpdatingMax = false;

        #endregion

        #region Constructor

        public IntervalTree()
            : base()
        {
        }

        public IntervalTree(IEnumerable<TValue> values)
            : base()
        {
            foreach (var v in values)
            {
                AddWithoutMaxUpdating(v);
            }

            InitializeMax();
        }

        #endregion

        #region Methods

        public RedBlackTreeCoordinate<TKey, TValue> Add(TValue value)
        {
            return Add(value.Start, value);
        }

        public RedBlackTreeCoordinate<TKey, TValue> AddWithoutMaxUpdating(TValue value)
        {
            _blockUpdatingMax = true;

            var result = Add(value);

            _blockUpdatingMax = false;
            return result;
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

                if (point.CompareTo(node.Data) >= 0)
                    continue;

                queue.Enqueue(node.Left);

                for (var element = node.Values.First; element != null; element = element.Next)
                {
                    var interval = element.Value;
                    if (interval.Start.CompareTo(point) < 0 && interval.End.CompareTo(point) > 0)
                        yield return new RedBlackTreeCoordinate<TKey, TValue>(node, element);
                }

                if (point.CompareTo(node.Key) <= 0)
                    continue;

                queue.Enqueue(node.Right);
            }
        }

        public void InitializeMax()
        {
            var nodeStack = new Stack<RedBlackTreeNode<TKey, TValue>>();
            nodeStack.Push(_root);

            while (nodeStack.Count != 0)
            {
                var currNode = nodeStack.Peek();
                if (!IsVoid(currNode.Left) && !currNode.Left.Flag)
                    nodeStack.Push(currNode.Left);
                else
                {
                    if (!IsVoid(currNode.Right) && !currNode.Right.Flag)
                        nodeStack.Push(currNode.Right);
                    else
                    {
                        currNode.Flag = true;
                        UpdateMax(currNode);
                        nodeStack.Pop();
                    }
                }
            }
        }

        protected override void OnValueAdded(RedBlackTreeCoordinate<TKey, TValue> coordinate, TValue value)
        {
            base.OnValueAdded(coordinate, value);

            if (_blockUpdatingMax)
                return;

            var end = value.End;
            if (!UpdateMaxByNewValue(coordinate.TreeNode, end))
                return;

            UpdateMaxUp(coordinate.TreeNode);
        }

        protected override void OnValueRemoved(RedBlackTreeNode<TKey, TValue> node)
        {
            base.OnValueRemoved(node);

            if (_blockUpdatingMax)
                return;

            if (UpdateMax(node))
                UpdateMaxUp(node);
        }

        protected override void OnRotated(
            RedBlackTreeNode<TKey, TValue> bottomNode,
            RedBlackTreeNode<TKey, TValue> topNode)
        {
            base.OnRotated(bottomNode, topNode);

            if (_blockUpdatingMax)
                return;

            UpdateMax(bottomNode);

            if (bottomNode.Data.CompareTo(topNode.Data) > 0)
                topNode.Data = bottomNode.Data;
        }

        protected override void OnTransplanted(RedBlackTreeNode<TKey, TValue> node)
        {
            base.OnTransplanted(node);

            if (_blockUpdatingMax)
                return;

            UpdateMax(node);
            UpdateMaxUp(node);
        }

        public bool UpdateMax(RedBlackTreeNode<TKey, TValue> node)
        {
            if (node.Values.Count == 0)
                return false;

            var result = node.Values.First.Value.End;

            foreach (var value in node.Values)
            {
                if (value.End.CompareTo(result) > 0)
                    result = value.End;
            }

            var left = node.Left;
            if (!IsVoid(left))
            {
                var leftMax = left.Data;
                if (leftMax.CompareTo(result) > 0)
                    result = leftMax;
            }

            var right = node.Right;
            if (!IsVoid(right))
            {
                var rightMax = right.Data;
                if (rightMax.CompareTo(result) > 0)
                    result = rightMax;
            }

            if (node.Data.Equals(result))
                return false;

            node.Data = result;
            return true;
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
            else if (data.CompareTo(end) < 0)
                node.Data = end;
            else
                return false;

            return true;
        }

        #endregion
    }
}
