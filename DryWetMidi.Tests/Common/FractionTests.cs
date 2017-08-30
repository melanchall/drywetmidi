using Melanchall.DryWetMidi.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Common
{
    [TestClass]
    public class FractionTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse fraction in form of 'Numerator/Denominator'.")]
        public void TryParse_NumeratorDenominator()
        {
            TestTryParse("3/4", new Fraction(3, 4));
        }

        [TestMethod]
        [Description("Try parse fraction in form of 'Numerator'.")]
        public void TryParse_Numerator()
        {
            TestTryParse("3", new Fraction(3, 1));
        }

        [TestMethod]
        [Description("Try parse fraction in form of '/Denominator'.")]
        public void TryParse_Denominator()
        {
            TestTryParse("/4", new Fraction(1, 4));
        }

        [TestMethod]
        [Description("Parse string representation of a fraction.")]
        public void Parse_ToString()
        {
            var expectedFraction = new Fraction(5, 17);
            Assert.AreEqual(expectedFraction, Fraction.Parse(expectedFraction.ToString()));
        }

        #endregion

        #region Private methods

        private static void TestTryParse(string input, Fraction expectedFraction)
        {
            Fraction.TryParse(input, out var actualFraction);
            Assert.AreEqual(expectedFraction, actualFraction);
        }

        #endregion
    }
}
