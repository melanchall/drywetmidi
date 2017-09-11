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
            TimeParsingTester.TestTryParse("3.6.3/8", new MusicalTime(3, 6, new Fraction(3, 8)));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'Bars.Beats'.")]
        public void TryParse_BarsBeats()
        {
            TimeParsingTester.TestTryParse("3.6", new MusicalTime(3, 6));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'Fraction'.")]
        public void TryParse_Fraction()
        {
            TimeParsingTester.TestTryParse("3/7", new MusicalTime(new Fraction(3, 7)));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'FractionMnemonic'.")]
        public void TryParse_FractionMnemonic()
        {
            TimeParsingTester.TestTryParse("w", (MusicalTime)MusicalFraction.Whole);
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'Bars.Beats.FractionMnemonic'.")]
        public void TryParse_BarsBeatsFractionMnemonicDots()
        {
            TimeParsingTester.TestTryParse("1.2.w.", new MusicalTime(1, 2, MusicalFraction.WholeDotted));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'FractionMnemonic TupletMnemonic Dots'.")]
        public void TryParse_FractionMnemonic_TupletMnemonic_Dots()
        {
            TimeParsingTester.TestTryParse("e t .", (MusicalTime)MusicalFraction.CreateDottedTriplet(8, 1));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'FractionMnemonic Tuplet'.")]
        public void TryParse_FractionMnemonic_Tuplet()
        {
            TimeParsingTester.TestTryParse("h [ 3 : 2 ]", (MusicalTime)MusicalFraction.HalfTriplet);
        }

        [TestMethod]
        [Description("Try parse musical time in form of '/Denominator Tuplet Dots'.")]
        public void TryParse_Fraction_Tuplet_Dots()
        {
            TimeParsingTester.TestTryParse("/3[4:7]..", (MusicalTime)MusicalFraction.CreateDottedTuplet(3, 2, 4, 7));
        }

        [TestMethod]
        [Description("Try parse musical time in form of 'Numerator/Denominator Dots'.")]
        public void TryParse_Fraction_Dots()
        {
            TimeParsingTester.TestTryParse("2/3..", (MusicalTime)MusicalFraction.CreateDotted(2, 3, 2));
        }

        [TestMethod]
        [Description("Parse string representation of a time.")]
        public void Parse_ToString()
        {
            TimeParsingTester.TestToString(new MusicalTime(3, 6, new Fraction(3, 8)));
        }

        [TestMethod]
        [Description("Calculate difference between two musical times.")]
        public void Subtract_Musical()
        {
            var time1 = new MusicalTime(MusicalFraction.Whole);
            var time2 = new MusicalTime(MusicalFraction.Quarter);

            Assert.AreEqual(new MusicalLength(MusicalFraction.HalfDotted), time1.Subtract(time2, TempoMap.Default));
        }

        [TestMethod]
        [Description("Calculate difference between musical and metric times.")]
        public void Subtract_Metric_TempoChanged()
        {
            var time1 = new MusicalTime(80, 0);
            var time2 = new MetricTime(0, 1, 30);

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 0, 30), Tempo.FromBeatsPerMinute(200));
                tempoMapManager.SetTempo(new MetricTime(0, 2, 0), Tempo.FromBeatsPerMinute(90));

                var tempoMap = tempoMapManager.TempoMap;

                var expected = TimeConverter.ConvertFrom(time1, tempoMap);

                var length = time1.Subtract(time2, tempoMap);
                var actual = TimeConverter.ConvertFrom(time2.Add(length), tempoMap);

                Assert.AreEqual(expected, actual);
            }
        }

        #endregion
    }
}
