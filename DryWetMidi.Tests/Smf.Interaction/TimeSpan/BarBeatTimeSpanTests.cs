using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class BarBeatTimeSpanTests
    {
        #region Test methods

        [TestMethod]
        public void ConvertTo_TwoWholeBars()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //   <-------------------------------->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(2, 0), TimeConverter2.ConvertTo<BarBeatTimeSpan>(MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth, tempoMap));
        }

        [TestMethod]
        public void ConvertTo_WholeBarAndPartOfAnotherOne()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //   <-------------------------->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(1, 3), TimeConverter2.ConvertTo<BarBeatTimeSpan>(MusicalTimeSpan.Whole + 3 * MusicalTimeSpan.Eighth, tempoMap));
        }

        [TestMethod]
        public void ConvertTo_PartOfBar()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //        <------->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(0, 2),
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(MusicalTimeSpan.Half,
                                                                        MusicalTimeSpan.Quarter,
                                                                        tempoMap));
        }

        [TestMethod]
        public void ConvertTo_PartOfAnotherBar()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //                          <--->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(0, 2),
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(MusicalTimeSpan.Quarter,
                                                                        MusicalTimeSpan.Whole + MusicalTimeSpan.Eighth,
                                                                        tempoMap));
        }

        [TestMethod]
        public void ConvertTo_WholeBarIntersectingOneBar()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //             <---------------->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(1, 1),
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(MusicalTimeSpan.Half + 3 * MusicalTimeSpan.Eighth,
                                                                        MusicalTimeSpan.Half,
                                                                        tempoMap));
        }

        [TestMethod]
        public void ConvertTo_TwoWholeBarsIntersectingTwoBars()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //             <-------------------------->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(2, 0),
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth + 2 * MusicalTimeSpan.Sixteenth,
                                                                        MusicalTimeSpan.Half,
                                                                        tempoMap));
        }

        [TestMethod]
        public void ConvertTo_IntersectingOneBar()
        {
            // 4/4                 5/8            5/16
            //  |----'----'----'----|--'--'--'--'--|-'-'-'-'-|
            //  0                   1              2         3
            //                       <---------------->

            var tempoMap = GenerateTestTempoMap();
            Assert.AreEqual(new BarBeatTimeSpan(1, 2),
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(5 * MusicalTimeSpan.Eighth + 2 * MusicalTimeSpan.Sixteenth,
                                                                        MusicalTimeSpan.Whole,
                                                                        tempoMap));
        }

        #endregion

        #region Private methods

        private static TempoMap GenerateTestTempoMap()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(MusicalTimeSpan.Whole, new TimeSignature(5, 8));
                tempoMapManager.SetTimeSignature(MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth, new TimeSignature(5, 16));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
