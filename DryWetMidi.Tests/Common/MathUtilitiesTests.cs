using Melanchall.DryWetMidi.Common;
using NUnit.Framework;
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
            Assert.AreEqual(expectedLabel, actualLabel, "Invalid found element.");
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

        #endregion

        #region Private methods

        private void GetLastElementBelowThresholdWithIndex(
            (string, int)[] elements,
            long threshold,
            string expectedLabel,
            int expectedIndex)
        {
            var actualLabel = MathUtilities.GetLastElementBelowThreshold(elements, threshold, e => e.Item2, out var index).Item1;
            Assert.AreEqual(expectedLabel, actualLabel, "Invalid found element.");
            Assert.AreEqual(expectedIndex, index, "Invalid found index.");
        }

        #endregion
    }
}
