using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class RedBlackTreeTests
    {
        #region Nested classes

        private sealed class KeyValue
        {
            public KeyValue(int key, string value)
            {
                Key = key;
                Value = value;
            }

            public int Key { get; set; }

            public string Value { get; set; }
        }

        #endregion

        #region Test methods

        [Test]
        public void Enumerate_1()
        {
            var tree = new RedBlackTree<int, int>();
            Assert.AreEqual(0, tree.Count, "Invalid initial count.");

            var enumerated = tree.ToArray();

            CollectionAssert.IsEmpty(enumerated, "Enumerated collection is not empty.");
        }

        [Test]
        public void Enumerate_2([Values(0, 1, 2, 3, 4, 5, 10, 100, 1000, 10000)] int count)
        {
            var random = DryWetMidi.Common.Random.Instance;
            var data = Enumerable.Range(0, count).Select(_ => random.Next(1000)).ToArray();

            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(count, tree.Count, "Invalid initial count.");

            CollectionAssert.AreEqual(
                data.OrderBy(d => d).ToArray(),
                tree.ToArray(),
                "Enumerated collection is invalid.");

            Assert.IsTrue(
                tree.GetAllCoordinates().All(c => c.TreeNode.Tree == tree),
                "Some nodes have invalid tree reference.");
        }

        [Test]
        public void Add_1([Values(0, 10, 40, -2, 1000, 12, 2, 9, 11)] int value)
        {
            var data = new[] { 2, 3, 1, 1, 10, 100, 50, 45, 0 };
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Length, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var coordinate = tree.Add(value, value);
            Assert.AreEqual(tree, coordinate.TreeNode.Tree, "Invalid tree reference.");
            Assert.AreEqual(data.Length + 1, tree.Count, "Invalid count after add.");

            CheckAscendingOrder(tree.ToArray());
        }

        [Test]
        public void Add_2()
        {
            var data = new[] { 2, 3, 1, 1, 10, 100, 50, 45, 0 }.ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var dataToAdd = new[] { 0, 10, 40, -2, 1000, 12, 2, 9, 11 };
            var count = data.Count;

            foreach (var d in dataToAdd)
            {
                var coordinate = tree.Add(d, d);
                Assert.AreEqual(tree, coordinate.TreeNode.Tree, "Invalid tree reference.");
                Assert.AreEqual(++count, tree.Count, $"Invalid count after {d} addition.");
                CheckAscendingOrder(tree.ToArray());
            }
        }

        [Test]
        public void Add_3(
            [Values(1, 10, 100, 1000)] int treeSize,
            [Values(1, 2, 5, 10)] int step)
        {
            var tree = new RedBlackTree<int, int>();

            for (var i = 0; i < treeSize; i++)
            {
                var d = 10 * i;
                tree.Add(d, d);
            }

            var maxEnd = tree
                .GetAllCoordinates()
                .Max(n => n.Value);

            var steps = maxEnd / step;

            for (var i = 0; i < steps; i++)
            {
                var coordinate = tree.Add(step, step);
                Assert.AreEqual(tree, coordinate.TreeNode.Tree, $"Invalid tree reference (addition {i}).");
            }

            CheckAscendingOrder(tree.ToArray());
        }

        [Test]
        public void Remove_1([Values(500, 100, 2, 4, 6, 9, 10, 700, 701, 702, 45, 44, 43)] int value)
        {
            var data = Enumerable.Range(0, 1000).ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var nodeX = tree.GetNodeByKey(value);
            var node = new RedBlackTreeCoordinate<int, int>(nodeX, nodeX.Values.First);
            Assert.IsTrue(tree.Remove(node), "Value is not removed.");
            Assert.AreEqual(data.Count - 1, tree.Count, "Invalid count after removing.");
            Assert.IsNull(node.TreeNode.Tree, $"Invalid tree reference after removing.");

            CheckAscendingOrder(tree.ToArray());
        }

        [Test]
        public void Remove_2()
        {
            var data = Enumerable.Range(0, 1000).ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var dataToDelete = new[] { 500, 100, 2, 4, 6, 9, 10, 700, 701, 702, 45, 44, 43 };
            var count = data.Count;

            foreach (var d in dataToDelete)
            {
                var nodeX = tree.GetNodeByKey(d);
                var node = new RedBlackTreeCoordinate<int, int>(nodeX, nodeX.Values.First);
                Assert.IsTrue(tree.Remove(node), "Value is not removed.");
                Assert.AreEqual(--count, tree.Count, $"Invalid count (removing {d}).");
                Assert.IsNull(node.TreeNode.Tree, $"Invalid tree reference (removing {d}).");

                CheckAscendingOrder(tree.ToArray());
            }
        }

        [Test]
        public void Remove_3()
        {
            var data = Enumerable.Range(0, 1000).ToList();
            var tree = new RedBlackTree<int, int>(data, d => d);
            Assert.AreEqual(data.Count, tree.Count, "Invalid initial count.");

            CheckAscendingOrder(tree.ToArray());

            var nodeToDeleteX = tree.GetNodeByKey(500);
            var nodeToDelete = new RedBlackTreeCoordinate<int, int>(nodeToDeleteX, nodeToDeleteX.Values.First);
            Assert.IsTrue(tree.Remove(nodeToDelete), "Value is not removed.");
            Assert.AreEqual(data.Count - 1, tree.Count, $"Invalid count after first removing.");
            Assert.IsNull(nodeToDelete.TreeNode.Tree, $"Invalid tree reference after first removing.");

            Assert.IsFalse(tree.Remove(nodeToDelete), "Value is removed.");
            Assert.AreEqual(data.Count - 1, tree.Count, $"Invalid count after second removing.");
        }

        [Test]
        public void Remove_4()
        {
            var data = new[] { 1, 2, 3, 3, 4, 5, 5, 5 };
            var tree = new RedBlackTree<int, int>(data, d => d);

            var node = tree.GetNodeByKey(3);
            Assert.AreEqual(2, node.Values.Count, "Invalid elements count for 3.");

            Assert.IsTrue(tree.Remove(new RedBlackTreeCoordinate<int, int>(node, node.Values.First)), "Value is not removed (first removing).");
            Assert.AreEqual(1, node.Values.Count, "Invalid elements count for 3 (first removing).");
            Assert.IsNotNull(tree.GetNodeByKey(3), "Node 3 is not found (first removing).");
            Assert.AreEqual(tree, tree.GetNodeByKey(3).Tree, $"Invalid tree reference (first removing).");

            Assert.IsTrue(tree.Remove(new RedBlackTreeCoordinate<int, int>(node, node.Values.Last)), "Value is not removed (second removing).");
            CollectionAssert.IsEmpty(node.Values, "Invalid elements count for 3 (second removing).");
            Assert.IsNull(tree.GetNodeByKey(3), "Node 3 is found (second removing).");
            Assert.IsNull(node.Tree, $"Invalid tree reference (second removing).");
        }

        [Test]
        public void Remove_5([Values(1, 10, 100)] int count)
        {
            var data = Enumerable.Range(0, count).Select(_ => 3).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            var node = tree.GetNodeByKey(3);
            Assert.AreEqual(count, node.Values.Count, "Invalid elements count for 3.");

            for (var i = 0; i < count - 1; i++)
            {
                Assert.IsTrue(tree.Remove(new RedBlackTreeCoordinate<int, int>(node, node.Values.First)), $"Value is not removed (removing {i}).");
                Assert.AreEqual(count - i - 1, node.Values.Count, $"Invalid elements count for 3 (removing {i}).");
                Assert.IsNotNull(tree.GetNodeByKey(3), $"Node 3 is not found (removing {i}).");
                Assert.AreEqual(tree, tree.GetNodeByKey(3).Tree, $"Invalid tree reference (first removing).");
            }

            Assert.IsTrue(tree.Remove(new RedBlackTreeCoordinate<int, int>(node, node.Values.Last)), "Value is not removed (last removing).");
            CollectionAssert.IsEmpty(node.Values, "Invalid elements count for 3 (last removing).");
            Assert.IsNull(tree.GetNodeByKey(3), "Node 3 is found (last removing).");
            Assert.IsNull(node.Tree, $"Invalid tree reference (second removing).");
        }

        [Test]
        public void Remove_6(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 3, 5, 10)] int step)
        {
            var tree = new RedBlackTree<int, int>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    tree.Add(i, i);
                }
            }

            var coordinates = tree.GetAllCoordinates().ToArray();

            for (var i = 0; i < coordinates.Length; i += step)
            {
                var coordinate = coordinates[i];
                Assert.IsTrue(tree.Remove(coordinate), $"Value is not removed (removing {i}).");
                CheckAscendingOrder(tree.ToArray());
            }
        }

        [Test]
        public void Remove_7(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 5, 10)] int intervalLength)
        {
            var tree = new RedBlackTree<int, int>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    tree.Add(i, i);
                }
            }

            var k = 0;

            while (true)
            {
                var root = tree.GetRoot();
                if (root == null)
                    break;

                var coordinate = new RedBlackTreeCoordinate<int, int>(root, root.Values.First);
                Assert.IsTrue(tree.Remove(coordinate), $"Value is not removed (removing {k}).");
                CheckAscendingOrder(tree.ToArray());
                
                k++;
            }
        }

        [Test]
        public void GetNextCoordinate([Values(1, 2, 4, 8, 16, 32, 64, 128, 10, 100, 1000)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < data.Length; i++)
            {
                var node = tree.GetNodeByKey(i);
                var coordinate = new RedBlackTreeCoordinate<int, int>(node, node.Values.First);
                var values = EnumerateViaGetNextCoordinate(tree, coordinate).ToArray();
                CheckAscendingOrder(values);
            }
        }

        [Test]
        public void GetPreviousCoordinate([Values(1, 2, 4, 8, 16, 32, 64, 128, 10, 100, 1000)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < data.Length; i++)
            {
                var node = tree.GetNodeByKey(i);
                var coordinate = new RedBlackTreeCoordinate<int, int>(node, node.Values.Last);
                var values = EnumerateViaGetPreviousNode(tree, coordinate).ToArray();
                CheckDescendingOrder(values);
            }
        }

        [Test]
        public void GetValuesByKey()
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

            var values1 = tree.GetValuesByKey(1);
            CollectionAssert.AreEqual(new[] { aValue, eValue, hValue }, values1, "Invalid values by key 1.");

            var values2 = tree.GetValuesByKey(2);
            CollectionAssert.AreEqual(new[] { bValue, iValue }, values2, "Invalid values by key 2.");

            var values3 = tree.GetValuesByKey(3);
            CollectionAssert.AreEqual(new[] { cValue }, values3, "Invalid values by key 3.");
        }

        [Test]
        public void GetLastCoordinateBelowThreshold_1([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < count; i++)
            {
                var result = tree.GetLastCoordinateBelowThreshold(i);
                if (i == 0)
                    Assert.IsNull(result, $"Invalid result for {i}.");
                else
                {
                    Assert.IsNotNull(result.TreeNode, "Tree node is null.");
                    Assert.AreEqual(i - 1, result.TreeNode.Key, $"Invalid result for {i}.");
                }
            }
        }

        [Test]
        public void GetLastCoordinateBelowThreshold_2([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).Select(i => (double)i).ToArray();
            var tree = new RedBlackTree<double, double>(data, d => d);

            for (var i = 0; i < count; i++)
            {
                var result = tree.GetLastCoordinateBelowThreshold(i + 0.5);

                Assert.IsNotNull(result, "Result is null.");
                Assert.IsNotNull(result.TreeNode, "Tree node is null.");
                Assert.AreEqual(i, result.TreeNode.Key, $"Invalid result for {i}.");
            }
        }

        [Test]
        public void GetLastCoordinateBelowThreshold_3()
        {
            var tree = new RedBlackTree<double, double>(new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 }, d => d);

            var result = tree.GetLastCoordinateBelowThreshold(1);
            Assert.IsNull(result, "Invalid result for 1.");

            void Check(double threshold, double expectedResult, double[] expectedPreviousValues)
            {
                var node = tree.GetLastCoordinateBelowThreshold(threshold);
                var previousValues = EnumerateViaGetPreviousNode(tree, node).ToArray();
                Assert.AreEqual(expectedResult, node.NodeElement.Value, $"Invalid result for {threshold}.");
                CollectionAssert.AreEqual(
                    expectedPreviousValues,
                    previousValues,
                    $"Invalid previous values list for {threshold}.");
            }

            Check(2, 1, new double[] { 1, 1 });
            Check(1.5, 1, new double[] { 1, 1 });
            Check(3, 2, new double[] { 2, 2, 2, 1, 1 });
            Check(2.5, 2, new double[] { 2, 2, 2, 1, 1 });
            Check(4, 3, new double[] { 3, 2, 2, 2, 1, 1 });
            Check(3.5, 3, new double[] { 3, 2, 2, 2, 1, 1 });
            Check(5, 4, new double[] { 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(4.5, 4, new double[] { 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(6, 5, new double[] { 5, 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(5.5, 5, new double[] { 5, 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(10, 5, new double[] { 5, 4, 4, 4, 3, 2, 2, 2, 1, 1 });
            Check(9.5, 5, new double[] { 5, 4, 4, 4, 3, 2, 2, 2, 1, 1 });
        }

        [Test]
        public void GetLastCoordinateBelowThreshold_4()
        {
            var tree = new RedBlackTree<int, int>(new[] { 0, 5500 }, d => d);

            var result = tree.GetLastCoordinateBelowThreshold(700);
            Assert.IsNotNull(result.TreeNode, "Tree node is null.");
            Assert.AreEqual(0, result.NodeElement.Value, "Invalid result.");
            Assert.AreEqual(1, result.NodeElement.List.Count, "Invalid node's elements count.");
        }

        [Test]
        public void GetFirstCoordinateAboveThreshold_1([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();
            var tree = new RedBlackTree<int, int>(data, d => d);

            for (var i = 0; i < count; i++)
            {
                var result = tree.GetFirstCoordinateAboveThreshold(i);
                if (i == count - 1)
                    Assert.IsNull(result, $"Invalid result for {i}.");
                else
                {
                    Assert.IsNotNull(result.TreeNode, "Tree node is null.");
                    Assert.AreEqual(i + 1, result.TreeNode.Key, $"Invalid result for {i}.");
                }
            }
        }

        [Test]
        public void GetFirstCoordinateAboveThreshold_2([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).Select(i => (double)i).ToArray();
            var tree = new RedBlackTree<double, double>(data, d => d);

            for (var i = 0; i < count; i++)
            {
                var result = tree.GetFirstCoordinateAboveThreshold(i - 0.5);

                Assert.IsNotNull(result, "Result is null.");
                Assert.IsNotNull(result.TreeNode, "Tree node is null.");
                Assert.AreEqual(i, result.TreeNode.Key, $"Invalid result for {i}.");
            }
        }

        [Test]
        public void GetFirstCoordinateAboveThreshold_3()
        {
            var tree = new RedBlackTree<double, double>(new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 }, d => d);

            var result = tree.GetFirstCoordinateAboveThreshold(5);
            Assert.IsNull(result, "Invalid result for 5.");

            void Check(double threshold, double expectedResult, double[] expectedNextValues)
            {
                var node = tree.GetFirstCoordinateAboveThreshold(threshold);
                var nextValues = EnumerateViaGetNextCoordinate(tree, node).ToArray();
                Assert.AreEqual(expectedResult, node.NodeElement.Value, $"Invalid result for {threshold}.");
                CollectionAssert.AreEqual(
                    expectedNextValues,
                    nextValues,
                    $"Invalid next values list for {threshold}.");
            }

            Check(4, 5, new double[] { 5 });
            Check(4.5, 5, new double[] { 5 });
            Check(3, 4, new double[] { 4, 4, 4, 5 });
            Check(3.5, 4, new double[] { 4, 4, 4, 5 });
            Check(2, 3, new double[] { 3, 4, 4, 4, 5 });
            Check(2.5, 3, new double[] { 3, 4, 4, 4, 5 });
            Check(1, 2, new double[] { 2, 2, 2, 3, 4, 4, 4, 5 });
            Check(1.5, 2, new double[] { 2, 2, 2, 3, 4, 4, 4, 5 });
            Check(0, 1, new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 });
            Check(0.5, 1, new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 });
            Check(-5, 1, new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 });
            Check(-4.5, 1, new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 });
        }

        [Test]
        public void GetFirstCoordinateAboveThreshold_4()
        {
            var tree = new RedBlackTree<int, int>(new[] { 0, 5500 }, d => d);

            var result = tree.GetFirstCoordinateAboveThreshold(700);
            Assert.IsNotNull(result.TreeNode, "Tree node is null.");
            Assert.AreEqual(5500, result.NodeElement.Value, "Invalid result.");
            Assert.AreEqual(1, result.NodeElement.List.Count, "Invalid node's elements count.");
        }

        [Test]
        public void Clear_1()
        {
            var tree = new RedBlackTree<int, int>();
            tree.Clear();
            CollectionAssert.IsEmpty(tree, "Tree is not empty.");
        }

        [Test]
        public void Clear_2([Values(1, 2, 3, 4, 10, 100, 1000)] int count)
        {
            var tree = new RedBlackTree<int, int>(Enumerable.Range(0, count), d => d);
            tree.Clear();
            CollectionAssert.IsEmpty(tree, "Tree is not empty.");
        }

        #endregion

        #region Private methods

        private static IEnumerable<TValue> EnumerateViaGetNextCoordinate<TKey, TValue>(RedBlackTree<TKey, TValue> tree, RedBlackTreeCoordinate<TKey, TValue> node)
            where TKey : IComparable<TKey>
        {
            do
            {
                yield return node.NodeElement.Value;
            }
            while ((node = tree.GetNextCoordinate(node)) != null);
        }

        private static IEnumerable<TValue> EnumerateViaGetPreviousNode<TKey, TValue>(RedBlackTree<TKey, TValue> tree, RedBlackTreeCoordinate<TKey, TValue> node)
            where TKey : IComparable<TKey>
        {
            do
            {
                yield return node.NodeElement.Value;
            }
            while ((node = tree.GetPreviousCoordinate(node)) != null);
        }

        private static void CheckAscendingOrder<TValue>(TValue[] values)
            where TValue : IComparable
        {
            for (var i = 0; i < values.Length - 1; i++)
            {
                Assert.GreaterOrEqual(values[i + 1], values[i], $"Ascending order is broken on index {i}.");
            }
        }

        private static void CheckDescendingOrder<TValue>(TValue[] values)
            where TValue : IComparable
        {
            for (var i = 0; i < values.Length - 1; i++)
            {
                Assert.LessOrEqual(values[i + 1], values[i], $"Descending order is broken on index {i}.");
            }
        }

        #endregion
    }
}
