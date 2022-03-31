using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackCurrentTimeWatcherTests
    {
        #region Test methods

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTime_Metric()
        {
            CheckWatchCurrentTime(
                playbackLength: new MetricTimeSpan(0, 0, 1),
                pollingInterval: new MetricTimeSpan(0, 0, 0, 50),
                timeType: TimeSpanType.Metric,
                expectedTimes: Enumerable.Range(0, 21).Select(i => (MetricTimeSpan)TimeSpan.FromMilliseconds(i * 50)).ToArray());
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTime_BarBeat()
        {
            CheckWatchCurrentTime(
                playbackLength: new BarBeatTicksTimeSpan(3, 0, 0),
                pollingInterval: new BarBeatTicksTimeSpan(1, 0, 0),
                timeType: TimeSpanType.BarBeatTicks,
                expectedTimes: new[]
                {
                    new BarBeatTicksTimeSpan(0, 0, 0),
                    new BarBeatTicksTimeSpan(1, 0, 0),
                    new BarBeatTicksTimeSpan(2, 0, 1),
                    new BarBeatTicksTimeSpan(3, 0, 1)
                });
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTime_BarBeatFraction()
        {
            CheckWatchCurrentTime(
                playbackLength: new BarBeatFractionTimeSpan(1, 0.00),
                pollingInterval: new BarBeatFractionTimeSpan(0, 0.50),
                timeType: TimeSpanType.BarBeatFraction,
                expectedTimes: new[]
                {
                    new BarBeatFractionTimeSpan(0, 0.0),
                    new BarBeatFractionTimeSpan(0, 0.50),
                    new BarBeatFractionTimeSpan(0, 1.0),
                    new BarBeatFractionTimeSpan(0, 1.50),
                    new BarBeatFractionTimeSpan(0, 2.0),
                    new BarBeatFractionTimeSpan(0, 2.50),
                    new BarBeatFractionTimeSpan(0, 3.0),
                    new BarBeatFractionTimeSpan(0, 3.50),
                    new BarBeatFractionTimeSpan(1, 0.0)
                });
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTime_Musical()
        {
            CheckWatchCurrentTime(
                playbackLength: new MusicalTimeSpan(10, 16),
                pollingInterval: new MusicalTimeSpan(1, 16),
                timeType: TimeSpanType.Musical,
                expectedTimes: Enumerable.Range(0, 11).Select(i => new MusicalTimeSpan(i, 16)).ToArray());
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTime_Midi()
        {
            CheckWatchCurrentTime(
                playbackLength: new MidiTimeSpan(500),
                pollingInterval: new MidiTimeSpan(100),
                timeType: TimeSpanType.Midi,
                expectedTimes: Enumerable.Range(0, 6).Select(i => new MidiTimeSpan(i * 100)).ToArray());
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTimeWithCommonTimeType_Metric()
        {
            CheckWatchCurrentTimeWithCommonTimeType(
                playbackLength: new MetricTimeSpan(0, 0, 1),
                pollingInterval: new MetricTimeSpan(0, 0, 0, 50),
                timeType: TimeSpanType.Metric,
                expectedTimes: Enumerable.Range(0, 21).Select(i => (MetricTimeSpan)TimeSpan.FromMilliseconds(i * 50)).ToArray());
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTimeWithCommonTimeType_BarBeat()
        {
            CheckWatchCurrentTimeWithCommonTimeType(
                playbackLength: new BarBeatTicksTimeSpan(3, 0, 0),
                pollingInterval: new BarBeatTicksTimeSpan(1, 0, 0),
                timeType: TimeSpanType.BarBeatTicks,
                expectedTimes: new[]
                {
                    new BarBeatTicksTimeSpan(0, 0, 0),
                    new BarBeatTicksTimeSpan(1, 0, 0),
                    new BarBeatTicksTimeSpan(2, 0, 1),
                    new BarBeatTicksTimeSpan(3, 0, 1)
                });
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTimeWithCommonTimeType_BarBeatFraction()
        {
            CheckWatchCurrentTimeWithCommonTimeType(
                playbackLength: new BarBeatFractionTimeSpan(1, 0.00),
                pollingInterval: new BarBeatFractionTimeSpan(0, 0.50),
                timeType: TimeSpanType.BarBeatFraction,
                expectedTimes: new[]
                {
                    new BarBeatFractionTimeSpan(0, 0.0),
                    new BarBeatFractionTimeSpan(0, 0.50),
                    new BarBeatFractionTimeSpan(0, 1.0),
                    new BarBeatFractionTimeSpan(0, 1.50),
                    new BarBeatFractionTimeSpan(0, 2.0),
                    new BarBeatFractionTimeSpan(0, 2.50),
                    new BarBeatFractionTimeSpan(0, 3.0),
                    new BarBeatFractionTimeSpan(0, 3.50),
                    new BarBeatFractionTimeSpan(1, 0.0)
                });
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTimeWithCommonTimeType_Musical()
        {
            CheckWatchCurrentTimeWithCommonTimeType(
                playbackLength: new MusicalTimeSpan(10, 16),
                pollingInterval: new MusicalTimeSpan(1, 16),
                timeType: TimeSpanType.Musical,
                expectedTimes: Enumerable.Range(0, 11).Select(i => new MusicalTimeSpan(i, 16)).ToArray());
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchCurrentTimeWithCommonTimeType_Midi()
        {
            CheckWatchCurrentTimeWithCommonTimeType(
                playbackLength: new MidiTimeSpan(500),
                pollingInterval: new MidiTimeSpan(100),
                timeType: TimeSpanType.Midi,
                expectedTimes: Enumerable.Range(0, 6).Select(i => new MidiTimeSpan(i * 100)).ToArray());
        }

        #endregion
    }
}
