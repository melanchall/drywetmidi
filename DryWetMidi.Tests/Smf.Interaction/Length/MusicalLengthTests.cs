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
        [Description("Try parse musical length in form of '/Denominator'.")]
        public void TryParse_Denominator()
        {
            LengthParsingTester.TestTryParse("/4", (MusicalLength)new Fraction(1, 4));
        }

        [TestMethod]
        [Description("Try parse musical length in form of 'FractionMnemonic'.")]
        public void TryParse_FractionMnemonic()
        {
            LengthParsingTester.TestTryParse("w", (MusicalLength)MusicalFraction.Whole);
        }

        [TestMethod]
        [Description("Try parse musical length in form of 'FractionMnemonic TupletMnemonic Dots'.")]
        public void TryParse_FractionMnemonic_TupletMnemonic_Dots()
        {
            LengthParsingTester.TestTryParse("e t .", (MusicalLength)MusicalFraction.CreateDottedTriplet(8, 1));
        }

        [TestMethod]
        [Description("Try parse musical length in form of 'FractionMnemonic Tuplet'.")]
        public void TryParse_FractionMnemonic_Tuplet()
        {
            LengthParsingTester.TestTryParse("h [ 3 : 2 ]", (MusicalLength)MusicalFraction.HalfTriplet);
        }

        [TestMethod]
        [Description("Try parse musical length in form of '/Denominator Tuplet Dots'.")]
        public void TryParse_Fraction_Tuplet_Dots()
        {
            LengthParsingTester.TestTryParse("/3[4:7]..", (MusicalLength)MusicalFraction.CreateDottedTuplet(3, 2, 4, 7));
        }

        [TestMethod]
        [Description("Try parse musical length in form of 'Numerator/Denominator Dots'.")]
        public void TryParse_Fraction_Dots()
        {
            LengthParsingTester.TestTryParse("2/3..", (MusicalLength)MusicalFraction.CreateDotted(2, 3, 2));
        }

        [TestMethod]
        [Description("Parse string representation of a length.")]
        public void Parse_ToString()
        {
            LengthParsingTester.TestToString((MusicalLength)new Fraction(5, 17));
        }

        #endregion
    }
}
