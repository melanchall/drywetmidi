using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class BarBeatTimeSpanTests
    {
        #region Constants

        private static readonly TempoMap DefaultTempoMap = GenerateDefaultTempoMap();
        private static readonly TempoMap SimpleTempoMap = GenerateSimpleTempoMap();
        private static readonly TempoMap ComplexTempoMap = GenerateComplexTempoMap();

        #endregion

        #region Test methods

        #region Convert

        #region Default

        [TestMethod]
        [Description("Conversion of two 4/4 bars placed at bar start.")]
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
        [Description("Conversion of two 4/4 bars placed at beat start.")]
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
        [Description("Conversion of arbitrary time span overlaying one 4/4 bar.")]
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
        [Description("Conversion of 4/4 time span placed at beat start and ended at bar start.")]
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
        [Description("Conversion of arbitrary time span overlaying one 4/4 bar.")]
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
        [Description("Conversion of two 4/4 beats crossing bar start.")]
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
        [Description("Conversion of two 4/4 beats at the middle of bar.")]
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
        [Description("Conversion of one 4/4 beat placed at beat start and ended at bar start.")]
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
        [Description("Conversion of one 4/4 beat placed at subbeat position.")]
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

        #region Parse

        [TestMethod]
        [Description("Parse zero bar-beat time span.")]
        public void Parse_1()
        {
            TimeSpanTestUtilities.Parse("0.0.0", new BarBeatTimeSpan());
        }

        [TestMethod]
        [Description("Parse one-bar time span.")]
        public void Parse_2()
        {
            TimeSpanTestUtilities.Parse("1.0.0", new BarBeatTimeSpan(1, 0, 0));
        }

        [TestMethod]
        [Description("Parse arbitrary bar-beat time span.")]
        public void Parse_3()
        {
            TimeSpanTestUtilities.Parse("0.10.5", new BarBeatTimeSpan(0, 10, 5));
        }

        [TestMethod]
        [Description("Parse arbitrary bar-beat time span.")]
        public void Parse_4()
        {
            TimeSpanTestUtilities.Parse("100.20.0", new BarBeatTimeSpan(100, 20, 0));
        }

        #endregion

        #region Add

        [TestMethod]
        public void Add_SameType_1()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatTimeSpan(0, 0, 0),
                                               new BarBeatTimeSpan(0, 0, 0),
                                               new BarBeatTimeSpan(0, 0, 0),
                                               TimeSpanMode.TimeTime);
        }

        [TestMethod]
        public void Add_SameType_2()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatTimeSpan(10, 0, 10),
                                               new BarBeatTimeSpan(0, 0, 0),
                                               new BarBeatTimeSpan(10, 0, 10),
                                               TimeSpanMode.TimeTime);
        }

        [TestMethod]
        public void Add_SameType_3()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatTimeSpan(10, 0, 10),
                                               new BarBeatTimeSpan(0, 3, 5),
                                               new BarBeatTimeSpan(10, 3, 15),
                                               TimeSpanMode.TimeTime);
        }

        [TestMethod]
        public void Add_TimeLength_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(new BarBeatTimeSpan(1, 2, 10),
                                                 new MetricTimeSpan(0, 2, 30),
                                                 DefaultTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(new BarBeatTimeSpan(1, 2, 10),
                                                 new MetricTimeSpan(0, 2, 30),
                                                 SimpleTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_3()
        {
            TimeSpanTestUtilities.Add_TimeLength(new BarBeatTimeSpan(1, 2, 10),
                                                 new MetricTimeSpan(0, 2, 30),
                                                 ComplexTempoMap);
        }

        [TestMethod]
        public void Add_LengthLength_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   DefaultTempoMap,
                                                   0);
        }

        [TestMethod]
        public void Add_LengthLength_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   DefaultTempoMap,
                                                   1000);
        }

        [TestMethod]
        public void Add_LengthLength_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   DefaultTempoMap,
                                                   100000);
        }

        [TestMethod]
        public void Add_LengthLength_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   SimpleTempoMap,
                                                   0);
        }

        [TestMethod]
        public void Add_LengthLength_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   SimpleTempoMap,
                                                   1000);
        }

        [TestMethod]
        public void Add_LengthLength_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   SimpleTempoMap,
                                                   100000);
        }

        [TestMethod]
        public void Add_LengthLength_7()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   ComplexTempoMap,
                                                   0);
        }

        [TestMethod]
        public void Add_LengthLength_8()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   ComplexTempoMap,
                                                   1000);
        }

        [TestMethod]
        public void Add_LengthLength_9()
        {
            TimeSpanTestUtilities.Add_LengthLength(new BarBeatTimeSpan(1, 2, 10),
                                                   new MetricTimeSpan(0, 2, 30),
                                                   ComplexTempoMap,
                                                   100000);
        }

        #endregion

        #endregion

        #region Private methods

        private static void TestConversion(BarBeatTimeSpan barBeatTimeSpan, ITimeSpan referenceTimeSpan, ITimeSpan time, TempoMap tempoMap)
        {
            time = time ?? new MidiTimeSpan();

            Assert.AreEqual(barBeatTimeSpan,
                            LengthConverter2.ConvertTo<BarBeatTimeSpan>(referenceTimeSpan, time, tempoMap),
                            "ConvertTo failed.");

            Assert.AreEqual(LengthConverter2.ConvertFrom(referenceTimeSpan, time, tempoMap),
                            LengthConverter2.ConvertFrom(barBeatTimeSpan, time, tempoMap),
                            "ConvertFrom failed.");
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

                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromMillisecondsPerQuarterNote(300));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 30), Tempo.FromMillisecondsPerQuarterNote(600));

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

                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromMillisecondsPerQuarterNote(300));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 30), Tempo.FromMillisecondsPerQuarterNote(600));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 31), Tempo.FromMillisecondsPerQuarterNote(680));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
