using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class MathLengthTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse musical length plus musical length.")]
        public void TryParse_MusicalPlusMusical()
        {
            LengthParsingTester.TestTryParse("4/5 + 6/8",
                                             new MathLength(new MusicalLength(new Fraction(4, 5)),
                                                            new MusicalLength(new Fraction(6, 8)),
                                                            MathOperation.Add));
        }

        [TestMethod]
        [Description("Try parse musical length plus metric length.")]
        public void TryParse_MusicalPlusMetric()
        {
            LengthParsingTester.TestTryParse("4/5 + 7:56",
                                             new MathLength(new MusicalLength(new Fraction(4, 5)),
                                                            new MetricLength(0, 7, 56),
                                                            MathOperation.Add));
        }

        [TestMethod]
        [Description("Try parse metric length minus MIDI length.")]
        public void TryParse_MetricMinusMidi()
        {
            LengthParsingTester.TestTryParse("0:0:0:123 - 756",
                                             new MathLength(new MetricLength(0, 0, 0, 123),
                                                            new MidiLength(756),
                                                            MathOperation.Subtract));
        }

        [TestMethod]
        [Description("Try parse metric length minus math length.")]
        public void TryParse_MetricMinusMath()
        {
            LengthParsingTester.TestTryParse("0:0:0:123 - (5/6 + 56)",
                                             new MathLength(new MetricLength(0, 0, 0, 123),
                                                            new MathLength(new MusicalLength(new Fraction(5, 6)),
                                                                           new MidiLength(56),
                                                                           MathOperation.Add),
                                                            MathOperation.Subtract));
        }

        [TestMethod]
        [Description("Parse string representation of a length.")]
        public void TryParse_ToString()
        {
            LengthParsingTester.TestToString(new MathLength(new MetricLength(0, 0, 0, 123),
                                                            new MidiLength(756),
                                                            MathOperation.Subtract));
        }

        #endregion
    }
}
