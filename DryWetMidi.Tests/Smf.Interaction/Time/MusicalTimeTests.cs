using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MusicalTimeTests
    {
        #region Test methods

        [TestMethod]
        [Description("Try parse musical time in form of 'Bars.Beats.Fraction'.")]
        public void TryParse_BarsBeatsFraction()
        {
            ParsingTester.TestTryParse("3.6.3/8", new MusicalTime(3, 6, new Fraction(3, 8)));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'Bars.Beats'.")]
        public void TryParse_BarsBeats()
        {
            ParsingTester.TestTryParse("3.6", new MusicalTime(3, 6));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'Fraction'.")]
        public void TryParse_Fraction()
        {
            ParsingTester.TestTryParse("3/7", new MusicalTime(new Fraction(3, 7)));
        }

        [TestMethod]
        [Description("Parse string representation of a time.")]
        public void Parse_ToString()
        {
            ParsingTester.TestToString(new MusicalTime(3, 6, new Fraction(3, 8)));
        }

        #endregion
    }
}
