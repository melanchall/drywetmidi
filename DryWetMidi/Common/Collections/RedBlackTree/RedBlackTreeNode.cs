using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTreeNode<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        #region Constants

        public static readonly RedBlackTreeNode<TKey, TValue> Void = new RedBlackTreeNode<TKey, TValue>(default(TKey), null);

        #endregion

        #region Constructor

        public RedBlackTreeNode(TKey key, RedBlackTreeNode<TKey, TValue> parent)
        {
            Key = key;
            Parent = parent;
        }

        #endregion

        #region Properties

        public TKey Key { get; set; }

        public LinkedList<TValue> Values { get; private set; } = new LinkedList<TValue>();

        public RedBlackTreeNode<TKey, TValue> Left { get; set; }

        public RedBlackTreeNode<TKey, TValue> Right { get; set; }

        public RedBlackTreeNode<TKey, TValue> Parent { get; set; }

        public bool IsRed { get; set; }

        public RedBlackTree<TKey, TValue> Tree { get; set; }

        public TKey Data { get; set; }

        public bool Flag { get; set; }

        #endregion

        #region Methods

        public RedBlackTreeNode<TKey, TValue> Clone()
        {
            if (this == Void)
                return Void;

            var node = new RedBlackTreeNode<TKey, TValue>(Key, Parent)
            {
                IsRed = IsRed,
                Tree = Tree,
                Data = Data,
                Values = Values
            };

            var leftClone = Left?.Clone();
            leftClone.Parent = node;
            node.Left = leftClone;

            var rightClone = Right?.Clone();
            rightClone.Parent = node;
            node.Right = rightClone;

            return node;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return this != Void ? $"{Key}: {string.Join(", ", Values)}" : "<Void>";
        }

        #endregion
    }
}
