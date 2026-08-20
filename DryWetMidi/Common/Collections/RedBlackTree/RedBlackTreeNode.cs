using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTreeNode<TKey, TValue>
        where TKey : notnull, IComparable<TKey>
        where TValue : notnull
    {
        #region Constructor

        public RedBlackTreeNode(TKey key, RedBlackTreeNode<TKey, TValue>? parent, RedBlackTreeNode<TKey, TValue> voidNode)
        {
            Key = key;
            Left = voidNode;
            Right = voidNode;
            Parent = parent;
        }

        private RedBlackTreeNode()
        {
            Key = default!;
            Left = this;
            Right = this; 
            IsVoidNode = true;
        }

        #endregion

        #region Properties

        public bool IsVoidNode { get; }

        public TKey Key { get; set; }

        public LinkedList<TValue> Values { get; private set; } = new LinkedList<TValue>();

        public RedBlackTreeNode<TKey, TValue> Left { get; set; }

        public RedBlackTreeNode<TKey, TValue> Right { get; set; }

        public RedBlackTreeNode<TKey, TValue>? Parent { get; set; }

        public bool IsRed { get; set; }

        public RedBlackTree<TKey, TValue>? Tree { get; set; }

        public TKey? Data { get; set; }

        public bool Flag { get; set; }

        #endregion

        #region Methods

        public RedBlackTreeNode<TKey, TValue> Clone(RedBlackTreeNode<TKey, TValue> oldVoid, RedBlackTreeNode<TKey, TValue> newVoid)
        {
            if (this == oldVoid)
                return newVoid;

            var node = new RedBlackTreeNode<TKey, TValue>(Key, Parent, newVoid)
            {
                IsRed = IsRed,
                Tree = Tree,
                Data = Data,
                Values = Values
            };

            var leftClone = Left.Clone(oldVoid, newVoid);
            leftClone.Parent = node;
            node.Left = leftClone;

            var rightClone = Right.Clone(oldVoid, newVoid);
            rightClone.Parent = node;
            node.Right = rightClone;

            return node;
        }

        public static RedBlackTreeNode<TKey, TValue> CreateVoidNode()
        {
            return new RedBlackTreeNode<TKey, TValue>();
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return !IsVoidNode ? $"{Key}: {string.Join(", ", Values)}" : "<Void>";
        }

        #endregion
    }
}
