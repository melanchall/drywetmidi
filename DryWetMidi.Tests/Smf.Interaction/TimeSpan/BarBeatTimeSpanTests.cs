using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class BarBeatTimeSpanTests : TimeSpanTests
    {
        #region Constants

        private static readonly TempoMap DefaultTempoMap = GenerateDefaultTempoMap();
        private static readonly TempoMap SimpleTempoMap = GenerateSimpleTempoMap();
        private static readonly TempoMap ComplexTempoMap = GenerateComplexTempoMap();

        #endregion

        #region Overrides

        protected override IEnumerable<TimeSpanParseInfo> TimeSpansToParse => new[]
        {
            new TimeSpanParseInfo("0.0.0", new BarBeatTimeSpan()),
            new TimeSpanParseInfo("1.0.0", new BarBeatTimeSpan(1, 0, 0)),
            new TimeSpanParseInfo("0.10.5", new BarBeatTimeSpan(0, 10, 5)),
            new TimeSpanParseInfo("100.20.0", new BarBeatTimeSpan(100, 20, 0)),
        };

        protected override IEnumerable<TimeSpansOperationInfo> TimeSpansToAdd => new[]
        {
            new TimeSpansOperationInfo(new BarBeatTimeSpan(0, 1, 2), new BarBeatTimeSpan(10, 0, 0), new BarBeatTimeSpan(10, 1, 2)),
            new TimeSpansOperationInfo(new BarBeatTimeSpan(0, 1, 2), new MetricTimeSpan(0, 1, 30)),
            new TimeSpansOperationInfo(new BarBeatTimeSpan(0, 1, 2), MusicalTimeSpan.Quarter.Triplet()),
        };

        #endregion

        #region Test methods

        #region Convert

        #region Default

        [TestMethod]
        public void Convert_Default_1()
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
        public void Convert_Default_2()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //       |=======================================|
            //       '              ^                   ^    '

            TestConversion(new BarBeatTimeSpan(2, 0),
                           2 * MusicalTimeSpan.Whole,
                           MusicalTimeSpan.Quarter,
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_3()
        {
            // 4/4
            //  |--------+--------+--------+--------|--------+--------+--------+--------|--------+--------+--------+--------|
            //  0                                   1                                   2                                   3
            //                          |=======================================================|
            //                             '        ^                                   ^

            TestConversion(new BarBeatTimeSpan(1, 2, 60),
                           MusicalTimeSpan.Whole.SingleDotted() + MusicalTimeSpan.ThirtySecond,
                           MusicalTimeSpan.Half + 3 * MusicalTimeSpan.Sixteenth,
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_4()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                 |========================|
            //                 '    ^                   ^

            TestConversion(new BarBeatTimeSpan(1, 1),
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                           MusicalTimeSpan.Half.SingleDotted(),
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_5()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----------------+----+----+----|
            //  0                   1                   2                               3
            //                 |============================|
            //                 '    ^                   ^

            TestConversion(new BarBeatTimeSpan(1, 1, 90),
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + 3 * MusicalTimeSpan.SixtyFourth,
                           MusicalTimeSpan.Half.SingleDotted(),
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_6()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                 |=========|
            //                 '    ^    '

            TestConversion(new BarBeatTimeSpan(0, 2),
                           MusicalTimeSpan.Half,
                           MusicalTimeSpan.Half.SingleDotted(),
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_7()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                           |=========|
            //                           '    '    '

            TestConversion(new BarBeatTimeSpan(0, 2),
                           MusicalTimeSpan.Half,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_8()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                                     |====|
            //                                     '    ^

            TestConversion(new BarBeatTimeSpan(0, 1),
                           MusicalTimeSpan.Quarter,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Half.SingleDotted(),
                           DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_9()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+--------|--------+----+----+----|
            //  0                   1                       2                       3
            //                                      |=========|
            //                                              ^

            TestConversion(new BarBeatTimeSpan(0, 1),
                           MusicalTimeSpan.Quarter,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Half.SingleDotted() + MusicalTimeSpan.ThirtySecond,
                           DefaultTempoMap);
        }

        #endregion

        #region Simple

        [TestMethod]
        public void Convert_Simple_1()
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
        public void Convert_Simple_2()
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
        public void Convert_Simple_3()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //       |=========|
            //       '    '    '

            TestConversion(new BarBeatTimeSpan(0, 2),
                           MusicalTimeSpan.Half,
                           MusicalTimeSpan.Quarter,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_4()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                         |=====|
            //                         '  '  '

            TestConversion(new BarBeatTimeSpan(0, 2),
                           MusicalTimeSpan.Quarter,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Eighth,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_5()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |==================|
            //            '    '    ^  '  '  '

            TestConversion(new BarBeatTimeSpan(1, 1),
                           MusicalTimeSpan.Half + 3 * MusicalTimeSpan.Eighth,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_6()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |==============================|
            //            '    '    ^              ^ ' ' '

            TestConversion(new BarBeatTimeSpan(2, 1),
                           MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth + 3 * MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_7()
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
        public void Convert_Simple_8()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |========================|
            //            '    '    ^              ^

            TestConversion(new BarBeatTimeSpan(1, 2),
                           MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_9()
        {
            // 4/4                             5/8            5/16
            //  |----+----+----+----------------|--+--+--+--+--|----+-+-+-+-|
            //  0                               1              2         3
            //                  |==================================|
            //                                  ^              ^

            TestConversion(new BarBeatTimeSpan(1, 1, 60),
                           MusicalTimeSpan.Quarter + 5 * MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond,
                           MusicalTimeSpan.Half.SingleDotted() + MusicalTimeSpan.SixtyFourth,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_10()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |===========|
            //            '    '    ^

            TestConversion(new BarBeatTimeSpan(0, 2, 60),
                           MusicalTimeSpan.Half + MusicalTimeSpan.ThirtySecond,
                           MusicalTimeSpan.Half,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_11()
        {
            // 4/4                             5/8            5/16
            //  |----+----+----------------+----|--+--+--+--+--|-+-+-+-+-|
            //  0                               1              2         3
            //             |===|
            //            

            TestConversion(new BarBeatTimeSpan(0, 0, 90),
                           3 * MusicalTimeSpan.SixtyFourth,
                           MusicalTimeSpan.Half + MusicalTimeSpan.SixtyFourth,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_12()
        {
            // 4/4                 5/8                  5/16
            //  |----+----+----+----|--------+--+--+--+--|-+-+-+-+-|
            //  0                   1                    2         3
            //                          |===|
            //            

            TestConversion(new BarBeatTimeSpan(0, 0, 90),
                           3 * MusicalTimeSpan.SixtyFourth,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Sixteenth,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_13()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                            |==========|
            //                            '  '  '  ^ '

            TestConversion(new BarBeatTimeSpan(0, 4, 0),
                           3 * MusicalTimeSpan.Eighth + MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_14()
        {
            // 4/4                 5/8                  5/16
            //  |----+----+----+----|--+--+--------+--+--|--+-+-+-+-|
            //  0                   1                    2          3
            //                             |===============|
            //                                     '  '  ^

            TestConversion(new BarBeatTimeSpan(0, 3, 30),
                           3 * MusicalTimeSpan.Eighth + MusicalTimeSpan.SixtyFourth,
                           MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + MusicalTimeSpan.SixtyFourth,
                           SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_15()
        {
            // 4/4                             5/8                  5/16
            //  |----+----+----+----------------|--+--+--+--+--------|--+-+-+-+-|
            //  0                               1                    2          3
            //                                |=====================|
            //                                  ^  '  '  '  '

            TestConversion(new BarBeatTimeSpan(0, 4, 240),
                           5 * MusicalTimeSpan.Eighth,
                           MusicalTimeSpan.Half.SingleDotted() + 15 * MusicalTimeSpan.SixtyFourth,
                           SimpleTempoMap);
        }

        #endregion

        #region Complex

        [TestMethod]
        public void Convert_Complex_1()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //            |==========================================================|
            //            '    '    ^                   ^              ^         ^ ' '

            TestConversion(new BarBeatTimeSpan(4, 0),
                           MusicalTimeSpan.Half + MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 7 * MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Half,
                           ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_2()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //            |==================================================================|
            //            '    '    ^                   ^              ^         ^         ^ '

            TestConversion(new BarBeatTimeSpan(4, 3),
                           MusicalTimeSpan.Half + MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 11 * MusicalTimeSpan.Sixteenth,
                           MusicalTimeSpan.Half,
                           ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_3()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //                                |===============================================================|
            //                                '    '    ^              ^         ^         ^         ^  '  '  '

            TestConversion(new BarBeatTimeSpan(5, 1),
                           MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth + 15 * MusicalTimeSpan.Sixteenth + 3 * MusicalTimeSpan.Eighth,
                           MusicalTimeSpan.Whole.SingleDotted(),
                           ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_4()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //                                                                                 |===========|
            //                                                                                 ' ' ' ^  '  '

            TestConversion(new BarBeatTimeSpan(1, 0),
                           3 * MusicalTimeSpan.Sixteenth + 2 * MusicalTimeSpan.Eighth,
                           2 * MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 12 * MusicalTimeSpan.Sixteenth,
                           ComplexTempoMap);
        }

        #endregion

        #endregion

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

        private static TempoMap GenerateDefaultTempoMap()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(480)))
            {
                return tempoMapManager.TempoMap;
            }
        }

        private static TempoMap GenerateSimpleTempoMap()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(480)))
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

            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(480)))
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
