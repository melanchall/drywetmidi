using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class IntervalTreeTests
    {
        #region Nested classes

        private sealed class Interval : IInterval<int>
        {
            public Interval(int start, int length)
            {
                Start = start;
                End = start + length;
            }

            public int Start { get; set; }

            public int End { get; set; }

            public override string ToString() =>
                $"{Start}-{End}";
        }

        #endregion

        #region Test methods

        [Test]
        public void Add_1(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    var interval = new Interval(i, j + 1);
                    tree.Add(interval);
                    CheckTreeNodesMaxValues(tree, $"Invalid tree after addition {i}/{j} of {interval}.");
                }
            }

            ClassicAssert.AreEqual(
                uniqueValuesCount,
                GetNodesCount(tree),
                "Invalid nodes count.");

            ClassicAssert.AreEqual(
                uniqueValuesCount * sameKeyGroupSize,
                tree.GetAllCoordinates().Count(),
                "Invalid coordinates count.");

            CheckAscendingOrder(tree);
        }

        [Test]
        public void Add_2(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 5, 10)] int intervalLength)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    var interval = new Interval(i, intervalLength);
                    tree.Add(interval);
                    CheckTreeNodesMaxValues(tree, $"Invalid tree after addition {i}/{j} of {interval}.");
                }
            }

            ClassicAssert.AreEqual(
                uniqueValuesCount,
                GetNodesCount(tree),
                "Invalid nodes count.");

            ClassicAssert.AreEqual(
                uniqueValuesCount * sameKeyGroupSize,
                tree.GetAllCoordinates().Count(),
                "Invalid coordinates count.");

            CheckAscendingOrder(tree);
        }

        [Test]
        public void Add_3(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    var interval = new Interval(10 * i, j * 5 + 1);
                    tree.Add(interval);
                    CheckTreeNodesMaxValues(tree, $"Invalid tree after addition {i}/{j} of {interval}.");
                }
            }

            ClassicAssert.AreEqual(
                uniqueValuesCount,
                GetNodesCount(tree),
                "Invalid nodes count.");

            ClassicAssert.AreEqual(
                uniqueValuesCount * sameKeyGroupSize,
                tree.GetAllCoordinates().Count(),
                "Invalid coordinates count.");

            CheckAscendingOrder(tree);
        }

        [Test]
        public void Add_4(
            [Values(1, 2, 10, 100, 200)] int treeSize,
            [Values(1, 2, 5, 10)] int initialStep,
            [Values(1, 2, 5, 10)] int step)
        {
            var intervals = Enumerable
                .Range(0, treeSize)
                .Select(i => new Interval(initialStep * i, initialStep))
                .ToArray();
            var tree = new IntervalTree<int, Interval>(intervals);

            var maxEnd = intervals.Max(i => i.End);
            var steps = maxEnd / step;

            for (var i = 0; i < steps; i++)
            {
                var interval = new Interval(step, step);
                tree.Add(interval);
                CheckTreeNodesMaxValues(tree, $"Invalid tree after addition {i} of {interval}.");
            }

            CheckAscendingOrder(tree);
        }

        [Test]
        public void Add_FromOverlappingAll(
            [Values(1, 2, 10, 100, 200)] int treeSize,
            [Values(1, 2, 5, 10)] int length,
            [Values(1, 2, 5, 10)] int step)
        {
            var intervals = Enumerable
                .Range(0, treeSize)
                .Select(i => new Interval(length * i, length))
                .ToArray();
            var tree = new IntervalTree<int, Interval>(intervals);

            var start = intervals.Min(i => i.Start) - 1;
            var end = intervals.Max(i => i.End) + 1;
            
            var j = 0;

            while (start < end)
            {
                var interval = new Interval(start, end - start);
                tree.Add(interval);
                CheckTreeNodesMaxValues(tree, $"Invalid tree after addition {j} of {interval}.");
                CheckAscendingOrder(tree);

                start += step;
                end -= step;
                j++;
            }
        }

        [Test]
        public void AddWithoutMaxUpdating_1(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    var interval = new Interval(i, j + 1);
                    tree.AddWithoutMaxUpdating(interval);
                }
            }

            tree.InitializeMax();
            CheckTreeNodesMaxValues(tree, $"Invalid tree.");

            ClassicAssert.AreEqual(
                uniqueValuesCount,
                GetNodesCount(tree),
                "Invalid nodes count.");

            ClassicAssert.AreEqual(
                uniqueValuesCount * sameKeyGroupSize,
                tree.GetAllCoordinates().Count(),
                "Invalid coordinates count.");

            CheckAscendingOrder(tree);
        }

        [Test]
        public void AddWithoutMaxUpdating_2(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 5, 10)] int intervalLength)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    var interval = new Interval(i, intervalLength);
                    tree.AddWithoutMaxUpdating(interval);
                }
            }

            tree.InitializeMax();
            CheckTreeNodesMaxValues(tree, $"Invalid tree.");

            ClassicAssert.AreEqual(
                uniqueValuesCount,
                GetNodesCount(tree),
                "Invalid nodes count.");

            ClassicAssert.AreEqual(
                uniqueValuesCount * sameKeyGroupSize,
                tree.GetAllCoordinates().Count(),
                "Invalid coordinates count.");

            CheckAscendingOrder(tree);
        }

        [Test]
        public void AddWithoutMaxUpdating_3(
            [Values(1, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    var interval = new Interval(10 * i, j * 5 + 1);
                    tree.AddWithoutMaxUpdating(interval);
                }
            }

            tree.InitializeMax();
            CheckTreeNodesMaxValues(tree, $"Invalid tree.");

            ClassicAssert.AreEqual(
                uniqueValuesCount,
                GetNodesCount(tree),
                "Invalid nodes count.");

            ClassicAssert.AreEqual(
                uniqueValuesCount * sameKeyGroupSize,
                tree.GetAllCoordinates().Count(),
                "Invalid coordinates count.");

            CheckAscendingOrder(tree);
        }

        [Test]
        public void AddWithoutMaxUpdating_4(
            [Values(1, 2, 10, 100, 200)] int treeSize,
            [Values(1, 2, 5, 10)] int initialStep,
            [Values(1, 2, 5, 10)] int step)
        {
            var tree = new IntervalTree<int, Interval>();

            var intervals = Enumerable
                .Range(0, treeSize)
                .Select(i => new Interval(initialStep * i, initialStep))
                .ToArray();

            foreach (var interval in intervals)
            {
                tree.AddWithoutMaxUpdating(interval);
            }

            var maxEnd = intervals.Max(i => i.End);
            var steps = maxEnd / step;

            for (var i = 0; i < steps; i++)
            {
                var interval = new Interval(step, step);
                tree.AddWithoutMaxUpdating(interval);
            }

            tree.InitializeMax();
            CheckTreeNodesMaxValues(tree, $"Invalid tree.");
            CheckAscendingOrder(tree);
        }

        [Test]
        public void AddWithoutMaxUpdating_FromOverlappingAll(
            [Values(1, 2, 10, 100, 200)] int treeSize,
            [Values(1, 2, 5, 10)] int length,
            [Values(1, 2, 5, 10)] int step)
        {
            var tree = new IntervalTree<int, Interval>();

            var intervals = Enumerable
                .Range(0, treeSize)
                .Select(i => new Interval(length * i, length))
                .ToArray();

            foreach (var interval in intervals)
            {
                tree.AddWithoutMaxUpdating(interval);
            }

            var start = intervals.Min(i => i.Start) - 1;
            var end = intervals.Max(i => i.End) + 1;

            var j = 0;

            while (start < end)
            {
                var interval = new Interval(start, end - start);
                tree.AddWithoutMaxUpdating(interval);

                start += step;
                end -= step;
                j++;
            }

            tree.InitializeMax();
            CheckTreeNodesMaxValues(tree, $"Invalid tree.");
            CheckAscendingOrder(tree);
        }

        [Test]
        public void Remove_1(
            [Values(1, 2, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 3, 5, 10)] int step)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    tree.Add(new Interval(i, j + 1));
                }
            }

            var coordinates = tree.GetAllCoordinates().ToArray();

            for (int i = 0, j = 0; i < coordinates.Length; i += step, j++)
            {
                var coordinate = coordinates[i];
                tree.Remove(coordinate);
                CheckTreeNodesMaxValues(tree, $"Invalid tree after deletion {j} of {coordinate} ({i}th).");
            }
        }

        [Test]
        public void Remove_2(
            [Values(1, 2, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 5, 10)] int intervalLength)
        {
            var tree = new IntervalTree<int, Interval>();

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    tree.Add(new Interval(i, intervalLength));
                }
            }

            while (true)
            {
                var root = tree.GetRoot();
                if (root == null)
                    break;

                var coordinate = new RedBlackTreeCoordinate<int, Interval>(root, root.Values.First);
                tree.Remove(coordinate);
                CheckTreeNodesMaxValues(tree, $"Invalid tree after deletion of {coordinate}.");
            }
        }

        [Test]
        public void Remove_3(
            [Values(1, 2, 10, 100)] int uniqueValuesCount,
            [Values(1, 2, 3, 7, 10)] int sameKeyGroupSize,
            [Values(1, 2, 3, 5, 10)] int step)
        {
            var tree = new IntervalTree<int, Interval>();
            var maxEnd = uniqueValuesCount * sameKeyGroupSize + 1;

            for (var i = 0; i < uniqueValuesCount; i++)
            {
                for (var j = 0; j < sameKeyGroupSize; j++)
                {
                    tree.Add(new Interval(i, maxEnd--));
                }
            }

            var coordinates = tree.GetAllCoordinates().ToArray();

            for (var i = 0; i < coordinates.Length; i += step)
            {
                var coordinate = coordinates[i];
                tree.Remove(coordinate);
                CheckTreeNodesMaxValues(tree, $"Invalid tree after deletion {i} of {coordinate}.");
            }
        }

        [Test]
        public void Search_SameIntervals_PointBefore(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(0, 3, 5, 10, 100)] int length,
            [Values] bool postponeMaxUpdating)
        {
            var tree = CreateIntervalTree<int, Interval>(
                Enumerable.Range(0, intervalsCount).Select(i => new Interval(start, length)),
                postponeMaxUpdating);

            var foundIntervals = tree.Search(start - 1).Select(c => c.Value).ToArray();
            CollectionAssert.IsEmpty(foundIntervals, "There are intervals.");
        }

        [Test]
        public void Search_SameIntervals_PointAbove(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(0, 3, 5, 10, 100)] int length,
            [Values] bool postponeMaxUpdating)
        {
            var tree = CreateIntervalTree<int, Interval>(
                Enumerable.Range(0, intervalsCount).Select(i => new Interval(start, length)),
                postponeMaxUpdating);

            var foundIntervals = tree.Search(start + length + 1).Select(c => c.Value).ToArray();
            CollectionAssert.IsEmpty(foundIntervals, "There are intervals.");
        }

        [Test]
        public void Search_SameIntervals_PointOnStart(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(0, 3, 5, 10, 100)] int length,
            [Values] bool postponeMaxUpdating)
        {
            var intervals = Enumerable.Range(0, intervalsCount).Select(i => new Interval(start, length)).ToArray();
            var tree = CreateIntervalTree<int, Interval>(intervals, postponeMaxUpdating);

            var foundIntervals = tree.Search(start).Select(c => c.Value).ToArray();
            CollectionAssert.IsEmpty(foundIntervals, "There are intervals.");
        }

        [Test]
        public void Search_SameIntervals_PointOnEnd(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(0, 3, 5, 10, 100)] int length,
            [Values] bool postponeMaxUpdating)
        {
            var intervals = Enumerable.Range(0, intervalsCount).Select(i => new Interval(start, length)).ToArray();
            var tree = CreateIntervalTree<int, Interval>(intervals, postponeMaxUpdating);

            var foundIntervals = tree.Search(start + length).Select(c => c.Value).ToArray();
            CollectionAssert.IsEmpty(foundIntervals, "There are intervals.");
        }

        [Test]
        public void Search_SameIntervals_PointInMiddle(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(3, 5, 10, 100)] int length,
            [Values] bool postponeMaxUpdating)
        {
            var intervals = Enumerable.Range(0, intervalsCount).Select(i => new Interval(start, length)).ToArray();
            var tree = CreateIntervalTree<int, Interval>(intervals, postponeMaxUpdating);

            var foundIntervals = tree.Search(start + length / 2).Select(c => c.Value).ToArray();
            CollectionAssert.AreEquivalent(intervals, foundIntervals, "Invalid intervals.");
        }

        [Test]
        public void Search_IntervalsWithGaps(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(3, 5, 10, 100)] int length,
            [Values(1, 3, 5, 10, 100)] int gap,
            [Values] bool postponeMaxUpdating)
        {
            var intervals = Enumerable
                .Range(0, intervalsCount)
                .Select(i => new Interval(start + (length + gap) * i, length))
                .ToArray();
            var tree = CreateIntervalTree<int, Interval>(intervals, postponeMaxUpdating);

            for (var i = 0; i < intervalsCount; i++)
            {
                var foundIntervals = tree.Search(start + (length + gap) * i + 1).Select(c => c.Value).ToArray();
                CollectionAssert.AreEqual(
                    new[] { intervals[i] },
                    foundIntervals,
                    "Invalid intervals.");
            }
        }

        [Test]
        public void Search_IntervalsWithGaps_WithOverlappingAll(
            [Values(1, 10, 100, 1000)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(3, 5, 10, 100)] int length,
            [Values(1, 3, 5, 10, 100)] int gap,
            [Values] bool postponeMaxUpdating)
        {
            var intervals = Enumerable
                .Range(0, intervalsCount)
                .Select(i => new Interval(start + (length + gap) * i, length))
                .ToArray();
            var tree = CreateIntervalTree<int, Interval>(intervals, postponeMaxUpdating);

            var minStart = intervals.Min(i => i.Start);
            var maxEnd = intervals.Max(i => i.End);

            var overlappingInterval = new Interval(
                minStart - 1,
                maxEnd - minStart + 2);
            tree.Add(overlappingInterval);

            for (var i = 0; i < intervalsCount; i++)
            {
                var foundIntervals = tree.Search(start + (length + gap) * i + 1).Select(c => c.Value).ToArray();
                CollectionAssert.AreEquivalent(
                    new[] { intervals[i], overlappingInterval },
                    foundIntervals,
                    "Invalid intervals.");
            }
        }

        [Test]
        public void ScaleIntervals(
            [Values(1, 10, 100)] int intervalsCount,
            [Values(-10, 0, 10)] int start,
            [Values(3, 5, 10, 100)] int length,
            [Values(3, 5, 10, 100)] int gap)
        {
            for (var i = 0; i < intervalsCount; i++)
            {
                var intervals = Enumerable
                    .Range(0, intervalsCount)
                    .Select(j => new Interval(start + (length + gap) * j, length))
                    .ToArray();

                var tree = new IntervalTree<int, Interval>(intervals);

                //

                var scalePoint = start + (length + gap) * i - 1;

                int Scale(int point) =>
                    point + (int)Math.Round((point - scalePoint) * 0.5);

                //

                var coordinate = tree.GetFirstCoordinateAboveThreshold(scalePoint);

                do
                {
                    coordinate.Key = Scale(coordinate.Key);
                    coordinate.Value.Start = coordinate.Key;
                    coordinate.Value.End = Scale(coordinate.Value.End);

                    if (tree.UpdateMax(coordinate.TreeNode))
                        tree.UpdateMaxUp(coordinate.TreeNode);
                }
                while ((coordinate = tree.GetNextCoordinate(coordinate)) != null);

                CheckTreeNodesMaxValues(tree, $"Invalid tree after scaling {i}.");
                CheckAscendingOrder(tree);

                //

                for (var j = 0; j < intervalsCount; j++)
                {
                    var searchPoint = start + (length + gap) * i + 1;
                    if (searchPoint > scalePoint)
                        searchPoint = Scale(searchPoint);

                    var foundIntervals = tree.Search(searchPoint).Select(c => c.Value).ToArray();
                    CollectionAssert.AreEquivalent(
                        new[] { intervals[i] },
                        foundIntervals,
                        $"Invalid intervals on search {j} / {i} in middle.");

                    //

                    searchPoint = intervals[i].End + 1;

                    foundIntervals = tree.Search(searchPoint).Select(c => c.Value).ToArray();
                    CollectionAssert.IsEmpty(
                        foundIntervals,
                        $"Invalid intervals on search {j} / {i} in gap.");
                }
            }
        }

        #endregion

        #region Private methods

        private static void CheckAscendingOrder<TKey, TValue>(
            IntervalTree<TKey, TValue> tree)
            where TKey : IComparable<TKey>, IComparable
            where TValue : IInterval<TKey>
        {
            var values = tree.ToArray();

            for (var i = 0; i < values.Length - 1; i++)
            {
                ClassicAssert.GreaterOrEqual(values[i + 1].Start, values[i].Start, $"Ascending order is broken on index {i}.");
            }
        }

        private static void CheckTreeNodesMaxValues<TKey, TValue>(
            IntervalTree<TKey, TValue> tree,
            string message)
            where TKey : IComparable<TKey>
            where TValue : IInterval<TKey>
        {
            var coordinates = tree.GetAllCoordinates();

            foreach (var coordinate in coordinates)
            {
                var node = coordinate.TreeNode;
                var max = (TKey)node.Data;
                var calculatedMax = GetSubTreeMax(node);
                ClassicAssert.AreEqual(
                    calculatedMax,
                    max,
                    $"{message} Invalid max value for node {node.Key} (value = {coordinate.Value})");
            }
        }

        private static TKey GetSubTreeMax<TKey, TValue>(RedBlackTreeNode<TKey, TValue> subTreeRoot)
            where TKey : IComparable<TKey>
            where TValue : IInterval<TKey>
        {
            var result = subTreeRoot.Values.Select(v => v.End).Max();

            var left = subTreeRoot.Left;
            if (left != null && left != RedBlackTreeNode<TKey, TValue>.Void)
            {
                var leftMax = GetSubTreeMax(left);
                if (leftMax.CompareTo(result) > 0)
                    result = leftMax;
            }

            var right = subTreeRoot.Right;
            if (right != null && right != RedBlackTreeNode<TKey, TValue>.Void)
            {
                var rightMax = GetSubTreeMax(right);
                if (rightMax.CompareTo(result) > 0)
                    result = rightMax;
            }

            return result;
        }

        private static int GetNodesCount<TKey, TValue>(RedBlackTree<TKey, TValue> tree)
            where TKey : IComparable<TKey>
            where TValue : IInterval<TKey> =>
            tree
                .GetAllCoordinates()
                .Select(n => n.TreeNode)
                .Distinct()
                .Count();

        private static IntervalTree<TKey, TValue> CreateIntervalTree<TKey, TValue>(
            IEnumerable<TValue> values,
            bool postponeMaxUpdating)
            where TKey : IComparable<TKey>
            where TValue : IInterval<TKey>
        {
            if (!postponeMaxUpdating)
                return new IntervalTree<TKey, TValue>(values);

            var tree = new IntervalTree<TKey, TValue>();

            foreach (var value in values)
            {
                tree.AddWithoutMaxUpdating(value);
            }

            tree.InitializeMax();
            return tree;
        }

        #endregion
    }
}
