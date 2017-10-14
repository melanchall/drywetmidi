using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
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

        #endregion

        #region Test methods

        #region Convert

        #region Default

        [TestMethod]
        public void Convert_Default_1()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_2()
        {
            var timeSpan = ShortSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_3()
        {
            var timeSpan = LongSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_4()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_5()
        {
            var timeSpan = ShortSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_6()
        {
            var timeSpan = LongSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_7()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_8()
        {
            var timeSpan = ShortSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_9()
        {
            var timeSpan = LongSpan;
            TimeSpanTestUtilities.TestConversion(timeSpan,
                                                 GetDefaultMidiTimeSpan(timeSpan),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        #endregion

        #region Simple

        [TestMethod]
        public void Convert_Simple_1()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_3()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_4()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_5()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_6()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_7()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_8()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_9()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        #endregion

        #region Complex

        [TestMethod]
        public void Convert_Complex_1()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_3()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ZeroTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_4()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_5()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_6()
        {
            TimeSpanTestUtilities.TestConversion(LongSpan,
                                                 LongSpan,
                                                 ShortTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_7()
        {
            TimeSpanTestUtilities.TestConversion(ZeroSpan,
                                                 new MidiTimeSpan(),
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_8()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 LargeTimeSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
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

        [TestMethod]
        [Description("Parse zero musical time span.")]
        public void Parse_1()
        {
            TimeSpanTestUtilities.Parse("0/1", new MusicalTimeSpan(0, 1));
        }

        [TestMethod]
        [Description("Parse quarter musical time span.")]
        public void Parse_2()
        {
            TimeSpanTestUtilities.Parse("q", MusicalTimeSpan.Quarter);
        }

        [TestMethod]
        [Description("Parse single dotted quarter musical time span.")]
        public void Parse_3()
        {
            TimeSpanTestUtilities.Parse("1/4.", MusicalTimeSpan.Quarter.SingleDotted());
        }

        [TestMethod]
        [Description("Parse double dotted eight musical time span.")]
        public void Parse_4()
        {
            TimeSpanTestUtilities.Parse("/8..", MusicalTimeSpan.Eighth.DoubleDotted());
        }

        [TestMethod]
        [Description("Parse single dotted triplet whole musical time span.")]
        public void Parse_5()
        {
            TimeSpanTestUtilities.Parse("wt.", MusicalTimeSpan.Whole.Triplet().SingleDotted());
        }

        [TestMethod]
        [Description("Parse tuplet whole musical time span where tuplet is 3 notes in space of 10 ones.")]
        public void Parse_6()
        {
            TimeSpanTestUtilities.Parse("w[3:10]", MusicalTimeSpan.Whole.Tuplet(3, 10));
        }

        [TestMethod]
        [Description("Parse triple dotted tuplet sixteenth musical time span where tuplet is 3 notes in space of 10 ones.")]
        public void Parse_7()
        {
            TimeSpanTestUtilities.Parse("s[3:10]...", MusicalTimeSpan.Sixteenth.Tuplet(3, 10).Dotted(3));
        }

        #endregion

        #region Add

        [TestMethod]
        public void Add_SameType_1()
        {
            TimeSpanTestUtilities.Add_SameType(new MusicalTimeSpan(),
                                               new MusicalTimeSpan(),
                                               new MusicalTimeSpan());
        }

        [TestMethod]
        public void Add_SameType_2()
        {
            TimeSpanTestUtilities.Add_SameType(new MusicalTimeSpan(5, 7),
                                               new MusicalTimeSpan(),
                                               new MusicalTimeSpan(5, 7));
        }

        [TestMethod]
        public void Add_SameType_3()
        {
            TimeSpanTestUtilities.Add_SameType(MusicalTimeSpan.Quarter,
                                               MusicalTimeSpan.Eighth,
                                               MusicalTimeSpan.Quarter.SingleDotted());
        }

        [TestMethod]
        public void Add_TimeTime_1()
        {
            TimeSpanTestUtilities.Add_TimeTime(ShortSpan,
                                               MetricSpan);
        }

        [TestMethod]
        public void Add_TimeTime_2()
        {
            TimeSpanTestUtilities.Add_TimeTime(LongSpan,
                                               MetricSpan);
        }

        [TestMethod]
        public void Add_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_Default_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(LongSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_Simple_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(LongSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_TimeLength(ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Add_TimeLength_Complex_2()
        {
            TimeSpanTestUtilities.Add_TimeLength(LongSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Add_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ZeroTime);
        }

        [TestMethod]
        public void Add_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ShortTime);
        }

        [TestMethod]
        public void Add_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   LargeTime);
        }

        [TestMethod]
        public void Add_LengthLength_Default_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ZeroTime);
        }

        [TestMethod]
        public void Add_LengthLength_Default_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   ShortTime);
        }

        [TestMethod]
        public void Add_LengthLength_Default_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.DefaultTempoMap,
                                                   LargeTime);
        }

        [TestMethod]
        public void Add_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ZeroTime);
        }

        [TestMethod]
        public void Add_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ShortTime);
        }

        [TestMethod]
        public void Add_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   LargeTime);
        }

        [TestMethod]
        public void Add_LengthLength_Simple_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ZeroTime);
        }

        [TestMethod]
        public void Add_LengthLength_Simple_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   ShortTime);
        }

        [TestMethod]
        public void Add_LengthLength_Simple_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.SimpleTempoMap,
                                                   LargeTime);
        }

        [TestMethod]
        public void Add_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ZeroTime);
        }

        [TestMethod]
        public void Add_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ShortTime);
        }

        [TestMethod]
        public void Add_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Add_LengthLength(ShortSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   LargeTime);
        }

        [TestMethod]
        public void Add_LengthLength_Complex_4()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ZeroTime);
        }

        [TestMethod]
        public void Add_LengthLength_Complex_5()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   ShortTime);
        }

        [TestMethod]
        public void Add_LengthLength_Complex_6()
        {
            TimeSpanTestUtilities.Add_LengthLength(LongSpan,
                                                   MetricSpan,
                                                   TimeSpanTestUtilities.ComplexTempoMap,
                                                   LargeTime);
        }

        #endregion

        #region Subtract

        [TestMethod]
        public void Subtract_SameType_1()
        {
            TimeSpanTestUtilities.Subtract_SameType(new MusicalTimeSpan(),
                                                    new MusicalTimeSpan(),
                                                    new MusicalTimeSpan());
        }

        [TestMethod]
        public void Subtract_SameType_2()
        {
            TimeSpanTestUtilities.Subtract_SameType(new MusicalTimeSpan(11, 27),
                                                    new MusicalTimeSpan(),
                                                    new MusicalTimeSpan(11, 27));
        }

        [TestMethod]
        public void Subtract_SameType_3()
        {
            TimeSpanTestUtilities.Subtract_SameType(MusicalTimeSpan.Whole,
                                                    MusicalTimeSpan.Half + MusicalTimeSpan.Eighth,
                                                    MusicalTimeSpan.Quarter.SingleDotted());
        }

        [TestMethod]
        public void Subtract_TimeTime_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    ShortSpan,
                                                    TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Subtract_TimeTime_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    ShortSpan,
                                                    TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Subtract_TimeTime_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeTime(MetricSpan,
                                                    ShortSpan,
                                                    TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Subtract_TimeLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      ShortSpan,
                                                      TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Subtract_TimeLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      ShortSpan,
                                                      TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Subtract_TimeLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_TimeLength(MetricSpan,
                                                      ShortSpan,
                                                      TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Subtract_LengthLength_Default_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ZeroTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Default_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        ShortTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Default_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.DefaultTempoMap,
                                                        LargeTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Simple_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ZeroTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Simple_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        ShortTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Simple_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.SimpleTempoMap,
                                                        LargeTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Complex_1()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ZeroTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Complex_2()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        ShortTime);
        }

        [TestMethod]
        public void Subtract_LengthLength_Complex_3()
        {
            TimeSpanTestUtilities.Subtract_LengthLength(MetricSpan,
                                                        ShortSpan,
                                                        TimeSpanTestUtilities.ComplexTempoMap,
                                                        LargeTime);
        }

        #endregion

        #region Multiply

        [TestMethod]
        [Description("Multiply zero time span by zero.")]
        public void Multiply_1()
        {
            Assert.AreEqual(new MusicalTimeSpan(),
                            new MusicalTimeSpan().Multiply(0));
        }

        [TestMethod]
        [Description("Multiply arbitrary time span by zero.")]
        public void Multiply_2()
        {
            Assert.AreEqual(new MusicalTimeSpan(),
                            new MusicalTimeSpan(3, 7).Multiply(0));
        }

        [TestMethod]
        [Description("Multiply by integer number.")]
        public void Multiply_3()
        {
            Assert.AreEqual(MusicalTimeSpan.Whole,
                            MusicalTimeSpan.Half.Multiply(2));
        }

        [TestMethod]
        [Description("Multiply by non-integer number.")]
        public void Multiply_4()
        {
            Assert.AreEqual(MusicalTimeSpan.Quarter.SingleDotted(),
                            MusicalTimeSpan.Quarter.Multiply(1.5));
        }

        [TestMethod]
        [Description("Multiply by negative number.")]
        public void Multiply_5()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MusicalTimeSpan().Multiply(-5));
        }

        #endregion

        #region Divide

        [TestMethod]
        [Description("Divide arbitrary time span by one.")]
        public void Divide_1()
        {
            Assert.AreEqual(new MusicalTimeSpan(1, 5),
                            new MusicalTimeSpan(1, 5).Divide(1));
        }

        [TestMethod]
        [Description("Divide arbitrary time span by integer number.")]
        public void Divide_2()
        {
            Assert.AreEqual(MusicalTimeSpan.Half,
                            MusicalTimeSpan.Whole.Divide(2));
        }

        [TestMethod]
        [Description("Divide by non-integer number.")]
        public void Divide_3()
        {
            Assert.AreEqual(new MusicalTimeSpan(1, 12),
                            MusicalTimeSpan.Eighth.Divide(1.5));
        }

        [TestMethod]
        [Description("Divide by zero.")]
        public void Divide_4()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MusicalTimeSpan().Divide(0));
        }

        [TestMethod]
        [Description("Divide by negative number.")]
        public void Divide_5()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MusicalTimeSpan().Divide(-8));
        }

        [TestMethod]
        [Description("Divide zero time span by one.")]
        public void Divide_6()
        {
            Assert.AreEqual(new MusicalTimeSpan(),
                            new MusicalTimeSpan().Divide(1));
        }

        #endregion

        #region Clone

        [TestMethod]
        public void Clone_1()
        {
            TimeSpanTestUtilities.TestClone(new MusicalTimeSpan());
        }

        [TestMethod]
        public void Clone_2()
        {
            TimeSpanTestUtilities.TestClone(new MusicalTimeSpan(5, 8));
        }

        #endregion

        #endregion

        #region Private methods

        private static MidiTimeSpan GetDefaultMidiTimeSpan(MusicalTimeSpan timeSpan)
        {
            return new MidiTimeSpan(4 * TimeSpanTestUtilities.TicksPerQuarterNote * timeSpan.Numerator / timeSpan.Denominator);
        }

        #endregion
    }
}
