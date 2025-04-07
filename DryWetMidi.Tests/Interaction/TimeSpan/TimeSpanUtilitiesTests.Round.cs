using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimeSpanUtilitiesTests
    {
        #region Constants

        private static readonly ITimeSpan[] TimeSpans_NoRounding = new ITimeSpan[]
        {
            new MidiTimeSpan(0),
            new MidiTimeSpan(100),

            new MetricTimeSpan(1, 2, 3, 4),
            new MetricTimeSpan(0),
            new MetricTimeSpan(0, 0, 0, 0),
            new MetricTimeSpan(0, 10, 0),
            new MetricTimeSpan(0, 0, 10),
            new MetricTimeSpan(10, 0, 0),

            new MusicalTimeSpan(0, 1),
            new MusicalTimeSpan(1, 4),
            new MusicalTimeSpan(8),
            new MusicalTimeSpan(10, 2),

            new BarBeatTicksTimeSpan(0),
            new BarBeatTicksTimeSpan(0, 0, 0),
            new BarBeatTicksTimeSpan(10),
            new BarBeatTicksTimeSpan(1, 2, 3),
            new BarBeatTicksTimeSpan(0, 0, 10),
            new BarBeatTicksTimeSpan(0, 10, 0),
            new BarBeatTicksTimeSpan(10, 0, 0),

            new BarBeatFractionTimeSpan(0),
            new BarBeatFractionTimeSpan(0, 10),
            new BarBeatFractionTimeSpan(10, 0),
            new BarBeatFractionTimeSpan(0, 5.5)
        };

        private static readonly object[] TimeSpans_RoundUp = new[]
        {
            new ITimeSpan[] { new MidiTimeSpan(0), new MidiTimeSpan(0), new MidiTimeSpan(0) },
            new ITimeSpan[] { new MidiTimeSpan(10), new MidiTimeSpan(0), new MidiTimeSpan(10) },
            new ITimeSpan[] { new MidiTimeSpan(10), new MidiTimeSpan(15), new MidiTimeSpan(15) },
            new ITimeSpan[] { new MidiTimeSpan(20), new MidiTimeSpan(15), new MidiTimeSpan(30) },

            new ITimeSpan[] { new MetricTimeSpan(0), new MetricTimeSpan(0), new MetricTimeSpan(0) },
            new ITimeSpan[] { new MetricTimeSpan(100), new MetricTimeSpan(0), new MetricTimeSpan(100) },
            new ITimeSpan[] { new MetricTimeSpan(0, 0, 2, 500), new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(0, 0, 4) },
            new ITimeSpan[] { new MetricTimeSpan(0, 0, 2, 500), new MetricTimeSpan(0, 0, 3), new MetricTimeSpan(0, 0, 3) },
            new ITimeSpan[] { new MetricTimeSpan(0, 0, 5), new MetricTimeSpan(0, 0, 4), new MetricTimeSpan(0, 0, 8) },

            new ITimeSpan[] { new MusicalTimeSpan(0, 1), new MusicalTimeSpan(0, 1), new MusicalTimeSpan(0, 1) },
            new ITimeSpan[] { new MusicalTimeSpan(3, 8), new MusicalTimeSpan(0, 1), new MusicalTimeSpan(3, 8) },
            new ITimeSpan[] { MusicalTimeSpan.Half, MusicalTimeSpan.Whole * 2, MusicalTimeSpan.Whole * 2 },
            new ITimeSpan[] { MusicalTimeSpan.Half, new MusicalTimeSpan(3, 8), new MusicalTimeSpan(6, 8) },

            new ITimeSpan[] { new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(10), new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(10) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 3), new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(1, 2, 3) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 10), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(2, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 1, 0), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(2, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 3), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(2, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(0, 1, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 1, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(0, 2, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 1, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(1, 2, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 3, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(1, 4, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 10), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 2, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(0, 2, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 1, 15), new BarBeatTicksTimeSpan(8, 3, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 3, 15), new BarBeatTicksTimeSpan(8, 3, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 6, 5), new BarBeatTicksTimeSpan(4, 5, 15), new BarBeatTicksTimeSpan(4, 10, 15) },

            new ITimeSpan[] { new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(10), new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(10) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 2), new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(1, 2) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0.10), new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(2, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 1), new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(2, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(0, 1) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(0, 1.2), new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(0, 2) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 1.2), new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(1, 2) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(0, 0.15), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0.1), new BarBeatFractionTimeSpan(0, 0.15), new BarBeatFractionTimeSpan(1, 0.15) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0.17), new BarBeatFractionTimeSpan(0, 0.15), new BarBeatFractionTimeSpan(1, 0.3) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(5, 0.17), new BarBeatFractionTimeSpan(4, 0.15), new BarBeatFractionTimeSpan(8, 0.3) },
        };

        private static readonly object[] TimeSpans_RoundDown = new[]
        {
            new ITimeSpan[] { new MidiTimeSpan(0), new MidiTimeSpan(0), new MidiTimeSpan(0) },
            new ITimeSpan[] { new MidiTimeSpan(10), new MidiTimeSpan(0), new MidiTimeSpan(10) },
            new ITimeSpan[] { new MidiTimeSpan(10), new MidiTimeSpan(15), new MidiTimeSpan(0) },
            new ITimeSpan[] { new MidiTimeSpan(20), new MidiTimeSpan(15), new MidiTimeSpan(15) },

            new ITimeSpan[] { new MetricTimeSpan(0), new MetricTimeSpan(0), new MetricTimeSpan(0) },
            new ITimeSpan[] { new MetricTimeSpan(100), new MetricTimeSpan(0), new MetricTimeSpan(100) },
            new ITimeSpan[] { new MetricTimeSpan(0, 0, 2, 500), new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(0, 0, 2) },
            new ITimeSpan[] { new MetricTimeSpan(0, 0, 2, 500), new MetricTimeSpan(0, 0, 3), new MetricTimeSpan(0, 0, 0) },
            new ITimeSpan[] { new MetricTimeSpan(0, 0, 5), new MetricTimeSpan(0, 0, 4), new MetricTimeSpan(0, 0, 4) },

            new ITimeSpan[] { new MusicalTimeSpan(0, 1), new MusicalTimeSpan(0, 1), new MusicalTimeSpan(0, 1) },
            new ITimeSpan[] { new MusicalTimeSpan(3, 8), new MusicalTimeSpan(0, 1), new MusicalTimeSpan(3, 8) },
            new ITimeSpan[] { MusicalTimeSpan.Half, MusicalTimeSpan.Whole * 2, new MusicalTimeSpan(0, 1) },
            new ITimeSpan[] { MusicalTimeSpan.Half, new MusicalTimeSpan(3, 8), new MusicalTimeSpan(3, 8) },

            new ITimeSpan[] { new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(10), new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(10) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 3), new BarBeatTicksTimeSpan(0), new BarBeatTicksTimeSpan(1, 2, 3) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 10), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 1, 0), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 3), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(0, 1, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 1, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(0, 1, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 1, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(1, 1, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 3, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(1, 3, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 10), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 2, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(0, 2, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 1, 15), new BarBeatTicksTimeSpan(4, 3, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 3, 15), new BarBeatTicksTimeSpan(4, 3, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 6, 5), new BarBeatTicksTimeSpan(4, 5, 15), new BarBeatTicksTimeSpan(0, 5, 0) },

            new ITimeSpan[] { new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(10), new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(10) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 2), new BarBeatFractionTimeSpan(0), new BarBeatFractionTimeSpan(1, 2) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0.10), new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 1), new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(0, 1) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(0, 1.2), new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(0, 1) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 1.2), new BarBeatFractionTimeSpan(0, 1), new BarBeatFractionTimeSpan(1, 1) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0), new BarBeatFractionTimeSpan(0, 0.15), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0.1), new BarBeatFractionTimeSpan(0, 0.15), new BarBeatFractionTimeSpan(1, 0) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(1, 0.17), new BarBeatFractionTimeSpan(0, 0.15), new BarBeatFractionTimeSpan(1, 0.15) },
            new ITimeSpan[] { new BarBeatFractionTimeSpan(5, 0.17), new BarBeatFractionTimeSpan(4, 0.15), new BarBeatFractionTimeSpan(4, 0.15) },
        };

        #endregion

        #region Test methods

        [TestCaseSource(nameof(TimeSpans_NoRounding))]
        public void Round_NoRounding(ITimeSpan timeSpan)
        {
            var result = timeSpan.Round(TimeSpanRoundingPolicy.NoRounding, 0, (MidiTimeSpan)100, TempoMap.Default);
            ClassicAssert.AreEqual(timeSpan, result, "Time span changed.");
            ClassicAssert.AreNotSame(timeSpan, result, "Result refers to the same object.");
        }

        [TestCaseSource(nameof(TimeSpans_RoundUp))]
        public void Round_RoundUp(ITimeSpan timeSpan, ITimeSpan step, ITimeSpan expectedTimeSpan)
        {
            var result = timeSpan.Round(TimeSpanRoundingPolicy.RoundUp, 0, step, TempoMap.Default);
            ClassicAssert.AreEqual(expectedTimeSpan, result, "Invalid result time span.");
            ClassicAssert.AreNotSame(timeSpan, result, "Result refers to the same object.");
        }

        [TestCaseSource(nameof(TimeSpans_RoundDown))]
        public void Round_RoundDown(ITimeSpan timeSpan, ITimeSpan step, ITimeSpan expectedTimeSpan)
        {
            var result = timeSpan.Round(TimeSpanRoundingPolicy.RoundDown, 0, step, TempoMap.Default);
            ClassicAssert.AreEqual(expectedTimeSpan, result, "Invalid result time span.");
            ClassicAssert.AreNotSame(timeSpan, result, "Result refers to the same object.");
        }

        #endregion
    }
}
