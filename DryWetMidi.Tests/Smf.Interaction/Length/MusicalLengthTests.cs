using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction.Length
{
    [TestClass]
    public sealed class MusicalLengthTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse musical length in form of 'Numerator/Denominator'.")]
        public void TryParse_NumeratorDenominator()
        {
            LengthParsingTester.TestTryParse("3/4", (MusicalLength)new Fraction(3, 4));
        }

        [TestMethod]
        [Description("Try parse fraction in form of '/Denominator'.")]
        public void TryParse_Denominator()
        {
            LengthParsingTester.TestTryParse("/4", (MusicalLength)new Fraction(1, 4));
        }

        [TestMethod]
        [Description("Parse string representation of a fraction.")]
        public void Parse_ToString()
        {
            LengthParsingTester.TestToString((MusicalLength)new Fraction(5, 17));
        }

        #endregion
    }
}
