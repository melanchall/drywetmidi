using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using Melanchall.DryWetMidi.Common;
using System.Drawing;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class RedBlackTreeTests
    {
        private sealed class KeyValue : IEquatable<KeyValue>
        {
            public KeyValue(int key, string value)
            {
                Key = key;
                Value = value;
            }

            public int Key { get; set; }

            public string Value { get; set; }

            public bool Equals(KeyValue other) =>
                ReferenceEquals(this, other);
        }

        [Test]
        public void Enumerate_Empty()
        {
            var tree = new RedBlackTree<int, int>();
            Assert.AreEqual(0, tree.Count, "Invalid initial count.");

            var enumerated = tree.ToArray();

            CollectionAssert.IsEmpty(enumerated, "Enumerated collection is not empty.");
        }

        [Test]
        public void Enumerate([Values(0, 1, 2, 3, 4, 5, 10, 100, 1000, 10000)] int count)
        {
            var random = DryWetMidi.Common.Random.Instance;
            var data = Enumerable.Range(0, count).Select(_ => random.Next(1000)).ToArray();

            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(count, tree.Count, "Invalid initial count.");

            var enumerated = tree.ToArray();

            CollectionAssert.AreEqual(
                data.OrderBy(d => d).ToArray(),
                enumerated,
                "Enumerated collection is invalid.");
        }

        [Test]
        public void InsertOne([Values(0, 10, 40, -2, 1000, 12, 2, 9, 11)] int value)
        {
            var data = new[] { 2, 3, 1, 1, 10, 100, 50, 45, 0 };
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Length, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            tree.Add(value, value);
            Assert.AreEqual(data.Length + 1, tree.Count, "Invalid count after add.");

            CheckAscendingOrder(tree.ToArray());
        }

        [Test]
        public void InsertMultiple()
        {
            var data = new[] { 2, 3, 1, 1, 10, 100, 50, 45, 0 }.ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var dataToAdd = new[] { 0, 10, 40, -2, 1000, 12, 2, 9, 11 };
            var count = data.Count;

            foreach (var d in dataToAdd)
            {
                tree.Add(d, d);
                Assert.AreEqual(++count, tree.Count, $"Invalid count after {d} addition.");
                CheckAscendingOrder(tree.ToArray());
            }
        }

        [Test]
        public void DeleteOne([Values(500, 100, 2, 4, 6, 9, 10, 700, 701, 702, 45, 44, 43)] int value)
        {
            var data = Enumerable.Range(0, 1000).ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            tree.Delete(tree.GetFirstNode(value));
            Assert.AreEqual(data.Count - 1, tree.Count, "Invalid count after deletion.");

            CheckAscendingOrder(tree.ToArray());
        }

        [Test]
        public void DeleteMultiple()
        {
            var data = Enumerable.Range(0, 1000).ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var dataToDelete = new[] { 500, 100, 2, 4, 6, 9, 10, 700, 701, 702, 45, 44, 43 };
            var count = data.Count;

            foreach (var d in dataToDelete)
            {
                tree.Delete(tree.GetFirstNode(d));
                Assert.AreEqual(--count, tree.Count, $"Invalid count after {d} deletion.");

                CheckAscendingOrder(tree.ToArray());
            }
        }

        [Test]
        public void DeleteNonExisting()
        {
            var data = Enumerable.Range(0, 1000).ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var nodeToDelete = tree.GetFirstNode(500);
            tree.Delete(nodeToDelete);
            Assert.AreEqual(data.Count - 1, tree.Count, $"Invalid count after first deletion.");

            tree.Delete(nodeToDelete);
            Assert.AreEqual(data.Count - 1, tree.Count, $"Invalid count after second deletion.");
        }

        [Test]
        public void GetNextNode([Values(1, 2, 4, 8, 16, 32, 64, 128, 10, 100, 1000)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < data.Length; i++)
            {
                var node = tree.GetFirstNode(i);
                var values = EnumerateViaGetNextNode(tree, node).ToArray();
                CheckAscendingOrder(values);
            }
        }

        [Test]
        public void GetPreviousNode([Values(1, 2, 4, 8, 16, 32, 64, 128, 10, 100, 1000)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < data.Length; i++)
            {
                var node = tree.GetFirstNode(i);
                var values = EnumerateViaGetPreviousNode(tree, node).ToArray();
                CheckDescendingOrder(values);
            }
        }

        [Test]
        public void GetValues()
        {
            var aValue = new KeyValue(1, "A");
            var bValue = new KeyValue(2, "B");
            var cValue = new KeyValue(3, "C");
            var dValue = new KeyValue(-3, "D");
            var eValue = new KeyValue(1, "E");
            var fValue = new KeyValue(0, "F");
            var gValue = new KeyValue(10, "G");
            var hValue = new KeyValue(1, "H");
            var iValue = new KeyValue(2, "I");

            var data = new[]
            {
                aValue,
                bValue,
                cValue,
                dValue,
                eValue,
                fValue,
                gValue,
                hValue,
                iValue,
            };
            var tree = new RedBlackTree<int, KeyValue>(data, d => d.Key);

            var values1 = tree.GetValues(1);
            CollectionAssert.AreEqual(new[] { aValue, eValue, hValue }, values1, "Invalid values by key 1.");

            var values2 = tree.GetValues(2);
            CollectionAssert.AreEqual(new[] { bValue, iValue }, values2, "Invalid values by key 2.");

            var values3 = tree.GetValues(3);
            CollectionAssert.AreEqual(new[] { cValue }, values3, "Invalid values by key 3.");
        }

        [Test]
        public void Clone([Values(1, 2, 4, 8, 16, 32, 64, 128, 10, 100, 1000, 10000)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();

            var tree = new RedBlackTree<int, int>(data, d => d);
            var treeElements = tree.ToArray();

            var treeClone = tree.Clone();
            var treeCloneElements = treeClone.ToArray();

            CollectionAssert.AreEqual(data, treeElements, "Original tree elements are invalid.");
            CollectionAssert.AreEqual(data, treeCloneElements, "Tree clone elements are invalid.");
            CollectionAssert.AreEqual(treeElements, treeCloneElements, "Tree clone elements aren't equal to original ones.");

            var treeNodes = tree.EnumerateNodes().ToArray();
            var treeCloneNodes = treeClone.EnumerateNodes().ToArray();
            var nodesIntersection = treeNodes.Intersect(treeCloneNodes).ToArray();
            CollectionAssert.IsEmpty(nodesIntersection, "There are the same nodes.");
        }

        [Test]
        public void GetLastNodeBelowThreshold_NoRepeats([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < count; i++)
            {
                var result = tree.GetLastNodeBelowThreshold(i);
                if (i == 0)
                    Assert.IsNull(result, $"Invalid result for {i}.");
                else
                    Assert.AreEqual(i - 1, result.Key, $"Invalid result for {i}.");
            }
        }

        [Test]
        public void GetLastNodeBelowThreshold_Repeats()
        {
            var tree = new RedBlackTree<int, int>(new[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 }, d => d);

            var result = tree.GetLastNodeBelowThreshold(1);
            Assert.IsNull(result, "Invalid result for 1.");

            void Check(int threshold, int expectedResult, int[] expectedPreviousValues)
            {
                var node = tree.GetLastNodeBelowThreshold(threshold);
                var previousValues = EnumerateViaGetPreviousNode(tree, node).ToArray();
                Assert.AreEqual(expectedResult, node.Value, $"Invalid result for {threshold}.");
                CollectionAssert.AreEqual(
                    expectedPreviousValues,
                    previousValues,
                    $"Invalid previous values list for {threshold}.");
            }

            Check(2, 1, new[] { 1, 1 });
            Check(3, 2, new[] { 2, 2, 2, 1, 1 });
            Check(4, 3, new[] { 3, 2, 2, 2, 1, 1 });
            Check(5, 4, new[] { 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(6, 5, new[] { 5, 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(10, 5, new[] { 5, 4, 4, 4, 3, 2, 2, 2, 1, 1 });
        }

        [Test]
        public void GetLastNodeBelowThreshold_InMiddle()
        {
            var tree = new RedBlackTree<int, int>(new[] { 0, 5500 }, d => d);

            var result = tree.GetLastNodeBelowThreshold(700);
        }

        private static IEnumerable<TValue> EnumerateViaGetNextNode<TKey, TValue>(RedBlackTree<TKey, TValue> tree, RedBlackTreeNode<TKey, TValue> node)
            where TKey : IComparable<TKey>
            where TValue : IEquatable<TValue>
        {
            do
            {
                yield return node.Value;
            }
            while ((node = tree.GetNextNode(node)) != null);
        }

        private static IEnumerable<int> EnumerateViaGetPreviousNode(RedBlackTree<int, int> tree, RedBlackTreeNode<int, int> node)
        {
            do
            {
                yield return node.Value;
            }
            while ((node = tree.GetPreviousNode(node)) != null);
        }

        private static void CheckAscendingOrder(int[] values)
        {
            for (var i = 0; i < values.Length - 1; i++)
            {
                Assert.GreaterOrEqual(values[i + 1], values[i], $"Ascending order is broken on index {i}.");
            }
        }

        private static void CheckDescendingOrder(int[] values)
        {
            for (var i = 0; i < values.Length - 1; i++)
            {
                Assert.LessOrEqual(values[i + 1], values[i], $"Descending order is broken on index {i}.");
            }
        }
    }
}
