using Melanchall.DryWetMidi.Common;
using NUnit.Framework;

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

        #endregion
    }
}
