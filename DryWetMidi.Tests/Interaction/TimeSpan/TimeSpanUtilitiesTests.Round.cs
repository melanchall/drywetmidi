using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimeSpanUtilitiesTests
    {
        #region Constants

        private static readonly long[] SimpleCaseTimes = { 0, 1, 10, 100, 1000 };

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
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 3, 2), new BarBeatTicksTimeSpan(0, 1, 0), new BarBeatTicksTimeSpan(2, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 6) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 10), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 21) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 21) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 2, 24) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(0, 2, 18) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 1, 15), new BarBeatTicksTimeSpan(8, 2, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 3, 15), new BarBeatTicksTimeSpan(9, 2, 30) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 6, 5), new BarBeatTicksTimeSpan(4, 5, 15), new BarBeatTicksTimeSpan(5, 1, 15) },

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
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(0, 3, 87) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 0, 385), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(1, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 0, 383), new BarBeatTicksTimeSpan(1, 0, 0), new BarBeatTicksTimeSpan(0, 0, 0) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(1, 0, 10), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(1, 0, 6) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(0, 2, 17), new BarBeatTicksTimeSpan(0, 0, 15), new BarBeatTicksTimeSpan(0, 2, 3) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 1, 15), new BarBeatTicksTimeSpan(4, 1, 15) },
            new ITimeSpan[] { new BarBeatTicksTimeSpan(5, 3, 17), new BarBeatTicksTimeSpan(4, 3, 15), new BarBeatTicksTimeSpan(4, 3, 15) },

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
        public void Round_NoRounding_Simple(ITimeSpan timeSpan)
        {
            foreach (var time in SimpleCaseTimes)
            {
                var result = timeSpan.Round(TimeSpanRoundingPolicy.NoRounding, (MidiTimeSpan)time, new MidiTimeSpan(100), TempoMap.Default);
                ClassicAssert.AreEqual(timeSpan, result, $"Time span changed (time = {time}).");
                ClassicAssert.AreNotSame(timeSpan, result, $"Result refers to the same object (time = {time}).");
            }
        }

        [TestCaseSource(nameof(TimeSpans_RoundUp))]
        public void Round_RoundUp_Simple(ITimeSpan timeSpan, ITimeSpan step, ITimeSpan expectedTimeSpan)
        {
            foreach (var time in SimpleCaseTimes)
            {
                var result = timeSpan.Round(TimeSpanRoundingPolicy.RoundUp, (MidiTimeSpan)time, step, TempoMap.Default);
                ClassicAssert.AreEqual(expectedTimeSpan, result, $"Invalid result time span (time = {time}).");
                ClassicAssert.AreNotSame(timeSpan, result, $"Result refers to the same object (time = {time}).");
            }
        }

        [Test]
        public void Round_RoundUp_BarBeatTicks_1() => RoundUp_FromZero(
            timeSpan: new BarBeatTicksTimeSpan(4, 0, 1),
            step: new BarBeatTicksTimeSpan(0, 0, 1),
            tempoMap: TempoMap.Default,
            expectedTimeSpan: new BarBeatTicksTimeSpan(4, 0, 1));

        [Test]
        public void Round_RoundUp_BarBeatTicks_2([Values(0, 1, 2)] long beats, [Values(1, 2, 4)] long ticks) => RoundUp_FromZero(
            timeSpan: new BarBeatTicksTimeSpan(5, beats, 1),
            step: new BarBeatTicksTimeSpan(0, 0, ticks),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(1, 0, 0), new TimeSignature(3, 4))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(5, beats, ticks));

        [Test]
        public void Round_RoundUp_BarBeatTicks_3() => RoundUp(
            timeSpan: new MusicalTimeSpan(7, 8),
            time: new MusicalTimeSpan(6, 4),
            step: new BarBeatTicksTimeSpan(1, 0, 0),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(2, 0, 0), new TimeSignature(5, 8))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(2, 0, 0));

        [Test]
        public void Round_RoundUp_BarBeatTicks_4() => RoundUp(
            timeSpan: new MusicalTimeSpan(3, 8),
            time: new MusicalTimeSpan(6, 4),
            step: new BarBeatTicksTimeSpan(0, 1, 0),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(2, 0, 0), new TimeSignature(5, 8))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(0, 2, 0));

        [TestCaseSource(nameof(TimeSpans_RoundDown))]
        public void Round_RoundDown_Simple(ITimeSpan timeSpan, ITimeSpan step, ITimeSpan expectedTimeSpan)
        {
            foreach (var time in SimpleCaseTimes)
            {
                var result = timeSpan.Round(TimeSpanRoundingPolicy.RoundDown, (MidiTimeSpan)time, step, TempoMap.Default);
                ClassicAssert.AreEqual(expectedTimeSpan, result, $"Invalid result time span (time = {time}).");
                ClassicAssert.AreNotSame(timeSpan, result, $"Result refers to the same object (time = {time}).");
            }
        }

        [Test]
        public void Round_RoundDown_BarBeatTicks_1() => RoundDown_FromZero(
            timeSpan: new BarBeatTicksTimeSpan(0, 1, 0),
            step: new BarBeatTicksTimeSpan(1, 0, 0),
            tempoMap: TempoMap.Default,
            expectedTimeSpan: new BarBeatTicksTimeSpan(0));

        [Test]
        public void Round_RoundDown_BarBeatTicks_2([Values(0, 1, 2)] long beats) => RoundDown_FromZero(
            timeSpan: new BarBeatTicksTimeSpan(5, beats, 1),
            step: new BarBeatTicksTimeSpan(0, 0, 1),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(1, 0, 0), new TimeSignature(3, 4))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(5, beats, 1));

        [Test]
        public void Round_RoundDown_BarBeatTicks_3([Values(0, 1, 2)] long beats, [Values(2, 4)] long ticks) => RoundDown_FromZero(
            timeSpan: new BarBeatTicksTimeSpan(5, beats, 1),
            step: new BarBeatTicksTimeSpan(0, 0, ticks),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(1, 0, 0), new TimeSignature(3, 4))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(5, beats, 0));

        [Test]
        public void Round_RoundDown_BarBeatTicks_3() => RoundDown_FromZero(
            timeSpan: new BarBeatTicksTimeSpan(2, 0, 0),
            step: new BarBeatTicksTimeSpan(3, 0, 0),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(1, 0, 0), new TimeSignature(3, 4))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(0, 0, 0));

        [Test]
        public void Round_RoundDown_BarBeatTicks_4() => RoundDown(
            timeSpan: new MusicalTimeSpan(7, 8),
            time: new MusicalTimeSpan(6, 4),
            step: new BarBeatTicksTimeSpan(1, 0, 0),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(2, 0, 0), new TimeSignature(5, 8))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(1, 0, 0));

        [Test]
        public void Round_RoundDown_BarBeatTicks_5() => RoundDown(
            timeSpan: new MusicalTimeSpan(3, 8),
            time: new MusicalTimeSpan(6, 4),
            step: new BarBeatTicksTimeSpan(0, 1, 0),
            tempoMap: GetTempoMap(
                (new BarBeatTicksTimeSpan(2, 0, 0), new TimeSignature(5, 8))),
            expectedTimeSpan: new BarBeatTicksTimeSpan(0, 1, 0));

        #endregion

        #region Private methods

        private TempoMap GetTempoMap(params (ITimeSpan Time, TimeSignature TimeSignature)[] timeSignatureChanges)
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach  (var (time, timeSignature) in timeSignatureChanges)
                {
                    tempoMapManager.SetTimeSignature(time, timeSignature);
                }

                return tempoMapManager.TempoMap;
            }
        }

        private void RoundUp_FromZero(
            ITimeSpan timeSpan,
            ITimeSpan step,
            TempoMap tempoMap,
            ITimeSpan expectedTimeSpan) => Round_FromZero(
                TimeSpanRoundingPolicy.RoundUp,
                timeSpan,
                step,
                tempoMap,
                expectedTimeSpan);

        private void RoundUp(
            ITimeSpan timeSpan,
            ITimeSpan time,
            ITimeSpan step,
            TempoMap tempoMap,
            ITimeSpan expectedTimeSpan) => Round(
                TimeSpanRoundingPolicy.RoundUp,
                timeSpan,
                time,
                step,
                tempoMap,
                expectedTimeSpan);

        private void RoundDown_FromZero(
            ITimeSpan timeSpan,
            ITimeSpan step,
            TempoMap tempoMap,
            ITimeSpan expectedTimeSpan) => Round_FromZero(
                TimeSpanRoundingPolicy.RoundDown,
                timeSpan,
                step,
                tempoMap,
                expectedTimeSpan);

        private void RoundDown(
            ITimeSpan timeSpan,
            ITimeSpan time,
            ITimeSpan step,
            TempoMap tempoMap,
            ITimeSpan expectedTimeSpan) => Round(
                TimeSpanRoundingPolicy.RoundDown,
                timeSpan,
                time,
                step,
                tempoMap,
                expectedTimeSpan);

        private void Round_FromZero(
            TimeSpanRoundingPolicy roundingPolicy,
            ITimeSpan timeSpan,
            ITimeSpan step,
            TempoMap tempoMap,
            ITimeSpan expectedTimeSpan) => Round(
                roundingPolicy,
                timeSpan,
                (MidiTimeSpan)0,
                step,
                tempoMap,
                expectedTimeSpan);

        private void Round(
            TimeSpanRoundingPolicy roundingPolicy,
            ITimeSpan timeSpan,
            ITimeSpan time,
            ITimeSpan step,
            TempoMap tempoMap,
            ITimeSpan expectedTimeSpan)
        {
            var result = timeSpan.Round(roundingPolicy, time, step, tempoMap);
            ClassicAssert.AreEqual(expectedTimeSpan, result, "Invalid result time span.");
            ClassicAssert.AreNotSame(timeSpan, result, "Result refers to the same object.");
        }

        #endregion
    }
}
