using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public sealed class MidiTimeSpanTests
    {
        #region Constants

        private static readonly MetricTimeSpan MetricSpan = new MetricTimeSpan(0, 2, 30);

        private static readonly MetricTimeSpan ShortMetricTime = new MetricTimeSpan(0, 0, 20);
        private static readonly MetricTimeSpan LargeMetricTime = new MetricTimeSpan(0, 5, 20);

        private static readonly MidiTimeSpan ShortSpan = new MidiTimeSpan(130);
        private static readonly MidiTimeSpan LongSpan = new MidiTimeSpan(1000000);

        private const long ZeroTime = 0;
        private const long ShortTime = 1000;
        private const long LargeTime = 100000;

        #endregion

        #region Test methods

        #region Convert

        #region Default

        [TestMethod]
        public void Convert_Default_1()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 null,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.DefaultTempoMap);
        }

        [TestMethod]
        public void Convert_Default_3()
        {
            var tempoMap = TimeSpanTestUtilities.DefaultTempoMap;
            var time = ShortMetricTime;

            TimeSpanTestUtilities.TestConversion(LengthConverter.ConvertTo<MidiTimeSpan>(MetricSpan,
                                                                                          time,
                                                                                          tempoMap),
                                                 MetricSpan,
                                                 time,
                                                 tempoMap);
        }

        [TestMethod]
        public void Convert_Default_4()
        {
            var tempoMap = TimeSpanTestUtilities.DefaultTempoMap;
            var time = LargeMetricTime;

            TimeSpanTestUtilities.TestConversion(LengthConverter.ConvertTo<MidiTimeSpan>(MetricSpan,
                                                                                          time,
                                                                                          tempoMap),
                                                 MetricSpan,
                                                 time,
                                                 tempoMap);
        }

        #endregion

        #region Simple

        [TestMethod]
        public void Convert_Simple_1()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 null,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.SimpleTempoMap);
        }

        [TestMethod]
        public void Convert_Simple_3()
        {
            var tempoMap = TimeSpanTestUtilities.SimpleTempoMap;
            var time = ShortMetricTime;

            TimeSpanTestUtilities.TestConversion(LengthConverter.ConvertTo<MidiTimeSpan>(MetricSpan,
                                                                                          time,
                                                                                          tempoMap),
                                                 MetricSpan,
                                                 time,
                                                 tempoMap);
        }

        [TestMethod]
        public void Convert_Simple_4()
        {
            var tempoMap = TimeSpanTestUtilities.SimpleTempoMap;
            var time = LargeMetricTime;

            TimeSpanTestUtilities.TestConversion(LengthConverter.ConvertTo<MidiTimeSpan>(MetricSpan,
                                                                                          time,
                                                                                          tempoMap),
                                                 MetricSpan,
                                                 time,
                                                 tempoMap);
        }

        #endregion

        #region Complex

        [TestMethod]
        public void Convert_Complex_1()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 null,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_2()
        {
            TimeSpanTestUtilities.TestConversion(ShortSpan,
                                                 ShortSpan,
                                                 MetricSpan,
                                                 TimeSpanTestUtilities.ComplexTempoMap);
        }

        [TestMethod]
        public void Convert_Complex_3()
        {
            var tempoMap = TimeSpanTestUtilities.ComplexTempoMap;
            var time = ShortMetricTime;

            TimeSpanTestUtilities.TestConversion(LengthConverter.ConvertTo<MidiTimeSpan>(MetricSpan,
                                                                                          time,
                                                                                          tempoMap),
                                                 MetricSpan,
                                                 time,
                                                 tempoMap);
        }

        [TestMethod]
        public void Convert_Complex_4()
        {
            var tempoMap = TimeSpanTestUtilities.ComplexTempoMap;
            var time = LargeMetricTime;

            TimeSpanTestUtilities.TestConversion(LengthConverter.ConvertTo<MidiTimeSpan>(MetricSpan,
                                                                                          time,
                                                                                          tempoMap),
                                                 MetricSpan,
                                                 time,
                                                 tempoMap);
        }

        #endregion

        #endregion

        #region Parse

        [TestMethod]
        [Description("Parse zero MIDI time span.")]
        public void Parse_1()
        {
            TimeSpanTestUtilities.Parse("0", new MidiTimeSpan());
        }

        [TestMethod]
        [Description("Parse short MIDI time span.")]
        public void Parse_2()
        {
            TimeSpanTestUtilities.Parse("100", new MidiTimeSpan(100));
        }

        [TestMethod]
        [Description("Parse long MIDI time span.")]
        public void Parse_3()
        {
            TimeSpanTestUtilities.Parse("123456", new MidiTimeSpan(123456));
        }

        #endregion

        #region Add

        [TestMethod]
        public void Add_SameType_1()
        {
            TimeSpanTestUtilities.Add_SameType(new MidiTimeSpan(),
                                               new MidiTimeSpan(),
                                               new MidiTimeSpan());
        }

        [TestMethod]
        public void Add_SameType_2()
        {
            TimeSpanTestUtilities.Add_SameType(new MidiTimeSpan(123),
                                               new MidiTimeSpan(),
                                               new MidiTimeSpan(123));
        }

        [TestMethod]
        public void Add_SameType_3()
        {
            TimeSpanTestUtilities.Add_SameType(new MidiTimeSpan(1000),
                                               new MidiTimeSpan(123),
                                               new MidiTimeSpan(1123));
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
            TimeSpanTestUtilities.Subtract_SameType(new MidiTimeSpan(),
                                                    new MidiTimeSpan(),
                                                    new MidiTimeSpan());
        }

        [TestMethod]
        public void Subtract_SameType_2()
        {
            TimeSpanTestUtilities.Subtract_SameType(new MidiTimeSpan(1123),
                                                    new MidiTimeSpan(),
                                                    new MidiTimeSpan(1123));
        }

        [TestMethod]
        public void Subtract_SameType_3()
        {
            TimeSpanTestUtilities.Subtract_SameType(new MidiTimeSpan(1123),
                                                    new MidiTimeSpan(1000),
                                                    new MidiTimeSpan(123));
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
            Assert.AreEqual(new MidiTimeSpan(),
                            new MidiTimeSpan().Multiply(0));
        }

        [TestMethod]
        [Description("Multiply arbitrary time span by zero.")]
        public void Multiply_2()
        {
            Assert.AreEqual(new MidiTimeSpan(),
                            new MidiTimeSpan(1234).Multiply(0));
        }

        [TestMethod]
        [Description("Multiply by integer number.")]
        public void Multiply_3()
        {
            Assert.AreEqual(new MidiTimeSpan(2468),
                            new MidiTimeSpan(1234).Multiply(2));
        }

        [TestMethod]
        [Description("Multiply by non-integer number.")]
        public void Multiply_4()
        {
            Assert.AreEqual(new MidiTimeSpan(1851),
                            new MidiTimeSpan(1234).Multiply(1.5));
        }

        [TestMethod]
        [Description("Multiply by negative number.")]
        public void Multiply_5()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MidiTimeSpan().Multiply(-5));
        }

        #endregion

        #region Divide

        [TestMethod]
        [Description("Divide arbitrary time span by one.")]
        public void Divide_1()
        {
            Assert.AreEqual(new MidiTimeSpan(1234),
                            new MidiTimeSpan(1234).Divide(1));
        }

        [TestMethod]
        [Description("Divide arbitrary time span by integer number.")]
        public void Divide_2()
        {
            Assert.AreEqual(new MidiTimeSpan(617),
                            new MidiTimeSpan(1234).Divide(2));
        }

        [TestMethod]
        [Description("Divide by non-integer number.")]
        public void Divide_3()
        {
            Assert.AreEqual(new MidiTimeSpan(824),
                            new MidiTimeSpan(1236).Divide(1.5));
        }

        [TestMethod]
        [Description("Divide by zero.")]
        public void Divide_4()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MidiTimeSpan().Divide(0));
        }

        [TestMethod]
        [Description("Divide by negative number.")]
        public void Divide_5()
        {
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new MidiTimeSpan().Divide(-8));
        }

        [TestMethod]
        [Description("Divide zero time span by one.")]
        public void Divide_6()
        {
            Assert.AreEqual(new MidiTimeSpan(),
                            new MidiTimeSpan().Divide(1));
        }

        #endregion

        #region Clone

        [TestMethod]
        public void Clone_1()
        {
            TimeSpanTestUtilities.TestClone(new MidiTimeSpan());
        }

        [TestMethod]
        public void Clone_2()
        {
            TimeSpanTestUtilities.TestClone(new MidiTimeSpan(123));
        }

        #endregion

        #endregion
    }
}
