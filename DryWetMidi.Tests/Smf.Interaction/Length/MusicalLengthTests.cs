using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
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

        [TestMethod]
        [Description("Add musical length.")]
        public void Add_Musical()
        {
            var actual = ((MusicalLength)MusicalFraction.Half).Add((MusicalLength)MusicalFraction.Quarter);
            var expected = (MusicalLength)MusicalFraction.HalfDotted;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Add metric length.")]
        public void Add_Metric()
        {
            var actual = ((MusicalLength)MusicalFraction.Half).Add(new MetricLength(0, 1, 30));
            var expected = new MathLength((MusicalLength)MusicalFraction.Half,
                                          new MetricLength(0, 1, 30),
                                          MathOperation.Add);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract musical length.")]
        public void Subtract_Musical()
        {
            var actual = ((MusicalLength)MusicalFraction.Half).Subtract((MusicalLength)MusicalFraction.Quarter);
            var expected = (MusicalLength)MusicalFraction.Quarter;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Subtract MIDI length.")]
        public void Subtract_Midi()
        {
            var actual = ((MusicalLength)MusicalFraction.Half).Subtract(new MidiLength(300));
            var expected = new MathLength((MusicalLength)MusicalFraction.Half,
                                          new MidiLength(300),
                                          MathOperation.Subtract);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Multiply musical length.")]
        public void Multiply()
        {
            var actual = ((MusicalLength)MusicalFraction.Half).Multiply(3);
            var expected = (MusicalLength)MusicalFraction.WholeDotted;

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Description("Divide musical length.")]
        public void Divide()
        {
            var actual = new MusicalLength(9 * MusicalFraction.EighthTriplet).Divide(3);
            var expected = (MusicalLength)MusicalFraction.Quarter;

            Assert.AreEqual(expected, actual);
        }

        #endregion
    }
}
