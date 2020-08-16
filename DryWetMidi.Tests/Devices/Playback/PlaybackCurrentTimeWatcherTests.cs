using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class PlaybackCurrentTimeWatcherTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Cleanup

        [OneTimeTearDown]
        public static void ClassCleanup()
        {
            PlaybackCurrentTimeWatcher.Instance.Dispose();
        }

        [TearDown]
        public static void TestCleanup()
        {
            PlaybackCurrentTimeWatcher.Instance.RemoveAllPlaybacks();
        }

        #endregion

        #region Test methods

        [Test]
        [Retry(RetriesNumber)]
        public void PlaybackNotStarted()
        {
            var waitingTime = TimeSpan.FromMilliseconds(500);
            var epsilon = TimeSpan.FromMilliseconds(10);

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new List<ITimeSpan>();
            var expectedTimes = Enumerable.Range(0, (int)(waitingTime.TotalMilliseconds / PlaybackCurrentTimeWatcher.Instance.PollingInterval.TotalMilliseconds) + 1).Select(i => new MetricTimeSpan()).ToArray();

            using (var playback = new Playback(events, TempoMap.Default))
            {
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += (_, e) => times.Add(e.Times.First().Time);

                PlaybackCurrentTimeWatcher.Instance.Start();
                Thread.Sleep(waitingTime + epsilon);
                PlaybackCurrentTimeWatcher.Instance.Stop();

                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            }

            WaitExpectedTimes(expectedTimes, times);
            CheckTimes(expectedTimes, times);
        }

        [Test]
        [Retry(RetriesNumber)]
        public void PlaybackFinished()
        {
            var waitingTime = TimeSpan.FromMilliseconds(500);
            var epsilon = TimeSpan.FromMilliseconds(10);

            var lastTime = 500L;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = lastTime }
            };

            var playback = new Playback(events, TempoMap.Default);

            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Midi);

            playback.Start();
            PlaybackCurrentTimeWatcher.Instance.Start();

            var timeout = TimeSpan.FromSeconds(30);
            var playbackFinished = SpinWait.SpinUntil(() => !playback.IsRunning, timeout);
            Assert.IsTrue(playbackFinished, $"Playback is not finished for {timeout}.");

            var times = new List<ITimeSpan>();
            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += (_, e) => times.Add(e.Times.First().Time);

            Thread.Sleep(waitingTime + epsilon);
            PlaybackCurrentTimeWatcher.Instance.Stop();

            CheckTimes(
                Enumerable.Range(0, (int)(waitingTime.TotalMilliseconds / PlaybackCurrentTimeWatcher.Instance.PollingInterval.TotalMilliseconds)).Select(i => new MidiTimeSpan(lastTime)).ToArray(),
                times);

            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            playback.Dispose();
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchMultiplePlaybacks()
        {
            var tempoMap = TempoMap.Default;

            var playback1 = new Playback(new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 400 } }, tempoMap);
            var playback2 = new Playback(new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 200 } }, tempoMap);

            var times = new Dictionary<Playback, List<ITimeSpan>>
            {
                [playback1] = new List<ITimeSpan>(),
                [playback2] = new List<ITimeSpan>()
            };

            var expectedTimes1 = new[]
            {
                new MidiTimeSpan(0),
                new MidiTimeSpan(100),
                new MidiTimeSpan(200),
                new MidiTimeSpan(300),
                new MidiTimeSpan(400)
            };

            var expectedTimes2 = new List<ITimeSpan>
            {
                new MidiTimeSpan(0),
                new MidiTimeSpan(100),
                new MidiTimeSpan(200),
                new MidiTimeSpan(200),
                new MidiTimeSpan(200)
            };

            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeConverter.ConvertTo<MetricTimeSpan>(100, tempoMap);

            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback1, TimeSpanType.Midi);
            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback2, TimeSpanType.Midi);

            EventHandler<PlaybackCurrentTimeChangedEventArgs> currentTimeChangedHandler = (_, e) =>
            {
                foreach (var time in e.Times)
                {
                    times[time.Playback].Add(time.Time);
                }
            };

            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += currentTimeChangedHandler;

            playback1.Start();
            playback2.Start();

            PlaybackCurrentTimeWatcher.Instance.Start();

            SpinWait.SpinUntil(() => !playback1.IsRunning && !playback2.IsRunning);

            PlaybackCurrentTimeWatcher.Instance.Stop();
            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);
            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);

            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= currentTimeChangedHandler;

            playback1.Dispose();
            playback2.Dispose();

            CheckTimes(expectedTimes1, times[playback1], "Playback 1.");
            CheckTimes(expectedTimes2, times[playback2], "Playback 2.");
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchMultiplePlaybacks_RemoveOne()
        {
            var tempoMap = TempoMap.Default;

            var playback1 = new Playback(new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 400 } }, tempoMap);
            var playback2 = new Playback(new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 500 } }, tempoMap);

            var times = new Dictionary<Playback, List<ITimeSpan>>
            {
                [playback1] = new List<ITimeSpan>(),
                [playback2] = new List<ITimeSpan>()
            };

            var expectedTimes1 = new[]
            {
                new MidiTimeSpan(0),
                new MidiTimeSpan(100),
                new MidiTimeSpan(200)
            };

            var expectedTimes2 = new List<ITimeSpan>
            {
                new MidiTimeSpan(0),
                new MidiTimeSpan(100),
                new MidiTimeSpan(200),
                new MidiTimeSpan(300),
                new MidiTimeSpan(400),
                new MidiTimeSpan(500)
            };

            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeConverter.ConvertTo<MetricTimeSpan>(100, tempoMap);

            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback1, TimeSpanType.Midi);
            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback2, TimeSpanType.Midi);

            EventHandler<PlaybackCurrentTimeChangedEventArgs> currentTimeChangedHandler = (_, e) =>
            {
                foreach (var time in e.Times)
                {
                    times[time.Playback].Add(time.Time);
                }
            };

            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += currentTimeChangedHandler;

            playback1.Start();
            playback2.Start();

            PlaybackCurrentTimeWatcher.Instance.Start();

            Thread.Sleep(TimeConverter.ConvertTo<MetricTimeSpan>(200, tempoMap));
            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);

            SpinWait.SpinUntil(() => !playback1.IsRunning && !playback2.IsRunning);

            PlaybackCurrentTimeWatcher.Instance.Stop();
            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);

            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= currentTimeChangedHandler;

            playback1.Dispose();
            playback2.Dispose();

            CheckTimes(expectedTimes1, times[playback1]);
            CheckTimes(expectedTimes2, times[playback2]);
        }

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

        #endregion

        #region Private methods

        private static void CheckWatchCurrentTime(
            ITimeSpan playbackLength,
            ITimeSpan pollingInterval,
            TimeSpanType timeType,
            ICollection<ITimeSpan> expectedTimes)
        {
            var tempoMap = TempoMap.Default;
            var length = TimeConverter.ConvertFrom(playbackLength, tempoMap);

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = length }
            };

            var times = new List<ITimeSpan>();

            using (var playback = new Playback(events, tempoMap))
            {
                PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeConverter.ConvertTo<MetricTimeSpan>(pollingInterval, tempoMap);
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, timeType);

                EventHandler<PlaybackCurrentTimeChangedEventArgs> currentTimeChangedHandler = (_, e) => times.Add(e.Times.First().Time);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += currentTimeChangedHandler;

                playback.Start();
                PlaybackCurrentTimeWatcher.Instance.Start();

                var timeout = TimeSpan.FromSeconds(30);
                var playbackFinished = SpinWait.SpinUntil(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, $"Playback is not finished for {timeout}.");

                WaitExpectedTimes(expectedTimes, times);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= currentTimeChangedHandler;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            }

            CheckTimes(expectedTimes, times);
        }

        private static void WaitExpectedTimes(ICollection<ITimeSpan> expectedTimes, ICollection<ITimeSpan> times)
        {
            var timeout = TimeSpan.FromMilliseconds(30);
            var timesReceived = SpinWait.SpinUntil(() => times.Count >= expectedTimes.Count, timeout);
            Assert.IsTrue(timesReceived, $"Times are not received for {timeout}.");
        }

        private static void CheckTimes(ICollection<ITimeSpan> expectedTimes, ICollection<ITimeSpan> actualTimes, string message = null)
        {
            Assert.AreEqual(expectedTimes.Count, actualTimes.Count, $"Count of times is invalid. {message}");

            foreach (var expectedActual in expectedTimes.Zip(actualTimes, (e, a) => new { Expected = e, Actual = a }))
            {
                var expected = expectedActual.Expected;
                var actual = expectedActual.Actual;

                var expectedType = expected.GetType();
                var actualType = actual.GetType();
                Assert.AreEqual(expectedType, actualType, "Types are different.");

                Assert.IsTrue(AreTimeSpansEqual(expected, actual), $"Time is invalid. Expected {expected} but received {actual}. {message}");
            }
        }

        private static bool AreTimeSpansEqual(ITimeSpan x, ITimeSpan y)
        {
            const long microsecondsEpsilon = 10000;
            const long ticksEpsilon = 3;
            const double fractionEpsilon = 0.01;
            const double fractionalBeatsEpsilon = 0.1;

            if (x.GetType() != y.GetType())
                return false;

            if (x is MetricTimeSpan xMetric && y is MetricTimeSpan yMetric)
                return Math.Abs(xMetric.TotalMicroseconds - yMetric.TotalMicroseconds) < microsecondsEpsilon;

            if (x is BarBeatTicksTimeSpan xBarBeat && y is BarBeatTicksTimeSpan yBarBeat)
                return xBarBeat.Bars == yBarBeat.Bars &&
                       xBarBeat.Beats == yBarBeat.Beats &&
                       Math.Abs(xBarBeat.Ticks - yBarBeat.Ticks) < ticksEpsilon;

            if (x is BarBeatFractionTimeSpan xBarBeatFraction && y is BarBeatFractionTimeSpan yBarBeatFraction)
                return xBarBeatFraction.Bars == yBarBeatFraction.Bars &&
                       Math.Abs(xBarBeatFraction.Beats - yBarBeatFraction.Beats) < fractionalBeatsEpsilon;

            if (x is MusicalTimeSpan xMusical && y is MusicalTimeSpan yMusical)
                return Math.Abs(xMusical.Numerator / (double)xMusical.Denominator - yMusical.Numerator / (double)yMusical.Denominator) < fractionEpsilon;

            if (x is MidiTimeSpan xMidi && y is MidiTimeSpan yMidi)
                return Math.Abs(xMidi.TimeSpan - yMidi.TimeSpan) < ticksEpsilon;

            return false;
        }

        #endregion
    }
}
