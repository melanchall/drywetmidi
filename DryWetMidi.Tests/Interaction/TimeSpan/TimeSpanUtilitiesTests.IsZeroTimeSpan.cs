using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimeSpanUtilitiesTests
    {
        #region Constants

        private static readonly Random Random = new Random();

        private static readonly object[] ZeroMathTimeSpansData = new[]
        {
            new ITimeSpan[] { new MidiTimeSpan(), new MidiTimeSpan() },
            new ITimeSpan[] { new MetricTimeSpan(), new MetricTimeSpan() },
            new ITimeSpan[] { new MusicalTimeSpan(), new MusicalTimeSpan() },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(), new BarBeatTicksTimeSpan() },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(), new BarBeatFractionTimeSpan() },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(), new MidiTimeSpan() },
            new ITimeSpan[] { new MusicalTimeSpan(), new MidiTimeSpan() },
            new ITimeSpan[] { new MusicalTimeSpan(), new BarBeatTicksTimeSpan() },
            new ITimeSpan[] { new MusicalTimeSpan(), new MidiTimeSpan().Add(new BarBeatTicksTimeSpan(), TimeSpanMode.TimeLength) },
            new ITimeSpan[] { new MetricTimeSpan().Subtract(new MidiTimeSpan(), TimeSpanMode.LengthLength), new MidiTimeSpan().Add(new BarBeatTicksTimeSpan(), TimeSpanMode.TimeLength) },
        };

        private static readonly object[] NonZeroMathTimeSpansData = new[]
        {
            new ITimeSpan[] { new MidiTimeSpan(), new MidiTimeSpan(1) },
            new ITimeSpan[] { new MetricTimeSpan(1), new MetricTimeSpan(2) },
            new ITimeSpan[] { new MusicalTimeSpan(), new MusicalTimeSpan(1, 1) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1), new BarBeatTicksTimeSpan() },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1), new BarBeatFractionTimeSpan() },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(), new MidiTimeSpan(1) },
            new ITimeSpan[] { new MusicalTimeSpan(2, 4), new MidiTimeSpan() },
            new ITimeSpan[] { new MusicalTimeSpan(1), new BarBeatTicksTimeSpan() },
            new ITimeSpan[] { new MusicalTimeSpan(), new MidiTimeSpan(1).Add(new BarBeatTicksTimeSpan(), TimeSpanMode.TimeLength) },
            new ITimeSpan[] { new MetricTimeSpan(1).Subtract(new MidiTimeSpan(), TimeSpanMode.LengthLength), new MidiTimeSpan().Add(new BarBeatTicksTimeSpan(), TimeSpanMode.TimeLength) },
        };

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

        #endregion

        #region Test methods

        [Test]
        public void IsZeroTimeSpan_DefaultTimeSpans()
        {
            var timeSpans = typeof(ITimeSpan)
                .Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && typeof(ITimeSpan).IsAssignableFrom(t) && t != typeof(MathTimeSpan))
                .Select(t => (ITimeSpan)Activator.CreateInstance(t))
                .ToArray();

            Assert.Greater(timeSpans.Length, 0, "Time spans are not created.");

            foreach (var timeSpan in timeSpans)
            {
                Assert.IsTrue(timeSpan.IsZeroTimeSpan(), $"Time span [{timeSpan}] is not zero.");
            }
        }

        [TestCaseSource(nameof(ZeroTimeSpans))]
        public void IsZeroTimeSpan_True(ITimeSpan timeSpan)
        {
            Assert.IsTrue(timeSpan.IsZeroTimeSpan(), $"Time span [{timeSpan}] is not zero.");
        }

        [TestCaseSource(nameof(NonZeroTimeSpans))]
        public void IsZeroTimeSpan_False(ITimeSpan timeSpan)
        {
            Assert.IsFalse(timeSpan.IsZeroTimeSpan(), $"Time span [{timeSpan}] is zero.");
        }

        [TestCaseSource(nameof(ZeroMathTimeSpansData))]
        public void IsZeroTimeSpan_MathTimeSpan_True(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            var mathTimeSpan = CreateMathTimeSpan(timeSpan1, timeSpan2);
            Assert.IsTrue(mathTimeSpan.IsZeroTimeSpan(), $"Time span [{mathTimeSpan}] is not zero.");
        }

        [TestCaseSource(nameof(NonZeroMathTimeSpansData))]
        public void IsZeroTimeSpan_MathTimeSpan_False(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            var mathTimeSpan = CreateMathTimeSpan(timeSpan1, timeSpan2);
            Assert.IsFalse(mathTimeSpan.IsZeroTimeSpan(), $"Time span [{mathTimeSpan}] is zero.");
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
                operations[Random.Next(operations.Length)],
                modes[Random.Next(modes.Length)]);
        }

        #endregion
    }
}
