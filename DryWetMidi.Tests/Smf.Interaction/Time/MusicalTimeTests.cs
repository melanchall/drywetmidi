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
        [Description("Parse musical time in form of 'Bars.Fraction'.")]
        public void TryParse_BarsDotFraction()
        {
            TestTryParse("3.34/65", new MusicalTime(3, 0, new Fraction(34, 65)));
        }

        [TestMethod]
        [Description("Parse musical time in form of 'Bars.Beats.Fraction'.")]
        public void TryParse_BarsDotBeatsDotFraction()
        {
            TestTryParse("3.6.3/8", new MusicalTime(3, 6, new Fraction(3, 8)));
        }

        [TestMethod]
        [Description("Parse musical time in form of '..Fraction'.")]
        public void TryParse_DotDotFraction()
        {
            TestTryParse("..3/4", new MusicalTime(new Fraction(3, 4)));
        }

        [TestMethod]
        [Description("Parse musical time in form of '.Beats.'.")]
        public void TryParse_DotBeatsDot()
        {
            TestTryParse(".4.", new MusicalTime(0, 4));
        }

        [TestMethod]
        [Description("Parse musical time in form of '..'.")]
        public void TryParse_DotDot()
        {
            TestTryParse("..", new MusicalTime(0, 0));
        }

        [TestMethod]
        [Description("Parse musical time in form of '.Beats.Fraction'.")]
        public void TryParse_DotBeatsDotFraction()
        {
            TestTryParse(".5.8/9", new MusicalTime(0, 5, new Fraction(8, 9)));
        }

        [TestMethod]
        [Description("Parse musical time in form of 'Bars..Fraction'.")]
        public void TryParse_BarsDotDotFraction()
        {
            TestTryParse("5..3/4", new MusicalTime(5, 0, new Fraction(3, 4)));
        }

        [TestMethod]
        [Description("Parse musical time in form of 'Fraction'.")]
        public void TryParse_Fraction()
        {
            TestTryParse("3/7", new MusicalTime(new Fraction(3, 7)));
        }

        #endregion

        #region Private methods

        private static void TestTryParse(string input, MusicalTime expectedTime)
        {
            MusicalTime.TryParse(input, out var actualTime);
            Assert.AreEqual(expectedTime, actualTime);
        }

        #endregion
    }
}
