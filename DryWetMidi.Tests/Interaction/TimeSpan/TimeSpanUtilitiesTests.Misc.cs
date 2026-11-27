using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimeSpanUtilitiesTests
    {
        #region Constants

        private static readonly ITimeSpan[] ZeroTimeSpans = new ITimeSpan[]
        {
            new MidiTimeSpan(),
            new MidiTimeSpan(0),

            new MetricTimeSpan(),
            new MetricTimeSpan(0),
            new MetricTimeSpan(0, 0, 0, 0),

            new MusicalTimeSpan(),
            new MusicalTimeSpan(0, 1),

            new BarBeatTicksTimeSpan(),
            new BarBeatTicksTimeSpan(0),
            new BarBeatTicksTimeSpan(0, 0, 0),

            new BarBeatFractionTimeSpan(),
            new BarBeatFractionTimeSpan(0),
            new BarBeatFractionTimeSpan(0, 0)
        };

        private static readonly ITimeSpan[] NonZeroTimeSpans = new ITimeSpan[]
        {
            new MidiTimeSpan(1),
            new MidiTimeSpan(10),

            new MetricTimeSpan(1),
            new MetricTimeSpan(1, 2, 3),
            new MetricTimeSpan(0, 0, 1, 0),
            new MetricTimeSpan(0, 1, 0, 0),

            new MusicalTimeSpan(1, 1),
            new MusicalTimeSpan(3, 8),

            new BarBeatTicksTimeSpan(1),
            new BarBeatTicksTimeSpan(0, 1, 0),
            new BarBeatTicksTimeSpan(0, 0, 1),

            new BarBeatFractionTimeSpan(1),
            new BarBeatFractionTimeSpan(0, 0.1),
            new BarBeatFractionTimeSpan(1, 0)
        };

        private static readonly object[] ParseData_Valid = new object[]
        {
            new object[] { "100", new MidiTimeSpan(100) },
            new object[] { "1:2:3", new MetricTimeSpan(1, 2, 3) },
            new object[] { "1h2m3s4ms", new MetricTimeSpan(1, 2, 3, 4) },
            new object[] { "3/8", new MusicalTimeSpan(3, 8) },
            new object[] { "1.2.3", new BarBeatTicksTimeSpan(1, 2, 3) },
            new object[] { "1_2.5", new BarBeatFractionTimeSpan(1, 2.5) }
        };

        #endregion

        #region Test methods

        [Test]
        public void GetZeroTimeSpan_Generic_Midi() =>
            ClassicAssert.AreEqual(new MidiTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan<MidiTimeSpan>());

        [Test]
        public void GetZeroTimeSpan_Generic_Metric() =>
            ClassicAssert.AreEqual(new MetricTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan<MetricTimeSpan>());

        [Test]
        public void GetZeroTimeSpan_Generic_Musical() =>
            ClassicAssert.AreEqual(new MusicalTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan<MusicalTimeSpan>());

        [Test]
        public void GetZeroTimeSpan_Generic_BarBeatTicks() =>
            ClassicAssert.AreEqual(new BarBeatTicksTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan<BarBeatTicksTimeSpan>());

        [Test]
        public void GetZeroTimeSpan_Generic_BarBeatFraction() =>
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan<BarBeatFractionTimeSpan>());

        [Test]
        public void GetZeroTimeSpan_Enum_Midi() =>
            ClassicAssert.AreEqual(new MidiTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan(TimeSpanType.Midi));

        [Test]
        public void GetZeroTimeSpan_Enum_Metric() =>
            ClassicAssert.AreEqual(new MetricTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan(TimeSpanType.Metric));

        [Test]
        public void GetZeroTimeSpan_Enum_Musical() =>
            ClassicAssert.AreEqual(new MusicalTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan(TimeSpanType.Musical));

        [Test]
        public void GetZeroTimeSpan_Enum_BarBeatTicks() =>
            ClassicAssert.AreEqual(new BarBeatTicksTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan(TimeSpanType.BarBeatTicks));

        [Test]
        public void GetZeroTimeSpan_Enum_BarBeatFraction() =>
            ClassicAssert.AreEqual(new BarBeatFractionTimeSpan(), TimeSpanUtilities.GetZeroTimeSpan(TimeSpanType.BarBeatFraction));

        [Test]
        public void IsZeroTimeSpan_DefaultTimeSpans()
        {
            var timeSpans = typeof(ITimeSpan)
                .Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && typeof(ITimeSpan).IsAssignableFrom(t) && t != typeof(MathTimeSpan))
                .Select(t => (ITimeSpan)Activator.CreateInstance(t))
                .ToArray();

            ClassicAssert.Greater(timeSpans.Length, 0, "Time spans are not created.");

            foreach (var timeSpan in timeSpans)
            {
                ClassicAssert.IsTrue(timeSpan.IsZeroTimeSpan(), $"Time span [{timeSpan}] is not zero.");
            }
        }

        [TestCaseSource(nameof(ZeroTimeSpans))]
        public void IsZeroTimeSpan_True(ITimeSpan timeSpan)
        {
            ClassicAssert.IsTrue(timeSpan.IsZeroTimeSpan(), $"Time span [{timeSpan}] is not zero.");
        }

        [TestCaseSource(nameof(NonZeroTimeSpans))]
        public void IsZeroTimeSpan_False(ITimeSpan timeSpan)
        {
            ClassicAssert.IsFalse(timeSpan.IsZeroTimeSpan(), $"Time span [{timeSpan}] is zero.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string input, ITimeSpan expectedTimeSpan)
        {
            var actualTimeSpan = TimeSpanUtilities.Parse(input);

            ClassicAssert.AreEqual(expectedTimeSpan.GetType(), actualTimeSpan.GetType(), $"Invalid type of time span parsed for '{input}'.");
            ClassicAssert.AreEqual(expectedTimeSpan, actualTimeSpan, $"Invalid time span parsed for '{input}'.");
        }

        [Test]
        public void Parse_SpanSlice()
        {
            var input = "[3/8]".AsSpan(1, 3);

            var actualTimeSpan = TimeSpanUtilities.Parse(input);

            ClassicAssert.AreEqual(new MusicalTimeSpan(3, 8), actualTimeSpan, "Invalid time span parsed from span slice.");
        }

        [Test]
        public void Parse_EmptyOrWhiteSpace([Values(null, "", "  ", "\t")] string input)
        {
            ClassicAssert.Throws<ArgumentException>(
                () => TimeSpanUtilities.Parse(input),
                $"Exception is not thrown for '{input}'.");
        }

        [Test]
        public void Parse_InvalidFormat([Values("abc", "1/", "1.2.3.4", "1h2x")] string input)
        {
            ClassicAssert.Throws<FormatException>(
                () => TimeSpanUtilities.Parse(input),
                $"Exception is not thrown for '{input}'.");
        }

        #endregion

        #region Private methods

        private static MathTimeSpan CreateMathTimeSpan(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            var operations = Enum.GetValues(typeof(MathOperation)).OfType<MathOperation>().ToArray();
            var modes = Enum.GetValues(typeof(TimeSpanMode)).OfType<TimeSpanMode>().ToArray();

            return new MathTimeSpan(
                timeSpan1,
                timeSpan2,
                operations[DryWetMidi.Common.Random.Instance.Next(operations.Length)],
                modes[DryWetMidi.Common.Random.Instance.Next(modes.Length)]);
        }

        #endregion
    }
}
