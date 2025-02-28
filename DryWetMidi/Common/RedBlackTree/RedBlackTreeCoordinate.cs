using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTreeCoordinate<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        #region Constructor

        public RedBlackTreeCoordinate(
            RedBlackTreeNode<TKey, TValue> treeNode,
            LinkedListNode<TValue> nodeElement)
        {
            TreeNode = treeNode;
            NodeElement = nodeElement;
        }

        #endregion

        #region Properties

        public RedBlackTreeNode<TKey, TValue> TreeNode { get; }

        public LinkedListNode<TValue> NodeElement { get; }

        public TKey Key
        {
            get { return TreeNode.Key; }
            set { TreeNode.Key = value; }
        }

        public TValue Value => NodeElement.Value;

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            var coordinate = obj as RedBlackTreeCoordinate<TKey, TValue>;
            return
                coordinate != null &&
                coordinate.TreeNode == TreeNode &&
                coordinate.NodeElement == NodeElement;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var result = 17;
                result = result * 23 + TreeNode.GetHashCode();
                result = result * 23 + NodeElement.GetHashCode();
                return result;
            }
        }

        public override string ToString()
        {
            return $"{Key}: {Value}";
        }

        #endregion
    }
}
