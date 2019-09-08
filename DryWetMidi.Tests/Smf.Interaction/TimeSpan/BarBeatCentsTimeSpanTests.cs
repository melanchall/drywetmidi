using System;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class BarBeatCentsTimeSpanTests
    {
        #region Constants

        private static readonly ITimeSpan MetricSpan = new MetricTimeSpan(0, 2, 30);
        private static readonly ITimeSpan BarBeatCentsSpan = new BarBeatCentsTimeSpan(1, 2, 5);

        private const long ZeroTime = 0;
        private const long ShortTime = 1000;
        private const long LargeTime = 100000;

        private static readonly Tuple<BarBeatCentsTimeSpan, BarBeatCentsTimeSpan>[] TimeSpansForComparison_Less = new[]
        {
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan(0, 0, 1)),
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan(0, 1, 0)),
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan(1, 0, 0)),
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan(0, 0, 1)),
            Tuple.Create(new BarBeatCentsTimeSpan(2, 0, 0), new BarBeatCentsTimeSpan(10, 0, 1)),
            Tuple.Create(new BarBeatCentsTimeSpan(0, 10, 0), new BarBeatCentsTimeSpan(0, 10, 1)),
            Tuple.Create(new BarBeatCentsTimeSpan(10, 10, 0), new BarBeatCentsTimeSpan(10, 10, 1)),
            Tuple.Create(new BarBeatCentsTimeSpan(10000, 899, 0), new BarBeatCentsTimeSpan(10000, 10000, 0)),
            Tuple.Create(new BarBeatCentsTimeSpan(0, 100), new BarBeatCentsTimeSpan(0, 110, 1)),
            Tuple.Create(new BarBeatCentsTimeSpan(199, 0, 10), new BarBeatCentsTimeSpan(200, 0, 80)),
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan(10, 110, 91))
        };

        private static readonly Tuple<BarBeatCentsTimeSpan, BarBeatCentsTimeSpan>[] TimeSpansForComparison_Equal = new[]
        {
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan()),
            Tuple.Create(new BarBeatCentsTimeSpan(), new BarBeatCentsTimeSpan(0, 0, 0)),
            Tuple.Create(new BarBeatCentsTimeSpan(10, 0, 0), new BarBeatCentsTimeSpan(10, 0, 0)),
            Tuple.Create(new BarBeatCentsTimeSpan(100, 100, 10), new BarBeatCentsTimeSpan(100, 100, 10)),
            Tuple.Create(new BarBeatCentsTimeSpan(0, 345, 0), new BarBeatCentsTimeSpan(0, 345, 0)),
            Tuple.Create(new BarBeatCentsTimeSpan(0, 0, 34.5), new BarBeatCentsTimeSpan(0, 0, 34.5))
        };

        #endregion

        #region Test methods

        #region Convert

        #region Default

        [Test]
        [Description("Conversion of two 4/4 bars placed at bar start.")]
        public void Convert_Default_1()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //  |=======================================|
            //  ^                   ^                   ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(2, 0),
                                                 2 * MusicalTimeSpan.Whole,
                                                 null,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of two 4/4 bars placed at beat start.")]
        public void Convert_Default_2()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //       |=======================================|
            //       '              ^                   ^    '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(2, 0),
                                                 2 * MusicalTimeSpan.Whole,
                                                 MusicalTimeSpan.Quarter,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of arbitrary time span overlaying one 4/4 bar.")]
        public void Convert_Default_3()
        {
            // 4/4
            //  |--------+--------+--------+--------|--------+--------+--------+--------|--------+--------+--------+--------|
            //  0                                   1                                   2                                   3
            //                          |=======================================================|
            //                             '        ^                                   ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 2, 12.5),
                                                 MusicalTimeSpan.Whole.SingleDotted() + MusicalTimeSpan.ThirtySecond,
                                                 MusicalTimeSpan.Half + 3 * MusicalTimeSpan.Sixteenth,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of 4/4 time span placed at beat start and ended at bar start.")]
        public void Convert_Default_4()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                 |========================|
            //                 '    ^                   ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 1),
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                                                 MusicalTimeSpan.Half.SingleDotted(),
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of arbitrary time span overlaying one 4/4 bar.")]
        public void Convert_Default_5()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----------------+----+----+----|
            //  0                   1                   2                               3
            //                 |============================|
            //                 '    ^                   ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 1, 18.75),
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + 3 * MusicalTimeSpan.SixtyFourth,
                                                 MusicalTimeSpan.Half.SingleDotted(),
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of two 4/4 beats crossing bar start.")]
        public void Convert_Default_6()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                 |=========|
            //                 '    ^    '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 2),
                                                 MusicalTimeSpan.Half,
                                                 MusicalTimeSpan.Half.SingleDotted(),
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of two 4/4 beats at the middle of bar.")]
        public void Convert_Default_7()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                           |=========|
            //                           '    '    '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 2),
                                                 MusicalTimeSpan.Half,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of one 4/4 beat placed at beat start and ended at bar start.")]
        public void Convert_Default_8()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+----|----+----+----+----|
            //  0                   1                   2                   3
            //                                     |====|
            //                                     '    ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 1),
                                                 MusicalTimeSpan.Quarter,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Half.SingleDotted(),
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        [Description("Conversion of one 4/4 beat placed at subbeat position.")]
        public void Convert_Default_9()
        {
            // 4/4
            //  |----+----+----+----|----+----+----+--------|--------+----+----+----|
            //  0                   1                       2                       3
            //                                      |=========|
            //                                              ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 1),
                                                 MusicalTimeSpan.Quarter,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Half.SingleDotted() + MusicalTimeSpan.ThirtySecond,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        #endregion

        #region Simple

        [Test]
        public void Convert_Simple_1()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //  |==================================|
            //  ^                   ^              ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(2, 0),
                                                 MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth,
                                                 null,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_2()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //  |============================|
            //  ^                   ^  '  '  '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 3),
                                                 MusicalTimeSpan.Whole + 3 * MusicalTimeSpan.Eighth,
                                                 null,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_3()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //       |=========|
            //       '    '    '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 2),
                                                 MusicalTimeSpan.Half,
                                                 MusicalTimeSpan.Quarter,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_4()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                         |=====|
            //                         '  '  '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 2),
                                                 MusicalTimeSpan.Quarter,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Eighth,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_5()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |==================|
            //            '    '    ^  '  '  '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 1),
                                                 MusicalTimeSpan.Half + 3 * MusicalTimeSpan.Eighth,
                                                 MusicalTimeSpan.Half,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_6()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |==============================|
            //            '    '    ^              ^ ' ' '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(2, 1),
                                                 MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth + 3 * MusicalTimeSpan.Sixteenth,
                                                 MusicalTimeSpan.Half,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_7()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                      |==================|
            //                      ^              ^ ' '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 2),
                                                 5 * MusicalTimeSpan.Eighth + 2 * MusicalTimeSpan.Sixteenth,
                                                 MusicalTimeSpan.Whole,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_8()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |========================|
            //            '    '    ^              ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 2),
                                                 MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth,
                                                 MusicalTimeSpan.Half,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_9()
        {
            // 4/4                             5/8            5/16
            //  |----+----+----+----------------|--+--+--+--+--|----+-+-+-+-|
            //  0                               1              2         3
            //                  |==================================|
            //                                  ^              ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 1, 68.75),
                                                 MusicalTimeSpan.Quarter + 5 * MusicalTimeSpan.Eighth + MusicalTimeSpan.ThirtySecond,
                                                 MusicalTimeSpan.Half.SingleDotted() + MusicalTimeSpan.SixtyFourth,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_10()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //            |===========|
            //            '    '    ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 2, 25),
                                                 MusicalTimeSpan.Half + MusicalTimeSpan.ThirtySecond,
                                                 MusicalTimeSpan.Half,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_11()
        {
            // 4/4                             5/8            5/16
            //  |----+----+----------------+----|--+--+--+--+--|-+-+-+-+-|
            //  0                               1              2         3
            //             |===|
            //            

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 0, 18.75),
                                                 3 * MusicalTimeSpan.SixtyFourth,
                                                 MusicalTimeSpan.Half + MusicalTimeSpan.SixtyFourth,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_12()
        {
            // 4/4                 5/8                  5/16
            //  |----+----+----+----|--------+--+--+--+--|-+-+-+-+-|
            //  0                   1                    2         3
            //                          |===|
            //            

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 0, 37.5),
                                                 3 * MusicalTimeSpan.SixtyFourth,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Sixteenth,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_13()
        {
            // 4/4                 5/8            5/16
            //  |----+----+----+----|--+--+--+--+--|-+-+-+-+-|
            //  0                   1              2         3
            //                            |==========|
            //                            '  '  '  ^ '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 4, 0),
                                                 3 * MusicalTimeSpan.Eighth + MusicalTimeSpan.Sixteenth,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_14()
        {
            // 4/4                 5/8                  5/16
            //  |----+----+----+----|--+--+--------+--+--|--+-+-+-+-|
            //  0                   1                    2          3
            //                             |===============|
            //                                     '  '  ^

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 3, 37.5),
                                                 3 * MusicalTimeSpan.Eighth + MusicalTimeSpan.SixtyFourth,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Quarter + MusicalTimeSpan.SixtyFourth,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_15()
        {
            // 4/4                             5/8                  5/16
            //  |----+----+----+----------------|--+--+--+--+--------|--+-+-+-+-|
            //  0                               1                    2          3
            //                                |=====================|
            //                                  ^  '  '  '  '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(0, 4, 93.75),
                                                 5 * MusicalTimeSpan.Eighth,
                                                 MusicalTimeSpan.Half.SingleDotted() + 15 * MusicalTimeSpan.SixtyFourth,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        #endregion

        #region Complex

        [Test]
        public void Convert_Complex_1()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //            |==========================================================|
            //            '    '    ^                   ^              ^         ^ ' '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(4, 0),
                                                 MusicalTimeSpan.Half + MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 7 * MusicalTimeSpan.Sixteenth,
                                                 MusicalTimeSpan.Half,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_2()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //            |==================================================================|
            //            '    '    ^                   ^              ^         ^         ^ '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(4, 3),
                                                 MusicalTimeSpan.Half + MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 11 * MusicalTimeSpan.Sixteenth,
                                                 MusicalTimeSpan.Half,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_3()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //                                |===============================================================|
            //                                '    '    ^              ^         ^         ^         ^  '  '  '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(5, 1),
                                                 MusicalTimeSpan.Half + 5 * MusicalTimeSpan.Eighth + 15 * MusicalTimeSpan.Sixteenth + 3 * MusicalTimeSpan.Eighth,
                                                 MusicalTimeSpan.Whole.SingleDotted(),
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_4()
        {
            // 4/4                                     5/8            5/16                          5/8
            //  |----+----+----+----|----+----+----+----|--+--+--+--+--|-+-+-+-+-|-+-+-+-+-|-+-+-+-+-|--+--+--+--+--|
            //  0                   1                   2              3         4         5         6              7
            //                                                                                 |===========|
            //                                                                                 ' ' ' ^  '  '

            TimeSpanTestUtilities.TestConversion(new BarBeatCentsTimeSpan(1, 0),
                                                 3 * MusicalTimeSpan.Sixteenth + 2 * MusicalTimeSpan.Eighth,
                                                 2 * MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 12 * MusicalTimeSpan.Sixteenth,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        #endregion

        #endregion

        #region Parse

        [Test]
        [Description("Parse zero bar-beat time span.")]
        public void Parse_1()
        {
            TimeSpanTestUtilities.Parse("0.0.0.0", new BarBeatCentsTimeSpan());
        }

        [Test]
        [Description("Parse one-bar time span.")]
        public void Parse_2()
        {
            TimeSpanTestUtilities.Parse("1.0.0.2", new BarBeatCentsTimeSpan(1, 0, 0.2));
        }

        [Test]
        [Description("Parse arbitrary bar-beat time span.")]
        public void Parse_3()
        {
            TimeSpanTestUtilities.Parse("0.10.5.0", new BarBeatCentsTimeSpan(0, 10, 5.0));
        }

        [Test]
        [Description("Parse arbitrary bar-beat time span.")]
        public void Parse_4()
        {
            TimeSpanTestUtilities.Parse("100.20.56.56", new BarBeatCentsTimeSpan(100, 20, 56.56));
        }

        #endregion

        #region Add

        [Test]
        public void Add_SameType_1()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatCentsTimeSpan(),
                                               new BarBeatCentsTimeSpan(),
                                               new BarBeatCentsTimeSpan());
        }

        [Test]
        public void Add_SameType_2()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatCentsTimeSpan(10, 0, 10),
                                               new BarBeatCentsTimeSpan(),
                                               new BarBeatCentsTimeSpan(10, 0, 10));
        }

        [Test]
        public void Add_SameType_3()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatCentsTimeSpan(10, 0, 10),
                                               new BarBeatCentsTimeSpan(0, 3, 5),
                                               new BarBeatCentsTimeSpan(10, 3, 15));
        }

        [Test]
        public void Add_TimeTime_1()
        {
            TimeSpanTestUtilities.Add_TimeTime(BarBeatCentsSpan,
                                               MetricSpan);
        }

        [Test]
        public void Add_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(BarBeatCentsSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Add_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(BarBeatCentsSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Add_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(BarBeatCentsSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Add_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatCentsSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   LargeTime);
        }

        #endregion

        #region Subtract

        [Test]
        public void Subtract_SameType_1()
        {
            TimeSpanTestUtilities.Subtract_SameType(new BarBeatCentsTimeSpan(),
                                                    new BarBeatCentsTimeSpan(),
                                                    new BarBeatCentsTimeSpan());
        }

        [Test]
        public void Subtract_SameType_2()
        {
            TimeSpanTestUtilities.Subtract_SameType(new BarBeatCentsTimeSpan(10, 0, 10),
                                                    new BarBeatCentsTimeSpan(),
                                                    new BarBeatCentsTimeSpan(10, 0, 10));
        }

        [Test]
        public void Subtract_SameType_3()
        {
            TimeSpanTestUtilities.Subtract_SameType(new BarBeatCentsTimeSpan(10, 7, 10),
                                                    new BarBeatCentsTimeSpan(0, 3, 6),
                                                    new BarBeatCentsTimeSpan(10, 4, 4));
        }

        [Test]
        public void Subtract_TimeTime_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    BarBeatCentsSpan,
                                                    TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Subtract_TimeTime_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    BarBeatCentsSpan,
                                                    TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Subtract_TimeTime_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    BarBeatCentsSpan,
                                                    TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      BarBeatCentsSpan,
                                                      TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      BarBeatCentsSpan,
                                                      TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      BarBeatCentsSpan,
                                                      TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Subtract_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        LargeTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        LargeTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatCentsSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        LargeTime);
        }

        #endregion

        #region Multiply

        [Test]
        [Description("Multiply zero time span by zero.")]
        public void Multiply_1()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(),
                            new BarBeatCentsTimeSpan().Multiply(0));
        }

        [Test]
        [Description("Multiply arbitrary time span by zero.")]
        public void Multiply_2()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(),
                            new BarBeatCentsTimeSpan(10, 5, 9).Multiply(0));
        }

        [Test]
        [Description("Multiply by integer number.")]
        public void Multiply_3()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(20, 10, 18),
                            new BarBeatCentsTimeSpan(10, 5, 9).Multiply(2));
        }

        [Test]
        [Description("Multiply by non-integer number.")]
        public void Multiply_4()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(15, 12, 9),
                            new BarBeatCentsTimeSpan(10, 8, 6).Multiply(1.5));
        }

        [Test]
        [Description("Multiply by negative number.")]
        public void Multiply_5()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatCentsTimeSpan().Multiply(-5));
        }

        #endregion

        #region Divide

        [Test]
        [Description("Divide arbitrary time span by one.")]
        public void Divide_1()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(5, 4, 3),
                            new BarBeatCentsTimeSpan(5, 4, 3).Divide(1));
        }

        [Test]
        [Description("Divide arbitrary time span by integer number.")]
        public void Divide_2()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(5, 3, 4),
                            new BarBeatCentsTimeSpan(10, 6, 8).Divide(2));
        }

        [Test]
        [Description("Divide by non-integer number.")]
        public void Divide_3()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(8, 2, 4),
                            new BarBeatCentsTimeSpan(12, 3, 6).Divide(1.5));
        }

        [Test]
        [Description("Divide by zero.")]
        public void Divide_4()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatCentsTimeSpan().Divide(0));
        }

        [Test]
        [Description("Divide by negative number.")]
        public void Divide_5()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new BarBeatCentsTimeSpan().Divide(-8));
        }

        [Test]
        [Description("Divide zero time span by one.")]
        public void Divide_6()
        {
            Assert.AreEqual(new BarBeatCentsTimeSpan(),
                            new BarBeatCentsTimeSpan().Divide(1));
        }

        #endregion

        #region Clone

        [Test]
        public void Clone_1()
        {
            TimeSpanTestUtilities.TestClone(new BarBeatCentsTimeSpan());
        }

        [Test]
        public void Clone_2()
        {
            TimeSpanTestUtilities.TestClone(new BarBeatCentsTimeSpan(1, 2, 3));
        }

        #endregion

        #region Compare

        [Test]
        [Description("Compare two time spans where first one is less than second one.")]
        public void Compare_Less()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Less)
            {
                var timeSpan1 = timeSpansPair.Item1;
                var timeSpan2 = timeSpansPair.Item2;

                Assert.IsTrue(timeSpan1 < timeSpan2,
                              $"{timeSpan1} isn't less than {timeSpan2} using <.");
                Assert.IsTrue(timeSpan1.CompareTo(timeSpan2) < 0,
                              $"{timeSpan1} isn't less than {timeSpan2} using typed CompareTo.");
                Assert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) < 0,
                              $"{timeSpan1} isn't less than {timeSpan2} using CompareTo(object).");
            }
        }

        [Test]
        [Description("Compare two time spans where first one is greater than second one.")]
        public void Compare_Greater()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Less)
            {
                var timeSpan1 = timeSpansPair.Item2;
                var timeSpan2 = timeSpansPair.Item1;

                Assert.IsTrue(timeSpan1 > timeSpan2,
                              $"{timeSpan1} isn't greater than {timeSpan2} using >.");
                Assert.IsTrue(timeSpan1.CompareTo(timeSpan2) > 0,
                              $"{timeSpan1} isn't greater than {timeSpan2} using typed CompareTo.");
                Assert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) > 0,
                              $"{timeSpan1} isn't greater than {timeSpan2} using CompareTo(object).");
            }
        }

        [Test]
        [Description("Compare two time spans where first one is less than or equal to second one.")]
        public void Compare_LessOrEqual()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Less.Concat(TimeSpansForComparison_Equal))
            {
                var timeSpan1 = timeSpansPair.Item1;
                var timeSpan2 = timeSpansPair.Item2;

                Assert.IsTrue(timeSpan1 <= timeSpan2,
                              $"{timeSpan1} isn't less than or equal to {timeSpan2} using <=.");
                Assert.IsTrue(timeSpan1.CompareTo(timeSpan2) <= 0,
                              $"{timeSpan1} isn't less than or equal to {timeSpan2} using typed CompareTo.");
                Assert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) <= 0,
                              $"{timeSpan1} isn't less than or equal to {timeSpan2} using CompareTo(object).");
            }
        }

        [Test]
        [Description("Compare two time spans where first one is greater than or equal to second one.")]
        public void Compare_GreaterOrEqual()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Less.Concat(TimeSpansForComparison_Equal))
            {
                var timeSpan1 = timeSpansPair.Item2;
                var timeSpan2 = timeSpansPair.Item1;

                Assert.IsTrue(timeSpan1 >= timeSpan2,
                              $"{timeSpan1} isn't greater than or equal to {timeSpan2} using >=.");
                Assert.IsTrue(timeSpan1.CompareTo(timeSpan2) >= 0,
                              $"{timeSpan1} isn't greater than or equal to {timeSpan2} using typed CompareTo.");
                Assert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) >= 0,
                              $"{timeSpan1} isn't greater than or equal to {timeSpan2} using CompareTo(object).");
            }
        }

        [Test]
        [Description("Compare two time spans using CompareTo where second time span is of different type.")]
        public void Compare_TypesMismatch()
        {
            var timeSpansPairs = new[]
            {
                Tuple.Create<BarBeatCentsTimeSpan, ITimeSpan>(new BarBeatCentsTimeSpan(), new MidiTimeSpan(100)),
                Tuple.Create<BarBeatCentsTimeSpan, ITimeSpan>(new BarBeatCentsTimeSpan(), new MusicalTimeSpan(1, 1000)),
                Tuple.Create<BarBeatCentsTimeSpan, ITimeSpan>(new BarBeatCentsTimeSpan(), new MetricTimeSpan(1, 2, 3))
            };

            foreach (var timeSpansPair in timeSpansPairs)
            {
                var timeSpan1 = timeSpansPair.Item1;
                var timeSpan2 = timeSpansPair.Item2;

                Assert.Throws<ArgumentException>(() => timeSpan1.CompareTo(timeSpan2));
            }
        }

        [Test]
        [Description("Compare two time spans for equality: true expected.")]
        public void Compare_Equal_True()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Equal)
            {
                var timeSpan1 = timeSpansPair.Item2;
                var timeSpan2 = timeSpansPair.Item1;

                Assert.IsTrue(timeSpan1 == timeSpan2,
                              $"{timeSpan1} isn't equal to {timeSpan2} using ==.");
                Assert.IsTrue(timeSpan1.Equals(timeSpan2),
                              $"{timeSpan1} isn't equal to {timeSpan2} using typed Equals.");
                Assert.IsTrue(timeSpan1.Equals((object)timeSpan2),
                              $"{timeSpan1} isn't equal to {timeSpan2} using Equals(object).");
            }
        }

        [Test]
        [Description("Compare two time spans for equality: false expected.")]
        public void Compare_Equal_False()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Less)
            {
                var timeSpan1 = timeSpansPair.Item2;
                var timeSpan2 = timeSpansPair.Item1;

                Assert.IsFalse(timeSpan1 == timeSpan2,
                               $"{timeSpan1} equal to {timeSpan2} using ==.");
                Assert.IsFalse(timeSpan1.Equals(timeSpan2),
                               $"{timeSpan1} equal to {timeSpan2} using typed Equals.");
                Assert.IsFalse(timeSpan1.Equals((object)timeSpan2),
                               $"{timeSpan1} equal to {timeSpan2} using Equals(object).");
            }
        }

        [Test]
        [Description("Compare two time spans for inequality: true expected.")]
        public void Compare_DoesNotEqual_True()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Less)
            {
                var timeSpan1 = timeSpansPair.Item2;
                var timeSpan2 = timeSpansPair.Item1;

                Assert.IsTrue(timeSpan1 != timeSpan2,
                              $"{timeSpan1} equal to {timeSpan2} using !=.");
                Assert.IsTrue(!timeSpan1.Equals(timeSpan2),
                              $"{timeSpan1} equal to {timeSpan2} using typed Equals.");
                Assert.IsTrue(!timeSpan1.Equals((object)timeSpan2),
                              $"{timeSpan1} equal to {timeSpan2} using Equals(object).");
            }
        }

        [Test]
        [Description("Compare two time spans for inequality: false expected.")]
        public void Compare_DoesNotEqual_False()
        {
            foreach (var timeSpansPair in TimeSpansForComparison_Equal)
            {
                var timeSpan1 = timeSpansPair.Item2;
                var timeSpan2 = timeSpansPair.Item1;

                Assert.IsFalse(timeSpan1 != timeSpan2,
                               $"{timeSpan1} isn't equal to {timeSpan2} using !=.");
                Assert.IsFalse(!timeSpan1.Equals(timeSpan2),
                               $"{timeSpan1} isn't equal to {timeSpan2} using typed Equals.");
                Assert.IsFalse(!timeSpan1.Equals((object)timeSpan2),
                               $"{timeSpan1} isn't equal to {timeSpan2} using Equals(object).");
            }
        }

        #endregion

        #endregion
    }
}
