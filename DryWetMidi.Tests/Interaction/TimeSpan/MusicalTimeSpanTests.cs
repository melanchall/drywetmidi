using System;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class MusicalTimeSpanTests
    {
        #region Constants

        private const long ZeroTime = 0;
        private const long ShortTime = 1000;
        private const long LargeTime = 100000;

        private static readonly MetricTimeSpan MetricSpan = new MetricTimeSpan(0, 2, 30);

        private static readonly ITimeSpan ZeroTimeSpan = new MidiTimeSpan();
        private static readonly ITimeSpan ShortTimeSpan = new MidiTimeSpan(100);
        private static readonly ITimeSpan LargeTimeSpan = new MetricTimeSpan(0, 2, 30);

        private static readonly MusicalTimeSpan ZeroSpan = new MusicalTimeSpan();
        private static readonly MusicalTimeSpan ShortSpan = MusicalTimeSpan.Quarter;
        private static readonly MusicalTimeSpan LongSpan = 80 * MusicalTimeSpan.Whole + MusicalTimeSpan.Eighth.DoubleDotted();

        private static readonly Tuple<MusicalTimeSpan, MusicalTimeSpan>[] TimeSpansForComparison_Less = new[]
        {
            Tuple.Create(new MusicalTimeSpan(), new MusicalTimeSpan(1, 1)),
            Tuple.Create(new MusicalTimeSpan(), new MusicalTimeSpan(1, 10)),
            Tuple.Create(new MusicalTimeSpan(), new MusicalTimeSpan(10, 10)),
            Tuple.Create(new MusicalTimeSpan(), MusicalTimeSpan.Half.SingleDotted().Triplet()),
            Tuple.Create(new MusicalTimeSpan(2, 1), new MusicalTimeSpan(10, 2)),
            Tuple.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Whole),
            Tuple.Create(new MusicalTimeSpan(10), new MusicalTimeSpan(2)),
            Tuple.Create(new MusicalTimeSpan(1, 100), new MusicalTimeSpan(1, 5))
        };

        private static readonly Tuple<MusicalTimeSpan, MusicalTimeSpan>[] TimeSpansForComparison_Equal = new[]
        {
            Tuple.Create(new MusicalTimeSpan(), new MusicalTimeSpan()),
            Tuple.Create(new MusicalTimeSpan(), new MusicalTimeSpan(0, 1)),
            Tuple.Create(new MusicalTimeSpan(10, 10), new MusicalTimeSpan(10, 10)),
            Tuple.Create(new MusicalTimeSpan(100, 12345), new MusicalTimeSpan(100, 12345)),
            Tuple.Create(new MusicalTimeSpan(1, 5), new MusicalTimeSpan(2, 10)),
            Tuple.Create(MusicalTimeSpan.Half, MusicalTimeSpan.Half),
            Tuple.Create(MusicalTimeSpan.Eighth.Dotted(5).Tuplet(10, 4), MusicalTimeSpan.Eighth.Dotted(5).Tuplet(10, 4))
        };

        private static readonly object[] DoublesToTimeSpans_Known = new[]
        {
            new object[] { 0.00000001, new MusicalTimeSpan(0, 1) },
            new object[] { 1.0, new MusicalTimeSpan(1, 1) },
            new object[] { 1.0 / 2, new MusicalTimeSpan(1, 2) },
            new object[] { 1.0 / 4, new MusicalTimeSpan(1, 4) },
            new object[] { 1.0 / 8, new MusicalTimeSpan(1, 8) },
            new object[] { 1.0 / 16, new MusicalTimeSpan(1, 16) },
            new object[] { 1.0 / 32, new MusicalTimeSpan(1, 32) },
            new object[] { 1.0 / 64, new MusicalTimeSpan(1, 64) },
            new object[] { 1.0 / 2 + 1.0 / 8, new MusicalTimeSpan(5, 8) },
            new object[] { 1.0 / 2 + 1.0 / 8 + 1.0 / 16, new MusicalTimeSpan(11, 16) },
            new object[] { 1.0 / 2 + 1.0 / 8 + 1.0 / 16 + 1.0 / 32 + 1.0 / 64, new MusicalTimeSpan(47, 64) },
            new object[] { 1.0 / 128, new MusicalTimeSpan(1, 128) },
            new object[] { 1.0 / 256, new MusicalTimeSpan(1, 256) },
            new object[] { 1.0 / 512, new MusicalTimeSpan(1, 512) },
            new object[] { 1.0 / 1024, new MusicalTimeSpan(1, 1024) },
        };

        private static readonly object[] DoublesToTimeSpansWithEpsilon_Known = new[]
        {
            new object[] { 0.00000001, 5, new MusicalTimeSpan(0, 1) },
            new object[] { 1.0, 0.2, new MusicalTimeSpan(1, 1) },
            new object[] { 1.0 / 2, 0.5, new MusicalTimeSpan(1, 2) },
            new object[] { 1.0 / 2, 1, new MusicalTimeSpan(1, 1) },
            new object[] { 1.0 / 4, 0.5, new MusicalTimeSpan(0, 1) },
            new object[] { 1.0 / 4, 0.25, new MusicalTimeSpan(1, 4) },
            new object[] { 1.0 / 4, 0.05, new MusicalTimeSpan(1, 4) },
            new object[] { 1.0 / 4, 1, new MusicalTimeSpan(0, 1) },
        };

        #endregion

        #region Test methods

        #region Convert

        #region Default

        [Test]
        public void Convert_Default_1()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_2()
        {
            var timeSpan = ShortSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_3()
        {
            var timeSpan = LongSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_4()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_5()
        {
            var timeSpan = ShortSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_6()
        {
            var timeSpan = LongSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_7()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_8()
        {
            var timeSpan = ShortSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_9()
        {
            var timeSpan = LongSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Convert_Default_FromMaxTicks()
        {
            ClassicAssert.DoesNotThrow(() =>
                TimeConverter.ConvertTo<MusicalTimeSpan>(long.MaxValue, TimeSpanTestUtilities.DefaultTempoMap));
        }

        [Test]
        public void Convert_Default_FromMaxTimeSpan()
        {
            ClassicAssert.DoesNotThrow(() =>
                TimeConverter.ConvertFrom(new MusicalTimeSpan(long.MaxValue, long.MaxValue), TimeSpanTestUtilities.DefaultTempoMap));
        }

        #endregion

        #region Simple

        [Test]
        public void Convert_Simple_1()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_3()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_4()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_5()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_6()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_7()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_8()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Convert_Simple_9()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        #endregion

        #region Complex

        [Test]
        public void Convert_Complex_1()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_3()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_4()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_5()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_6()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_7()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_8()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Convert_Complex_9()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        #endregion

        #endregion

        #region Parse

        [Test]
        [Description("Parse zero musical time span.")]
        public void Parse_1()
        {
            TimeSpanTestUtilities.Parse("0/1", new MusicalTimeSpan(0, 1));
        }

        [Test]
        [Description("Parse quarter musical time span.")]
        public void Parse_2()
        {
            TimeSpanTestUtilities.Parse("q", MusicalTimeSpan.Quarter);
        }

        [Test]
        [Description("Parse single dotted quarter musical time span.")]
        public void Parse_3()
        {
            TimeSpanTestUtilities.Parse("1/4.", MusicalTimeSpan.Quarter.SingleDotted());
        }

        [Test]
        [Description("Parse double dotted eight musical time span.")]
        public void Parse_4()
        {
            TimeSpanTestUtilities.Parse("/8..", MusicalTimeSpan.Eighth.DoubleDotted());
        }

        [Test]
        [Description("Parse single dotted triplet whole musical time span.")]
        public void Parse_5()
        {
            TimeSpanTestUtilities.Parse("wt.", MusicalTimeSpan.Whole.Triplet().SingleDotted());
        }

        [Test]
        [Description("Parse tuplet whole musical time span where tuplet is 3 notes in space of 10 ones.")]
        public void Parse_6()
        {
            TimeSpanTestUtilities.Parse("w[3:10]", MusicalTimeSpan.Whole.Tuplet(3, 10));
        }

        [Test]
        [Description("Parse triple dotted tuplet sixteenth musical time span where tuplet is 3 notes in space of 10 ones.")]
        public void Parse_7()
        {
            TimeSpanTestUtilities.Parse("s[3:10]...", MusicalTimeSpan.Sixteenth.Tuplet(3, 10).Dotted(3));
        }

        #endregion

        #region Add

        [Test]
        public void Add_SameType_1()
        {
            TimeSpanTestUtilities.Add_SameType(new MusicalTimeSpan(),
                                               new MusicalTimeSpan(),
                                               new MusicalTimeSpan());
        }

        [Test]
        public void Add_SameType_2()
        {
            TimeSpanTestUtilities.Add_SameType(new MusicalTimeSpan(5, 7),
                                               new MusicalTimeSpan(),
                                               new MusicalTimeSpan(5, 7));
        }

        [Test]
        public void Add_SameType_3()
        {
            TimeSpanTestUtilities.Add_SameType(MusicalTimeSpan.Quarter,
                                               MusicalTimeSpan.Eighth,
                                               MusicalTimeSpan.Quarter.SingleDotted());
        }

        [Test]
        public void Add_TimeTime_1()
        {
            TimeSpanTestUtilities.Add_TimeTime(ShortSpan,
                                               MetricSpan);
        }

        [Test]
        public void Add_TimeTime_2()
        {
            TimeSpanTestUtilities.Add_TimeTime(LongSpan,
                                               MetricSpan);
        }

        [Test]
        public void Add_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Add_TimeLength_Default_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(LongSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Add_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Add_TimeLength_Simple_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(LongSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Add_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Add_TimeLength_Complex_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(LongSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Add_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Default_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Default_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Default_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Simple_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Simple_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Simple_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   LargeTime);
        }

        [Test]
        public void Add_LengthLength_Complex_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ZeroTime);
        }

        [Test]
        public void Add_LengthLength_Complex_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ShortTime);
        }

        [Test]
        public void Add_LengthLength_Complex_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   LargeTime);
        }

        #endregion

        #region Subtract

        [Test]
        public void Subtract_SameType_1()
        {
            TimeSpanTestUtilities.Subtract_SameType(new MusicalTimeSpan(),
                                                    new MusicalTimeSpan(),
                                                    new MusicalTimeSpan());
        }

        [Test]
        public void Subtract_SameType_2()
        {
            TimeSpanTestUtilities.Subtract_SameType(new MusicalTimeSpan(11, 27),
                                                    new MusicalTimeSpan(),
                                                    new MusicalTimeSpan(11, 27));
        }

        [Test]
        public void Subtract_SameType_3()
        {
            TimeSpanTestUtilities.Subtract_SameType(MusicalTimeSpan.Whole,
                                                    MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
                                                    MusicalTimeSpan.Quarter.SingleDotted());
        }

        [Test]
        public void Subtract_TimeTime_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    ShortSpan,
                                                    TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Subtract_TimeTime_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    ShortSpan,
                                                    TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Subtract_TimeTime_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    ShortSpan,
                                                    TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      ShortSpan,
                                                      TimeSpanTestUtilities.DefaultTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      ShortSpan,
                                                      TimeSpanTestUtilities.SimpleTempoMap);
        }

        [Test]
        public void Subtract_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      ShortSpan,
                                                      TimeSpanTestUtilities.ComplexTempoMap);
        }

        [Test]
        public void Subtract_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        LargeTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        LargeTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ZeroTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ShortTime);
        }

        [Test]
        public void Subtract_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        LargeTime);
        }

        #endregion

        #region Multiply

        [Test]
        [Description("Multiply zero time span by zero.")]
        public void Multiply_1()
        {
            ClassicAssert.AreEqual(new MusicalTimeSpan(),
                            new MusicalTimeSpan().Multiply(0));
        }

        [Test]
        [Description("Multiply arbitrary time span by zero.")]
        public void Multiply_2()
        {
            ClassicAssert.AreEqual(new MusicalTimeSpan(),
                            new MusicalTimeSpan(3, 7).Multiply(0));
        }

        [Test]
        [Description("Multiply by integer number.")]
        public void Multiply_3()
        {
            ClassicAssert.AreEqual(MusicalTimeSpan.Whole,
                            MusicalTimeSpan.Half.Multiply(2));
        }

        [Test]
        [Description("Multiply by non-integer number.")]
        public void Multiply_4()
        {
            ClassicAssert.AreEqual(MusicalTimeSpan.Quarter.SingleDotted(),
                            MusicalTimeSpan.Quarter.Multiply(1.5));
        }

        [Test]
        [Description("Multiply by negative number.")]
        public void Multiply_5()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new MusicalTimeSpan().Multiply(-5));
        }

        #endregion

        #region Divide

        [Test]
        [Description("Divide arbitrary time span by one.")]
        public void Divide_1()
        {
            ClassicAssert.AreEqual(new MusicalTimeSpan(1, 5),
                            new MusicalTimeSpan(1, 5).Divide(1));
        }

        [Test]
        [Description("Divide arbitrary time span by integer number.")]
        public void Divide_2()
        {
            ClassicAssert.AreEqual(MusicalTimeSpan.Half,
                            MusicalTimeSpan.Whole.Divide(2));
        }

        [Test]
        [Description("Divide by non-integer number.")]
        public void Divide_3()
        {
            ClassicAssert.AreEqual(new MusicalTimeSpan(1, 12),
                            MusicalTimeSpan.Eighth.Divide(1.5));
        }

        [Test]
        [Description("Divide by zero.")]
        public void Divide_4()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new MusicalTimeSpan().Divide(0));
        }

        [Test]
        [Description("Divide by negative number.")]
        public void Divide_5()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() => new MusicalTimeSpan().Divide(-8));
        }

        [Test]
        [Description("Divide zero time span by one.")]
        public void Divide_6()
        {
            ClassicAssert.AreEqual(new MusicalTimeSpan(),
                            new MusicalTimeSpan().Divide(1));
        }

        #endregion

        #region Clone

        [Test]
        public void Clone_1()
        {
            TimeSpanTestUtilities.TestClone(new MusicalTimeSpan());
        }

        [Test]
        public void Clone_2()
        {
            TimeSpanTestUtilities.TestClone(new MusicalTimeSpan(5, 8));
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

                ClassicAssert.IsTrue(timeSpan1 < timeSpan2,
                              $"{timeSpan1} isn't less than {timeSpan2} using <.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo(timeSpan2) < 0,
                              $"{timeSpan1} isn't less than {timeSpan2} using typed CompareTo.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) < 0,
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

                ClassicAssert.IsTrue(timeSpan1 > timeSpan2,
                              $"{timeSpan1} isn't greater than {timeSpan2} using >.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo(timeSpan2) > 0,
                              $"{timeSpan1} isn't greater than {timeSpan2} using typed CompareTo.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) > 0,
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

                ClassicAssert.IsTrue(timeSpan1 <= timeSpan2,
                              $"{timeSpan1} isn't less than or equal to {timeSpan2} using <=.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo(timeSpan2) <= 0,
                              $"{timeSpan1} isn't less than or equal to {timeSpan2} using typed CompareTo.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) <= 0,
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

                ClassicAssert.IsTrue(timeSpan1 >= timeSpan2,
                              $"{timeSpan1} isn't greater than or equal to {timeSpan2} using >=.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo(timeSpan2) >= 0,
                              $"{timeSpan1} isn't greater than {timeSpan2} using typed CompareTo.");
                ClassicAssert.IsTrue(timeSpan1.CompareTo((object)timeSpan2) >= 0,
                              $"{timeSpan1} isn't greater than {timeSpan2} using CompareTo(object).");
            }
        }

        [Test]
        [Description("Compare two time spans using CompareTo where second time span is of different type.")]
        public void Compare_TypesMismatch()
        {
            var timeSpansPairs = new[]
            {
                Tuple.Create<MusicalTimeSpan, ITimeSpan>(new MusicalTimeSpan(), new MetricTimeSpan(100)),
                Tuple.Create<MusicalTimeSpan, ITimeSpan>(new MusicalTimeSpan(), new MidiTimeSpan(1)),
                Tuple.Create<MusicalTimeSpan, ITimeSpan>(new MusicalTimeSpan(), new BarBeatTicksTimeSpan(1, 2, 3))
            };

            foreach (var timeSpansPair in timeSpansPairs)
            {
                var timeSpan1 = timeSpansPair.Item1;
                var timeSpan2 = timeSpansPair.Item2;

                ClassicAssert.Throws<ArgumentException>(() => timeSpan1.CompareTo(timeSpan2));
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

                ClassicAssert.IsTrue(timeSpan1 == timeSpan2,
                              $"{timeSpan1} isn't equal to {timeSpan2} using ==.");
                ClassicAssert.IsTrue(timeSpan1.Equals(timeSpan2),
                              $"{timeSpan1} isn't equal to {timeSpan2} using typed Equals.");
                ClassicAssert.IsTrue(timeSpan1.Equals((object)timeSpan2),
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

                ClassicAssert.IsFalse(timeSpan1 == timeSpan2,
                               $"{timeSpan1} equal to {timeSpan2} using ==.");
                ClassicAssert.IsFalse(timeSpan1.Equals(timeSpan2),
                               $"{timeSpan1} equal to {timeSpan2} using typed Equals.");
                ClassicAssert.IsFalse(timeSpan1.Equals((object)timeSpan2),
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

                ClassicAssert.IsTrue(timeSpan1 != timeSpan2,
                              $"{timeSpan1} equal to {timeSpan2} using !=.");
                ClassicAssert.IsTrue(!timeSpan1.Equals(timeSpan2),
                              $"{timeSpan1} equal to {timeSpan2} using typed Equals.");
                ClassicAssert.IsTrue(!timeSpan1.Equals((object)timeSpan2),
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

                ClassicAssert.IsFalse(timeSpan1 != timeSpan2,
                               $"{timeSpan1} isn't equal to {timeSpan2} using !=.");
                ClassicAssert.IsFalse(!timeSpan1.Equals(timeSpan2),
                               $"{timeSpan1} isn't equal to {timeSpan2} using typed Equals.");
                ClassicAssert.IsFalse(!timeSpan1.Equals((object)timeSpan2),
                               $"{timeSpan1} isn't equal to {timeSpan2} using Equals(object).");
            }
        }

        #endregion

        #region Divide

        [Test]
        [Description("Divide musical time span by another one.")]
        public void Divide()
        {
            ClassicAssert.AreEqual(1, MusicalTimeSpan.Half.Divide(MusicalTimeSpan.Half));
            ClassicAssert.AreEqual(1.5, MusicalTimeSpan.Half.SingleDotted().Divide(MusicalTimeSpan.Half));
            ClassicAssert.AreEqual(0.5, MusicalTimeSpan.Half.Divide(MusicalTimeSpan.Whole));

            ClassicAssert.Throws<DivideByZeroException>(() => new MusicalTimeSpan().Divide(new MusicalTimeSpan()));
        }

        #endregion

        #region ChangeDenominator

        [Test]
        public void ChangeDenominator()
        {
            CheckChangeDenominator(new MusicalTimeSpan(5), 16, 3);
            CheckChangeDenominator(new MusicalTimeSpan(1, 2), 4, 2);
            CheckChangeDenominator(MusicalTimeSpan.Eighth, 16, 2);
            CheckChangeDenominator(MusicalTimeSpan.Whole.SingleDotted(), 5, 8);
        }

        #endregion

        #region FromDouble

        [TestCaseSource(nameof(DoublesToTimeSpans_Known))]
        public void FromDouble(double number, MusicalTimeSpan expectedTimeSpan)
        {
            for (var numeratorMultiplier = 1; numeratorMultiplier <= 10; numeratorMultiplier++)
            {
                var actualTimeSpan = MusicalTimeSpan.FromDouble(number * numeratorMultiplier);
                ClassicAssert.AreEqual(expectedTimeSpan.Multiply(numeratorMultiplier), actualTimeSpan, $"Invalid time span for multiplier {numeratorMultiplier}.");
            }
        }

        [TestCaseSource(nameof(DoublesToTimeSpansWithEpsilon_Known))]
        public void FromDouble_Epsilon(double number, double epsilon, MusicalTimeSpan expectedTimeSpan)
        {
            var actualTimeSpan = MusicalTimeSpan.FromDouble(number, epsilon);
            ClassicAssert.AreEqual(expectedTimeSpan, actualTimeSpan, $"Invalid time span.");
        }

        [Test]
        public void FromDouble_Unknown([Values(0.12345, 17.0/79, 2.5/0.567)] double number)
        {
            ClassicAssert.Throws<DoubleToMusicalTimeSpanParsingException>(
                () => MusicalTimeSpan.FromDouble(number),
                "No exception thrown.");
        }

        #endregion

        #endregion

        #region Private methods

        private static void CheckChangeDenominator(MusicalTimeSpan timeSpan, long denominator, long numerator)
        {
            CheckTimeSpan(timeSpan.ChangeDenominator(denominator), numerator, denominator);
        }

        private static void CheckTimeSpan(MusicalTimeSpan timeSpan, long numerator, long denominator)
        {
            ClassicAssert.AreEqual(numerator, timeSpan.Numerator, "Numerator is invalid.");
            ClassicAssert.AreEqual(denominator, timeSpan.Denominator, "Denominator is invalid.");
        }

        private static MidiTimeSpan GetDefaultMidiTimeSpan(MusicalTimeSpan timeSpan)
        {
            return new MidiTimeSpan(4 * TimeSpanTestUtilities.TicksPerQuarterNote * timeSpan.Numerator / timeSpan.Denominator);
        }

        #endregion
    }
}
