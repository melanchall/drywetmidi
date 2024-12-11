using System;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTreeNode<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public static readonly RedBlackTreeNode<TKey, TValue> Void = new RedBlackTreeNode<TKey, TValue>(default(TKey), default(TValue), null);

        public RedBlackTreeNode(TKey key, TValue value, RedBlackTreeNode<TKey, TValue> parent)
        {
            Key = key;
            Value = value;
            Parent = parent;
        }

        public TKey Key { get; set; }

        public TValue Value { get; set; }

        public RedBlackTreeNode<TKey, TValue> Left { get; set; }

        public RedBlackTreeNode<TKey, TValue> Right { get; set; }

        public RedBlackTreeNode<TKey, TValue> Parent { get; set; }

        public RedBlackTreeNodeColor Color { get; set; } = RedBlackTreeNodeColor.Black;

        public RedBlackTreeNode<TKey, TValue> Clone()
        {
            if (this == Void)
                return Void;

            var node = new RedBlackTreeNode<TKey, TValue>(Key, Value, Parent)
            {
                Color = Color,
                Left = Left.Clone(),
                Right = Right.Clone(),
            };

            node.Left.Parent = node.Right.Parent = node;
            return node;
        }

        public override string ToString()
        {
            return this != Void ? $"{Key}: {Value}" : "<Void>";
        }
    }
}
