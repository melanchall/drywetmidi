using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class RedBlackTreeNode<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        public static readonly RedBlackTreeNode<TKey, TValue> Void = new RedBlackTreeNode<TKey, TValue>(default(TKey), null);

        public RedBlackTreeNode(TKey key, RedBlackTreeNode<TKey, TValue> parent)
        {
            Key = key;
            Parent = parent;
        }

        public TKey Key { get; set; }

        public LinkedList<TValue> Values { get; } = new LinkedList<TValue>();

        public RedBlackTreeNode<TKey, TValue> Left { get; set; }

        public RedBlackTreeNode<TKey, TValue> Right { get; set; }

        public RedBlackTreeNode<TKey, TValue> Parent { get; set; }

        public RedBlackTreeNodeColor Color { get; set; } = RedBlackTreeNodeColor.Black;

        public RedBlackTree<TKey, TValue> Tree { get; set; }

        public object Data { get; set; }

        public override string ToString()
        {
            return this != Void ? $"{Key}: {string.Join(", ", Values)}" : "<Void>";
        }
    }
}
