using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

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

        public RedBlackTree<TKey, TValue> Clone()
        {
            var result = new RedBlackTree<TKey, TValue>
            {
                _root = _root.Clone(),
                Count = Count
            };

            result._root.Tree = result;
            return result;
        }

        public void Clear()
        {
            _root = RedBlackTreeNode<TKey, TValue>.Void;
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetCoordinate(TKey key, TValue value)
        {
            return GetCoordinatesByKey(key).FirstOrDefault(c => c.Value.Equals(value));
        }

        public IEnumerable<RedBlackTreeCoordinate<TKey, TValue>> GetCoordinatesByKey(TKey key)
        {
            var node = GetNodeByKey(key);
            if (IsVoid(node))
                yield break;

            for (var element = node.Values.First; element != null; element = element.Next)
            {
                yield return new RedBlackTreeCoordinate<TKey, TValue>(node, element);
            }
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

            return NodeOrNull(node);
        }

        public IEnumerable<TValue> GetValuesByKey(TKey key)
        {
            return GetCoordinatesByKey(key).Select(n => n.Value);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMinimumCoordinate()
        {
            return GetMinimumCoordinate(_root);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMinimumCoordinate(RedBlackTreeNode<TKey, TValue> node)
        {
            while (!IsVoid(node?.Left))
                node = node.Left;

            return !IsVoid(node)
                ? new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.First)
                : null;
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMaximumCoordinate()
        {
            return GetMaximumCoordinate(_root);
        }

        public RedBlackTreeCoordinate<TKey, TValue> GetMaximumCoordinate(RedBlackTreeNode<TKey, TValue> node)
        {
            while (!IsVoid(node?.Right))
                node = node.Right;

            return !IsVoid(node)
                ? new RedBlackTreeCoordinate<TKey, TValue>(node, node.Values.Last)
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
            var currentNode = _root;
            var lastNode = RedBlackTreeNode<TKey, TValue>.Void;

            while (!IsVoid(currentNode))
            {
                lastNode = currentNode;

                var compareResult = key.CompareTo(currentNode.Key);
                if (compareResult < 0)
                    currentNode = currentNode.Left;
                else if (compareResult > 0)
                    currentNode = currentNode.Right;
                else
                {
                    Count++;
                    var coordinateOnExistingNode = new RedBlackTreeCoordinate<TKey, TValue>(currentNode, currentNode.Values.AddLast(value));
                    OnValueAdded(coordinateOnExistingNode, value);
                    return coordinateOnExistingNode;
                }
            }

            var newNode = new RedBlackTreeNode<TKey, TValue>(key, lastNode) { Tree = this };
            var result = new RedBlackTreeCoordinate<TKey, TValue>(newNode, newNode.Values.AddLast(value));

            if (IsVoid(lastNode))
                _root = newNode;
            else if (key.CompareTo(lastNode.Key) < 0)
                lastNode.Left = newNode;
            else
                lastNode.Right = newNode;

            newNode.Left = RedBlackTreeNode<TKey, TValue>.Void;
            newNode.Right = RedBlackTreeNode<TKey, TValue>.Void;
            newNode.IsRed = true;

            OnValueAdded(result, value);
            InsertFixup(newNode);

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

            RedBlackTreeNode<TKey, TValue> child = null;
            var nextNode = node;
            var isNextNodeRed = nextNode.IsRed;
            
            if (IsVoid(node.Left))
            {
                child = node.Right;
                Transplant(node, node.Right);
            }
            else if (IsVoid(node.Right))
            {
                child = node.Left;
                Transplant(node, node.Left);
            }
            else
            {
                nextNode = GetMinimumCoordinate(node.Right).TreeNode;
                isNextNodeRed = nextNode.IsRed;
                child = nextNode.Right;
                
                if (nextNode != node.Right)
                {
                    Transplant(nextNode, nextNode.Right);
                    nextNode.Right = node.Right;
                    nextNode.Right.Parent = nextNode;
                }
                else
                    child.Parent = nextNode;

                Transplant(node, nextNode);
                nextNode.Left = node.Left;
                nextNode.Left.Parent = nextNode;
                nextNode.IsRed = node.IsRed;
            }

            OnTransplanted(nextNode);

            if (!isNextNodeRed)
                RemoveFixup(child);

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
            var coordinate = GetMinimumCoordinate();
            if (coordinate == null || IsVoid(coordinate.TreeNode))
                yield break;

            do
            {
                yield return coordinate;
            }
            while ((coordinate = GetNextCoordinate(coordinate)) != null);
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
            RedBlackTreeNode<TKey, TValue> bottomNode,
            RedBlackTreeNode<TKey, TValue> topNode)
        {
        }

        protected virtual void OnTransplanted(RedBlackTreeNode<TKey, TValue> node)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected bool IsVoid(RedBlackTreeNode<TKey, TValue> node)
        {
            return node == null || node == RedBlackTreeNode<TKey, TValue>.Void;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private RedBlackTreeNode<TKey, TValue> NodeOrNull(RedBlackTreeNode<TKey, TValue> node)
        {
            return IsVoid(node) ? null : node;
        }

        private void Transplant(
            RedBlackTreeNode<TKey, TValue> oldRoot,
            RedBlackTreeNode<TKey, TValue> newRoot)
        {
            var parent = oldRoot.Parent;

            if (IsVoid(parent))
                _root = newRoot;
            else if (oldRoot == parent.Left)
                parent.Left = newRoot;
            else
                parent.Right = newRoot;

            newRoot.Parent = parent;
        }

        private void RemoveFixup(RedBlackTreeNode<TKey, TValue> node)
        {
            while (node != _root && !node.IsRed)
            {
                var parent = node.Parent;

                if (node == parent.Left)
                {
                    var sibling = parent.Right;
                    if (IsVoid(sibling))
                        break;

                    if (sibling.IsRed)
                    {
                        sibling.IsRed = false;
                        parent.IsRed = true;
                        LeftRotate(parent);
                        sibling = parent.Right;
                    }

                    if (!sibling.Left.IsRed && !sibling.Right.IsRed)
                    {
                        sibling.IsRed = true;
                        node = parent;
                    }
                    else
                    {
                        if (!sibling.Right.IsRed)
                        {
                            sibling.Left.IsRed = false;
                            sibling.IsRed = true;
                            RightRotate(sibling);
                            sibling = parent.Right;
                        }

                        sibling.IsRed = parent.IsRed;
                        parent.IsRed = false;
                        sibling.Right.IsRed = false;
                        LeftRotate(parent);
                        node = _root;
                    }
                }
                else
                {
                    var sibling = parent.Left;
                    if (IsVoid(sibling))
                        break;

                    if (sibling.IsRed)
                    {
                        sibling.IsRed = false;
                        parent.IsRed = true;
                        RightRotate(parent);
                        sibling = parent.Left;
                    }

                    if (!sibling.Right.IsRed && !sibling.Left.IsRed)
                    {
                        sibling.IsRed = true;
                        node = parent;
                    }
                    else
                    {
                        if (!sibling.Left.IsRed)
                        {
                            sibling.Right.IsRed = false;
                            sibling.IsRed = true;
                            LeftRotate(sibling);
                            sibling = parent.Left;
                        }

                        sibling.IsRed = parent.IsRed;
                        parent.IsRed = false;
                        sibling.Left.IsRed = false;
                        RightRotate(parent);
                        node = _root;
                    }
                }
            }

            node.IsRed = false;
        }

        private void InsertFixup(RedBlackTreeNode<TKey, TValue> node)
        {
            RedBlackTreeNode<TKey, TValue> parent;

            while ((parent = node.Parent).IsRed)
            {
                var grandParent = parent.Parent;

                if (parent == grandParent.Left)
                {
                    var uncle = grandParent.Right;
                    if (uncle.IsRed)
                    {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandParent.IsRed = true;
                        node = grandParent;
                    }
                    else
                    {
                        if (node == parent.Right)
                        {
                            node = parent;
                            LeftRotate(node);
                            parent = node.Parent;
                            grandParent = parent.Parent;
                        }

                        parent.IsRed = false;
                        grandParent.IsRed = true;
                        RightRotate(grandParent);
                    }
                }
                else
                {
                    var uncle = grandParent.Left;
                    if (uncle.IsRed)
                    {
                        parent.IsRed = false;
                        uncle.IsRed = false;
                        grandParent.IsRed = true;
                        node = grandParent;
                    }
                    else
                    {
                        if (node == parent.Left)
                        {
                            node = parent;
                            RightRotate(node);
                            parent = node.Parent;
                            grandParent = parent.Parent;
                        }

                        parent.IsRed = false;
                        grandParent.IsRed = true;
                        LeftRotate(grandParent);
                    }
                }
            }

            _root.IsRed = false;
        }

        private void LeftRotate(RedBlackTreeNode<TKey, TValue> node)
        {
            var rightChild = node.Right;
            var leftGrandchild = rightChild.Left;

            node.Right = leftGrandchild;
            if (!IsVoid(leftGrandchild))
                leftGrandchild.Parent = node;
            
            var parent = node.Parent;
            rightChild.Parent = parent;

            if (IsVoid(parent))
                _root = rightChild;
            else if (node == parent.Left)
                parent.Left = rightChild;
            else
                parent.Right = rightChild;

            rightChild.Left = node;
            node.Parent = rightChild;

            OnRotated(node, rightChild);
        }

        private void RightRotate(RedBlackTreeNode<TKey, TValue> node)
        {
            var leftChild = node.Left;
            var rightGrandchild = leftChild.Right;

            node.Left = rightGrandchild;
            if (!IsVoid(rightGrandchild))
                rightGrandchild.Parent = node;

            var parent = node.Parent;
            leftChild.Parent = parent;

            if (IsVoid(parent))
                _root = leftChild;
            else if (node == parent.Right)
                parent.Right = leftChild;
            else
                parent.Left = leftChild;

            leftChild.Right = node;
            node.Parent = leftChild;

            OnRotated(node, leftChild);
        }

        #endregion

        #region IEnumerable<TValue>

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (var coordinate in GetAllCoordinates())
            {
                yield return coordinate.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
