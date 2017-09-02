using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class MathTimeTests
    {
        #region Test methods

        [TestMethod]
        [Description("Subtract metric length from metric time.")]
        public void Conversion_Metric_Metric_Subtract()
        {
            var tempoMap = TempoMap.Default;

            var mathTime = new MetricTime(0, 1, 30).Subtract(new MetricLength(0, 1, 0));

            Assert.AreEqual(TimeConverter.ConvertFrom(new MetricTime(0, 0, 30), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Subtract metric length from metric time crossing point of tempo change.")]
        public void Conversion_Metric_Metric_Subtract_TempoChanged()
        {
            TempoMap tempoMap = null;
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 1, 20), new Tempo(20000));
                tempoMap = tempoMapManager.TempoMap;
            }

            var mathTime = new MetricTime(0, 1, 30).Subtract(new MetricLength(0, 1, 0));

            Assert.AreEqual(TimeConverter.ConvertFrom(new MetricTime(0, 0, 30), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Subtract math length (made up from metric lengths) from metric time crossing points of tempo change.")]
        public void Conversion_Metric_Math_Subtract_TempoChanged()
        {
            TempoMap tempoMap = null;
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTime(0, 1, 5), new Tempo(20000));
                tempoMapManager.SetTempo(new MetricTime(0, 1, 50), new Tempo(2400));
                tempoMap = tempoMapManager.TempoMap;
            }

            var mathLength = new MetricLength(0, 0, 20).Add(new MetricLength(0, 0, 50));
            var mathTime = new MetricTime(0, 2, 0).Subtract(mathLength);

            Assert.AreEqual(TimeConverter.ConvertFrom(new MetricTime(0, 0, 50), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Subtract musical length from musical time.")]
        public void Conversion_Musical_Musical_Subtract()
        {
            var tempoMap = TempoMap.Default;

            var mathTime = new MusicalTime(2, 0).Subtract(new MusicalLength(MusicalFraction.Quarter));

            Assert.AreEqual(TimeConverter.ConvertFrom(new MusicalTime(7 * MusicalFraction.Quarter), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Subtract musical length from musical time crossing point of time signature change.")]
        public void Conversion_Musical_Musical_Subtract_TimeSignatureChanged()
        {
            //
            // 4/4               1/2  3/4
            //  |--------|--------|----|------|
            //  0        1        2  < 3   <  4
            //                       <-----<
            // 
            // We are moving from 3 bars and 2 beats by 3 quarter lengths,
            // target time is 2 bars and 0.5 beats (one quarter)

            TempoMap tempoMap = null;
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(new MusicalTime(2, 0), new TimeSignature(1, 2));
                tempoMapManager.SetTimeSignature(new MusicalTime(3, 0), new TimeSignature(3, 4));
                tempoMap = tempoMapManager.TempoMap;
            }

            var mathTime = new MusicalTime(3, 2).Subtract(new MusicalLength(3 * MusicalFraction.Quarter));

            Assert.AreEqual(TimeConverter.ConvertFrom(new MusicalTime(2, 0, MusicalFraction.Quarter), tempoMap),
                            TimeConverter.ConvertFrom(mathTime, tempoMap));
        }

        [TestMethod]
        [Description("Try parse musical time plus musical length.")]
        public void TryParse_MusicalPlusMusical()
        {
            TimeParsingTester.TestTryParse("0.0.4/5 + 6/8",
                                           new MathTime(new MusicalTime(new Fraction(4, 5)),
                                                        new MusicalLength(new Fraction(6, 8)),
                                                        MathOperation.Add));
        }

        [TestMethod]
        [Description("Try parse musical time plus metric length.")]
        public void TryParse_MusicalPlusMetric()
        {
            TimeParsingTester.TestTryParse("4/5 + 7:56",
                                           new MathTime(new MusicalTime(new Fraction(4, 5)),
                                                        new MetricLength(0, 7, 56),
                                                        MathOperation.Add));
        }

        [TestMethod]
        [Description("Try parse metric time minus MIDI length.")]
        public void TryParse_MetricMinusMidi()
        {
            TimeParsingTester.TestTryParse("0:0:0:123 - 756",
                                           new MathTime(new MetricTime(0, 0, 0, 123),
                                                        new MidiLength(756),
                                                        MathOperation.Subtract));
        }

        #endregion
    }
}
