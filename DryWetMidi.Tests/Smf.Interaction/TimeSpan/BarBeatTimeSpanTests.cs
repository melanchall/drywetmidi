using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class BarBeatTimeSpanTests
    {
        #region Constants

        private static readonly TempoMap DefaultTempoMap = TempoMap.Default;
        private static readonly TempoMap SimpleTempoMap = GenerateSimpleTempoMap();
        private static readonly TempoMap ComplexTempoMap = GenerateComplexTempoMap();

        #endregion

        #region Test methods

        [TestMethod]
        public void Convert_Simple_SeveralWholeBars_FromZero()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //  |=======================================|
            //  ^                   ^                   ^

            TestConversion(new BarBeatTimeSpan(2, 0),
                           2 * MusicalTimeSpan.Whole,
                           null,
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_SeveralWholeBars_FromMiddle()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //       |=======================================|
            //       ^                   ^                   ^

            TestConversion(new BarBeatTimeSpan(2, 0),
                           2 * MusicalTimeSpan.Whole,
                           MusicalTimeSpan.Quarter,
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_TwoWholeBars()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //  |==================================|
            //  ^                   ^              ^

            TestConversion(new BarBeatTimeSpan(2, 0),
                           MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth,
                           null,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_WholeBarAndPartOfAnotherOne()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //  |============================|
            //  ^                   ^  '  '  '

            TestConversion(new BarBeatTimeSpan(1, 3),
                           MusicalTimeSpan.Whole + 3 * MusicalTimeSpan.Eighth,
                           null,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_PartOfBar_DefaultSignature()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //       |=========|
            //       ^    '    '

            TestConversion(new BarBeatTimeSpan(0, 2),
                           MusicalTimeSpan.Half,
                           MusicalTimeSpan.Quarter,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_PartOfBar_CustomSignature()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                         |=====|
            //                         ^  '  '

            TestConversion(new BarBeatTimeSpan(0, 2),
                           MusicalTimeSpan.Quarter,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Eighth,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_WholeBarIntersectingOneBar()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |==================|
            //            ^               ^  '

            TestConversion(new BarBeatTimeSpan(1, 1),
                           MusicalTimeSpan.Half + 3 * MusicalTimeSpan.Eighth,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_TwoWholeBarsIntersectingTwoBars()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |============================|
            //            ^               ^            ^

            TestConversion(new BarBeatTimeSpan(2, 0),
                           MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth + 2 * MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_IntersectingOneBar()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                      |==================|
            //                      ^              ^ ' '

            TestConversion(new BarBeatTimeSpan(1, 2),
                           5 * MusicalTimeSpan.Eighth + 2 * MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Whole,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_MiddleToEnd()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |========================|
            //            ^               ^  '  '  '

            TestConversion(new BarBeatTimeSpan(1, 3),
                           MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_1()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //            |==========================================================|
            //            ^                   ^               ^            ^         ^

            TestConversion(new BarBeatTimeSpan(4, 0),
                           MusicalTimeSpan.Half + MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 7 * MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Half,
                           ComplexTempoMap);
        }

        #endregion

        #region Private methods

        private static void TestConversion(BarBeatTimeSpan barBeatTimeSpan, ITimeSpan referenceTimeSpan, ITimeSpan time, TempoMap tempoMap)
        {
            time = time ?? new MidiTimeSpan();

            Assert.AreEqual(barBeatTimeSpan,
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(referenceTimeSpan, time, tempoMap),
                            "Convert to failed.");

            Assert.AreEqual(LengthConverter2.ConvertFrom(referenceTimeSpan, time, tempoMap),
                            LengthConverter2.ConvertFrom(barBeatTimeSpan, time, tempoMap),
                            "Convert from failed.");
        }

        private static TempoMap GenerateSimpleTempoMap()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3

            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTimeSignature(MusicalTimeSpan.Whole, new TimeSignature(5, 8));
                tempoMapManager.SetTimeSignature(MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth, new TimeSignature(5, 16));

                return tempoMapManager.TempoMap;
            }
        }

        private static TempoMap GenerateComplexTempoMap()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7

            var steps = new[]
            {
                Tuple.Create(2 * MusicalTimeSpan.Whole, new TimeSignature(5, 8)),
                Tuple.Create(5 * MusicalTimeSpan.Eighth, new TimeSignature(5, 16)),
                Tuple.Create(15 * MusicalTimeSpan.Sixteenth, new TimeSignature(5, 8)),
            };

            using (var tempoMapManager = new TempoMapManager())
            {
                var time = new MusicalTimeSpan();

                foreach (var step in steps)
                {
                    time += step.Item1;
                    tempoMapManager.SetTimeSignature(time, step.Item2);
                }

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
