using System;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class BarBeatFractionTimeSpanTests
    {
        #region Constants

        private static readonly ITimeSpan MetricSpan = new MetricTimeSpan(0, 2, 30);
        private static readonly ITimeSpan BarBeatFractionSpan = new BarBeatFractionTimeSpan(1, 2.05);

        private const long ZeroTime = 0;
        private const long ShortTime = 1000;
        private const long LargeTime = 100000;

        private static object[] TimeSpansForComparison_Less = new[]
        {
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan(0, 0.01) },
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan(0, 1.00) },
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan(1, 0.00) },
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan(0, 0.01) },
            new object[] { new BarBeatFractionTimeSpan(2, 0.00), new BarBeatFractionTimeSpan(10, 0.01) },
            new object[] { new BarBeatFractionTimeSpan(0, 10.00), new BarBeatFractionTimeSpan(0, 10.01) },
            new object[] { new BarBeatFractionTimeSpan(10, 10.00), new BarBeatFractionTimeSpan(10, 10.01) },
            new object[] { new BarBeatFractionTimeSpan(10000, 899.00), new BarBeatFractionTimeSpan(10000, 10000.00) },
            new object[] { new BarBeatFractionTimeSpan(0, 100.00), new BarBeatFractionTimeSpan(0, 110.01) },
            new object[] { new BarBeatFractionTimeSpan(199, 0.10), new BarBeatFractionTimeSpan(200, 0.80) },
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan(10, 110.91) }
        };

        private static object[] TimeSpansForComparison_Equal = new[]
        {
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan() },
            new object[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan(0, 0.00) },
            new object[] { new BarBeatFractionTimeSpan(10, 0.00), new BarBeatFractionTimeSpan(10, 0.00) },
            new object[] { new BarBeatFractionTimeSpan(100, 100.10), new BarBeatFractionTimeSpan(100, 100.10) },
            new object[] { new BarBeatFractionTimeSpan(0, 345.00), new BarBeatFractionTimeSpan(0, 345.0) },
            new object[] { new BarBeatFractionTimeSpan(0, 0.345), new BarBeatFractionTimeSpan(0, 0.345) }
        };

        private static object[] TimeSpansForComparison_LessOrEqual =
            TimeSpansForComparison_Less.Concat(TimeSpansForComparison_Equal).ToArray();

        private static object[] StringsToTimeSpans = new[]
        {
            new object[] { "0_0", new BarBeatFractionTimeSpan() },
            new object[] { "0,0_0.0", new BarBeatFractionTimeSpan() },
            new object[] { "0_0.0", new BarBeatFractionTimeSpan() },
            new object[] { "10_0", new BarBeatFractionTimeSpan(10, 0.00) },
            new object[] { "10,8_0", new BarBeatFractionTimeSpan(10.8, 0.00) },
            new object[] { "100_100.10", new BarBeatFractionTimeSpan(100, 100.10) },
            new object[] { "100_100,10", new BarBeatFractionTimeSpan(100, 100.10) },
            new object[] { "0_345.00", new BarBeatFractionTimeSpan(0, 345.00) },
            new object[] { "0_345,00", new BarBeatFractionTimeSpan(0, 345.00) },
            new object[] { "10_45.00", new BarBeatFractionTimeSpan(10, 45.00) },
            new object[] { "10_45,00", new BarBeatFractionTimeSpan(10, 45.00) },
            new object[] { "2_45", new BarBeatFractionTimeSpan(2, 45) }
        };

        private static object[] TimeSpansToStrings = new[]
        {
            new object[] { new BarBeatFractionTimeSpan(), "0_0" },
            new object[] { new BarBeatFractionTimeSpan(10, 0.00), "10_0" },
            new object[] { new BarBeatFractionTimeSpan(10.8, 0.00), "10,8_0" },
            new object[] { new BarBeatFractionTimeSpan(100, 100.10), "100_100,1" },
            new object[] { new BarBeatFractionTimeSpan(0, 345.00), "0_345" },
            new object[] { new BarBeatFractionTimeSpan(10, 45.00), "10_45" },
            new object[] { new BarBeatFractionTimeSpan(2, 45), "2_45" }
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(2, 0),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(2, 0),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 2.125),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 1),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 1.1875),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 2),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 2),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 1),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 1),
                                                 MusicalTimeSpan.Quarter,
                                                 MusicalTimeSpan.Whole + MusicalTimeSpan.Half.SingleDotted() + MusicalTimeSpan.ThirtySecond,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_FromMaxTicks()
        {
            ClassicAssert.DoesNotThrow(() =>
                TimeConverter.ConvertTo<BarBeatFractionTimeSpan>(long.MaxValue, TimeSpanTestUtilities.DefaultTempoMap));
        }

        [Test]
        public void Convert_Default_FromMaxTimeSpan()
        {
            ClassicAssert.Throws<InvalidOperationException>(() =>
                TimeConverter.ConvertFrom(new BarBeatFractionTimeSpan(long.MaxValue, double.MaxValue), TimeSpanTestUtilities.DefaultTempoMap));
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(2, 0),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 3),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 2),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 2),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 1),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(2, 1),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 2),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 2),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 1.6875),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 2.25),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 0.1875),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 0.375),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 4),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 3.375),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(0, 4.9375),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(4, 0),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(4, 3),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(5, 1),
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

            TimeSpanTestUtilities.TestConversion(new BarBeatFractionTimeSpan(1, 0),
                                                 3 * MusicalTimeSpan.Sixteenth + 2 * MusicalTimeSpan.Eighth,
                                                 2 * MusicalTimeSpan.Whole + 5 * MusicalTimeSpan.Eighth + 12 * MusicalTimeSpan.Sixteenth,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        #endregion

        #endregion

        #region Parse

        [TestCaseSource(nameof(StringsToTimeSpans))]
        public void Parse(string s, BarBeatFractionTimeSpan expectedTimeSpan)
        {
            TimeSpanTestUtilities.Parse(s, expectedTimeSpan);
        }

        [TestCaseSource(nameof(TimeSpansToStrings))]
        public void ToString(BarBeatFractionTimeSpan timeSpan, string expectedString)
        {
            ClassicAssert.AreEqual(expectedString, timeSpan.ToString(), "Invalid string representation.");
        }

        #endregion

        #region Add

        [Test]
        public void Add_SameType_1()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatFractionTimeSpan(),
                                               new BarBeatFractionTimeSpan(),
                                               new BarBeatFractionTimeSpan());
        }

        [Test]
        public void Add_SameType_2()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatFractionTimeSpan(10, 0.10),
                                               new BarBeatFractionTimeSpan(),
                                               new BarBeatFractionTimeSpan(10, 0.10));
        }

        [Test]
        public void Add_SameType_3()
        {
            TimeSpanTestUtilities.Add_SameType(new BarBeatFractionTimeSpan(10, 0.10),
                                               new BarBeatFractionTimeSpan(0, 3.05),
                                               new BarBeatFractionTimeSpan(10, 3.15));
        }

        [Test]
        public void Add_TimeTime_1()
        {
            TimeSpanTestUtilities.Add_TimeTime(BarBeatFractionSpan,
                                               MetricSpan);
        }

        [Test]
        public void Add_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(BarBeatFractionSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Add_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(BarBeatFractionSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Add_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(BarBeatFractionSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Add_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(BarBeatFractionSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   LargeTime);
        }

        #endregion

        #region Subtract

        [Test]
        public void Subtract_SameType_1()
        {
            TimeSpanTestUtilities.Subtract_SameType(new BarBeatFractionTimeSpan(),
                                                    new BarBeatFractionTimeSpan(),
                                                    new BarBeatFractionTimeSpan());
        }

        [Test]
        public void Subtract_SameType_2()
        {
            TimeSpanTestUtilities.Subtract_SameType(new BarBeatFractionTimeSpan(10, 0.10),
                                                    new BarBeatFractionTimeSpan(),
                                                    new BarBeatFractionTimeSpan(10, 0.10));
        }

        [Test]
        public void Subtract_SameType_3()
        {
            TimeSpanTestUtilities.Subtract_SameType(new BarBeatFractionTimeSpan(10, 7.10),
                                                    new BarBeatFractionTimeSpan(0, 3.06),
                                                    new BarBeatFractionTimeSpan(10, 4.04));
        }

        [Test]
        public void Subtract_TimeTime_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    BarBeatFractionSpan,
                                                    TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Subtract_TimeTime_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    BarBeatFractionSpan,
                                                    TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Subtract_TimeTime_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    BarBeatFractionSpan,
                                                    TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      BarBeatFractionSpan,
                                                      TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      BarBeatFractionSpan,
                                                      TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      BarBeatFractionSpan,
                                                      TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Subtract_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        LargeTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        LargeTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        BarBeatFractionSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        LargeTime);
        }

        #endregion

        #region Multiply

        [Test]
        [Description("Multiply zero time span by zero.")]
        public void Multiply_1()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(),
                            new BarBeatFractionTimeSpan().Multiply(0));
        }

        [Test]
        [Description("Multiply arbitrary time span by zero.")]
        public void Multiply_2()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(),
                            new BarBeatFractionTimeSpan(10, 5.09).Multiply(0));
        }

        [Test]
        [Description("Multiply by integer number.")]
        public void Multiply_3()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(20, 10.18),
                            new BarBeatFractionTimeSpan(10, 5.09).Multiply(2));
        }

        [Test]
        [Description("Multiply by non-integer number.")]
        public void Multiply_4()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(15, 12.09),
                            new BarBeatFractionTimeSpan(10, 8.06).Multiply(1.5));
        }

        [Test]
        [Description("Multiply by negative number.")]
        public void Multiply_5()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new BarBeatFractionTimeSpan().Multiply(-5));
        }

        #endregion

        #region Divide

        [Test]
        [Description("Divide arbitrary time span by one.")]
        public void Divide_1()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(5, 4.03),
                            new BarBeatFractionTimeSpan(5, 4.03).Divide(1));
        }

        [Test]
        [Description("Divide arbitrary time span by integer number.")]
        public void Divide_2()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(5, 3.04),
                            new BarBeatFractionTimeSpan(10, 6.08).Divide(2));
        }

        [Test]
        [Description("Divide by non-integer number.")]
        public void Divide_3()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(8, 2.04),
                            new BarBeatFractionTimeSpan(12, 3.06).Divide(1.5));
        }

        [Test]
        [Description("Divide by zero.")]
        public void Divide_4()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new BarBeatFractionTimeSpan().Divide(0));
        }

        [Test]
        [Description("Divide by negative number.")]
        public void Divide_5()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new BarBeatFractionTimeSpan().Divide(-8));
        }

        [Test]
        [Description("Divide zero time span by one.")]
        public void Divide_6()
        {
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(),
                            new BarBeatFractionTimeSpan().Divide(1));
        }

        #endregion

        #region Clone

        [Test]
        public void Clone_1()
        {
            TimeSpanTestUtilities.TestClone(new BarBeatFractionTimeSpan());
        }

        [Test]
        public void Clone_2()
        {
            TimeSpanTestUtilities.TestClone(new BarBeatFractionTimeSpan(1, 2.03));
        }

        #endregion

        #region Compare

        [TestCaseSource(nameof(TimeSpansForComparison_Less))]
        public void Compare_Less(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ClassicAssert.IsTrue(
                timeSpan1 < timeSpan2,
                $"{timeSpan1} isn't less than {timeSpan2} using <.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo(timeSpan2) < 0,
                $"{timeSpan1} isn't less than {timeSpan2} using typed CompareTo.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo((object)timeSpan2) < 0,
                $"{timeSpan1} isn't less than {timeSpan2} using CompareTo(object).");
        }

        [TestCaseSource(nameof(TimeSpansForComparison_Less))]
        public void Compare_Greater(BarBeatFractionTimeSpan timeSpan2, BarBeatFractionTimeSpan timeSpan1)
        {
            ClassicAssert.IsTrue(
                timeSpan1 > timeSpan2,
                $"{timeSpan1} isn't greater than {timeSpan2} using >.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo(timeSpan2) > 0,
                $"{timeSpan1} isn't greater than {timeSpan2} using typed CompareTo.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo((object)timeSpan2) > 0,
                $"{timeSpan1} isn't greater than {timeSpan2} using CompareTo(object).");
        }

        [TestCaseSource(nameof(TimeSpansForComparison_LessOrEqual))]
        public void Compare_LessOrEqual(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ClassicAssert.IsTrue(
                timeSpan1 <= timeSpan2,
                $"{timeSpan1} isn't less than or equal to {timeSpan2} using <=.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo(timeSpan2) <= 0,
                $"{timeSpan1} isn't less than or equal to {timeSpan2} using typed CompareTo.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo((object)timeSpan2) <= 0,
                $"{timeSpan1} isn't less than or equal to {timeSpan2} using CompareTo(object).");
        }

        [TestCaseSource(nameof(TimeSpansForComparison_LessOrEqual))]
        public void Compare_GreaterOrEqual(BarBeatFractionTimeSpan timeSpan2, BarBeatFractionTimeSpan timeSpan1)
        {
            ClassicAssert.IsTrue(
                timeSpan1 >= timeSpan2,
                $"{timeSpan1} isn't greater than or equal to {timeSpan2} using >=.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo(timeSpan2) >= 0,
                $"{timeSpan1} isn't greater than or equal to {timeSpan2} using typed CompareTo.");
            ClassicAssert.IsTrue(
                timeSpan1.CompareTo((object)timeSpan2) >= 0,
                $"{timeSpan1} isn't greater than or equal to {timeSpan2} using CompareTo(object).");
        }

        [Test]
        [Description("Compare two time spans using CompareTo where second time span is of different type.")]
        public void Compare_TypesMismatch()
        {
            var timeSpansPairs = new (BarBeatFractionTimeSpan, ITimeSpan)[]
            {
                (new BarBeatFractionTimeSpan(), new MidiTimeSpan(100)),
                (new BarBeatFractionTimeSpan(), new MusicalTimeSpan(1, 1000)),
                (new BarBeatFractionTimeSpan(), new MetricTimeSpan(1, 2, 3))
            };

            foreach (var (timeSpan1, timeSpan2) in timeSpansPairs)
            {
                ClassicAssert.Throws<ArgumentException>(() => timeSpan1.CompareTo(timeSpan2));
            }
        }

        [TestCaseSource(nameof(TimeSpansForComparison_Equal))]
        public void Compare_Equal_True(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ClassicAssert.IsTrue(
                timeSpan1 == timeSpan2,
                $"{timeSpan1} isn't equal to {timeSpan2} using ==.");
            ClassicAssert.IsTrue(
                timeSpan1.Equals(timeSpan2),
                $"{timeSpan1} isn't equal to {timeSpan2} using typed Equals.");
            ClassicAssert.IsTrue(
                timeSpan1.Equals((object)timeSpan2),
                $"{timeSpan1} isn't equal to {timeSpan2} using Equals(object).");
        }

        [TestCaseSource(nameof(TimeSpansForComparison_Less))]
        public void Compare_Equal_False(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ClassicAssert.IsFalse(
                timeSpan1 == timeSpan2,
                $"{timeSpan1} equal to {timeSpan2} using ==.");
            ClassicAssert.IsFalse(
                timeSpan1.Equals(timeSpan2),
                $"{timeSpan1} equal to {timeSpan2} using typed Equals.");
            ClassicAssert.IsFalse(
                timeSpan1.Equals((object)timeSpan2),
                $"{timeSpan1} equal to {timeSpan2} using Equals(object).");
        }

        [TestCaseSource(nameof(TimeSpansForComparison_Less))]
        public void Compare_DoesNotEqual_True(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ClassicAssert.IsTrue(
                timeSpan1 != timeSpan2,
                $"{timeSpan1} equal to {timeSpan2} using !=.");
            ClassicAssert.IsTrue(
                !timeSpan1.Equals(timeSpan2),
                $"{timeSpan1} equal to {timeSpan2} using typed Equals.");
            ClassicAssert.IsTrue(
                !timeSpan1.Equals((object)timeSpan2),
                $"{timeSpan1} equal to {timeSpan2} using Equals(object).");
        }

        [TestCaseSource(nameof(TimeSpansForComparison_Equal))]
        public void Compare_DoesNotEqual_False(BarBeatFractionTimeSpan timeSpan1, BarBeatFractionTimeSpan timeSpan2)
        {
            ClassicAssert.IsFalse(
                timeSpan1 != timeSpan2,
                $"{timeSpan1} isn't equal to {timeSpan2} using !=.");
            ClassicAssert.IsFalse(
                !timeSpan1.Equals(timeSpan2),
                $"{timeSpan1} isn't equal to {timeSpan2} using typed Equals.");
            ClassicAssert.IsFalse(
                !timeSpan1.Equals((object)timeSpan2),
                $"{timeSpan1} isn't equal to {timeSpan2} using Equals(object).");
        }

        #endregion

        #endregion
    }
}
