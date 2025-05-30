using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestFixture]
    public sealed class MathUtilitiesTests
    {
        #region Test methods

        [TestCase(0, null)]
        [TestCase(5, null)]
        [TestCase(10, null)]
        [TestCase(15, "a")]
        [TestCase(50, "a")]
        [TestCase(70, "b")]
        [TestCase(100, "b")]
        [TestCase(300, "c")]
        [TestCase(500, "c")]
        [TestCase(700, "d")]
        [TestCase(800, "d")]
        [TestCase(900, "e")]
        [TestCase(1000, "e")]
        [TestCase(1500, "f")]
        [TestCase(2000, "f")]
        [TestCase(2500, "g")]
        public void GetLastElementBelowThreshold(long threshold, string expectedLabel)
        {
            var elements = new[]
            {
                ("a", 10),
                ("b", 50),
                ("c", 100),
                ("d", 500),
                ("e", 800),
                ("f", 1000),
                ("g", 2000),
            };

            var actualLabel = MathUtilities.GetLastElementBelowThreshold(elements, threshold, e => e.Item2).Item1;
            ClassicAssert.AreEqual(expectedLabel, actualLabel, "Invalid found element.");
        }

        [TestCase(0, null, -1)]
        [TestCase(5, "zero", 0)]
        [TestCase(10, "zero", 0)]
        [TestCase(15, "a", 1)]
        [TestCase(50, "a", 1)]
        [TestCase(70, "b", 2)]
        [TestCase(100, "b", 2)]
        [TestCase(300, "c", 3)]
        [TestCase(500, "c", 3)]
        [TestCase(700, "d", 4)]
        [TestCase(800, "d", 4)]
        [TestCase(900, "e", 5)]
        [TestCase(1000, "e", 5)]
        [TestCase(1500, "f", 6)]
        [TestCase(2000, "f", 6)]
        [TestCase(2500, "g", 7)]
        public void GetLastElementBelowThresholdWithIndex_1(long threshold, string expectedLabel, int expectedIndex) => GetLastElementBelowThresholdWithIndex(
            new[]
            {
                ("zero", 0),
                ("a", 10),
                ("b", 50),
                ("c", 100),
                ("d", 500),
                ("e", 800),
                ("f", 1000),
                ("g", 2000),
            },
            threshold,
            expectedLabel,
            expectedIndex);

        [TestCase(0, null, -1)]
        [TestCase(5, "b", 1)]
        [TestCase(10, "b", 1)]
        [TestCase(15, "c", 2)]
        public void GetLastElementBelowThresholdWithIndex_2(long threshold, string expectedLabel, int expectedIndex) => GetLastElementBelowThresholdWithIndex(
            new[]
            {
                ("a", 0),
                ("b", 0),
                ("c", 10),
            },
            threshold,
            expectedLabel,
            expectedIndex);

        [Test]
        public void GetLastElementBelowThresholdWithIndex_SameKey(
            [Values(0, 1, 2, 3, 10, 11)] int elementsCount,
            [Values(0, 10)] int elementsKey) =>
            GetLastElementBelowThresholdWithIndex(
                Enumerable.Range(0, elementsCount).Select(_ => ("a", elementsKey)).ToArray(),
                0,
                null,
                -1);

        [Test]
        public void GetLastElementBelowThreshold_1([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();

            for (var i = 0; i < count; i++)
            {
                int index;
                var element = MathUtilities.GetLastElementBelowThreshold(
                    data,
                    i,
                    d => d,
                    out index);

                if (i == 0)
                {
                    ClassicAssert.AreEqual(-1, index, $"Invalid index for {i}.");
                    ClassicAssert.AreEqual(0, element, $"Invalid element for {i}.");
                }
                else
                {
                    ClassicAssert.AreEqual(i - 1, index, $"Invalid index for {i}.");
                    ClassicAssert.AreEqual(data[i - 1], element, $"Invalid element for {i}.");
                }
            }
        }

        [Test]
        public void GetLastElementBelowThreshold_2([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).Select(i => (double)i).ToArray();

            for (var i = 0; i < count; i++)
            {
                int index;
                var element = MathUtilities.GetLastElementBelowThreshold(
                    data,
                    i + 0.5,
                    d => d,
                    out index);

                ClassicAssert.AreEqual(i, index, $"Invalid index for {i}.");
                ClassicAssert.AreEqual(data[i], element, $"Invalid element for {i}.");
            }
        }

        [Test]
        public void GetLastElementBelowThreshold_3()
        {
            var data = new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 };

            int index;
            var element = MathUtilities.GetLastElementBelowThreshold(
                data,
                1,
                d => d,
                out index);

            ClassicAssert.AreEqual(-1, index, "Invalid index for 1.");
            ClassicAssert.AreEqual(0, element, "Invalid element for 1.");

            void Check(double threshold, double expectedResult, double[] expectedPreviousValues)
            {
                element = MathUtilities.GetLastElementBelowThreshold(
                    data,
                    threshold,
                    d => d,
                    out index);

                var previousValues = Enumerable.Range(0, index + 1).Select(i => data[i]).Reverse().ToArray();

                ClassicAssert.AreEqual(expectedResult, element, $"Invalid element for {threshold}.");
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
        public void GetLastElementBelowThreshold_4()
        {
            var data = new[] { 2, 5500 };

            int index;
            var element = MathUtilities.GetLastElementBelowThreshold(
                data,
                700,
                d => d,
                out index);

            ClassicAssert.AreEqual(2, element, "Invalid element.");
            ClassicAssert.AreEqual(0, index, "Invalid index.");
        }

        [Test]
        public void GetFirstElementAboveThreshold_1([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).ToArray();

            for (var i = 0; i < count; i++)
            {
                int index;
                var element = MathUtilities.GetFirstElementAboveThreshold(
                    data,
                    i,
                    d => d,
                    out index);

                if (i == count - 1)
                {
                    ClassicAssert.AreEqual(-1, index, $"Invalid index for {i}.");
                    ClassicAssert.AreEqual(0, element, $"Invalid element for {i}.");
                }
                else
                {
                    ClassicAssert.AreEqual(i + 1, index, $"Invalid index for {i}.");
                    ClassicAssert.AreEqual(data[i + 1], element, $"Invalid element for {i}.");
                }
            }
        }

        [Test]
        public void GetFirstElementAboveThreshold_2([Values(1, 2, 4, 8, 16, 32, 64, 128)] int count)
        {
            var data = Enumerable.Range(0, count).Select(i => (double)i).ToArray();

            for (var i = 0; i < count; i++)
            {
                int index;
                var element = MathUtilities.GetFirstElementAboveThreshold(
                    data,
                    i - 0.5,
                    d => d,
                    out index);

                ClassicAssert.AreEqual(i, index, $"Invalid index for {i}.");
                ClassicAssert.AreEqual(data[i], element, $"Invalid element for {i}.");
            }
        }

        [Test]
        public void GetFirstElementAboveThreshold_3()
        {
            var data = new double[] { 1, 1, 2, 2, 2, 3, 4, 4, 4, 5 };

            int index;
            var element = MathUtilities.GetFirstElementAboveThreshold(
                data,
                5,
                d => d,
                out index);

            ClassicAssert.AreEqual(-1, index, "Invalid index for 5.");
            ClassicAssert.AreEqual(0, element, "Invalid element for 5.");

            void Check(double threshold, double expectedResult, double[] expectedNextValues)
            {
                element = MathUtilities.GetFirstElementAboveThreshold(
                    data,
                    threshold,
                    d => d,
                    out index);

                var nextValues = Enumerable.Range(index, data.Length - index).Select(i => data[i]).ToArray();
                ClassicAssert.AreEqual(expectedResult, element, $"Invalid element for {threshold}.");
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
        public void GetFirstElementAboveThreshold_4()
        {
            var data = new[] { 0, 5500 };

            int index;
            var element = MathUtilities.GetFirstElementAboveThreshold(
                data,
                700,
                d => d,
                out index);

            ClassicAssert.AreEqual(5500, element, "Invalid element.");
            ClassicAssert.AreEqual(1, index, "Invalid index.");
        }

        #endregion

        #region Private methods

        private void GetLastElementBelowThresholdWithIndex(
            (string, int)[] elements,
            long threshold,
            string expectedLabel,
            int expectedIndex)
        {
            var actualLabel = MathUtilities.GetLastElementBelowThreshold(elements, threshold, e => e.Item2, out var index).Item1;
            ClassicAssert.AreEqual(expectedLabel, actualLabel, "Invalid found element.");
            ClassicAssert.AreEqual(expectedIndex, index, "Invalid found index.");
        }

        #endregion
    }
}
