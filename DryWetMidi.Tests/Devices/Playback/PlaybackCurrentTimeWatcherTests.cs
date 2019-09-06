using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
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
            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new List<ITimeSpan>();
            var expectedTimes = Enumerable.Range(0, 6).Select(i => new MetricTimeSpan()).ToArray();

            using (var playback = new Playback(events, TempoMap.Default))
            {
                PlaybackCurrentTimeWatcher.Instance.WatchRunningPlaybacksOnly = false;
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += (_, e) => times.Add(e.Times.First().Time);

                PlaybackCurrentTimeWatcher.Instance.Start();
                Thread.Sleep(500);
                PlaybackCurrentTimeWatcher.Instance.Stop();

                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            }

            WaitExpectedTimes(expectedTimes, times);
            CheckTimes(expectedTimes, times);
        }

        [Test]
        public void PlaybackFinished()
        {
            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 500 }
            };

            var playback = new Playback(events, TempoMap.Default);

            PlaybackCurrentTimeWatcher.Instance.WatchRunningPlaybacksOnly = false;
            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Midi);

            playback.Start();
            PlaybackCurrentTimeWatcher.Instance.Start();

            SpinWait.SpinUntil(() => !playback.IsRunning);

            var times = new List<ITimeSpan>();
            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += (_, e) => times.Add(e.Times.First().Time);

            Thread.Sleep(500);

            CheckTimes(
                Enumerable.Range(0, 5).Select(i => new MidiTimeSpan(500)).ToArray(),
                times);

            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            playback.Dispose();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void WatchMultiplePlaybacks(bool watchRunningPlaybacksOnly)
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
                new MidiTimeSpan(200)
            };

            if (!watchRunningPlaybacksOnly)
                expectedTimes2.AddRange(new[] { new MidiTimeSpan(200), new MidiTimeSpan(200) });

            PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeConverter.ConvertTo<MetricTimeSpan>(100, tempoMap);
            PlaybackCurrentTimeWatcher.Instance.WatchRunningPlaybacksOnly = watchRunningPlaybacksOnly;

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

            CheckTimes(expectedTimes1, times[playback1]);
            CheckTimes(expectedTimes2, times[playback2]);
        }

        [Test]
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
            PlaybackCurrentTimeWatcher.Instance.WatchRunningPlaybacksOnly = true;

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
        public void WatchCurrentTime_Metric()
        {
            CheckWatchCurrentTime(
                playbackLength: new MetricTimeSpan(0, 0, 1),
                pollingInterval: new MetricTimeSpan(0, 0, 0, 50),
                timeType: TimeSpanType.Metric,
                expectedTimes: Enumerable.Range(0, 21).Select(i => (MetricTimeSpan)TimeSpan.FromMilliseconds(i * 50)).ToArray());
        }

        [Test]
        public void WatchCurrentTime_BarBeat()
        {
            CheckWatchCurrentTime(
                playbackLength: new BarBeatTimeSpan(3, 0, 0),
                pollingInterval: new BarBeatTimeSpan(1, 0, 0),
                timeType: TimeSpanType.BarBeat,
                expectedTimes: new[]
                {
                    new BarBeatTimeSpan(0, 0, 0),
                    new BarBeatTimeSpan(1, 0, 0),
                    new BarBeatTimeSpan(2, 0, 1),
                    new BarBeatTimeSpan(3, 0, 1)
                });
        }

        [Test]
        public void WatchCurrentTime_Musical()
        {
            CheckWatchCurrentTime(
                playbackLength: new MusicalTimeSpan(10, 16),
                pollingInterval: new MusicalTimeSpan(1, 16),
                timeType: TimeSpanType.Musical,
                expectedTimes: Enumerable.Range(0, 11).Select(i => new MusicalTimeSpan(i, 16)).ToArray());
        }

        [Test]
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
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += (_, e) => times.Add(e.Times.First().Time);

                playback.Start();
                PlaybackCurrentTimeWatcher.Instance.Start();

                SpinWait.SpinUntil(() => !playback.IsRunning);

                WaitExpectedTimes(expectedTimes, times);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            }

            CheckTimes(expectedTimes, times);
        }

        private static void WaitExpectedTimes(ICollection<ITimeSpan> expectedTimes, ICollection<ITimeSpan> times)
        {
            var timeout = TimeSpan.FromMilliseconds(3);
            var timesReceived = SpinWait.SpinUntil(() => times.Count >= expectedTimes.Count, timeout);
            Assert.IsTrue(timesReceived, $"Times are not received for {timeout}.");
        }

        private static void CheckTimes(ICollection<ITimeSpan> expectedTimes, ICollection<ITimeSpan> actualTimes)
        {
            Assert.AreEqual(expectedTimes.Count, actualTimes.Count, "Count of times is invalid.");

            foreach (var expectedActual in expectedTimes.Zip(actualTimes, (e, a) => new { Expected = e, Actual = a }))
            {
                var expected = expectedActual.Expected;
                var actual = expectedActual.Actual;

                Assert.IsTrue(AreTimeSpansEqual(expected, actual), $"Time is invalid. Expected {expected} but received {actual}.");
            }
        }

        private static bool AreTimeSpansEqual(ITimeSpan x, ITimeSpan y)
        {
            const long microsecondsEpsilon = 3000;
            const long ticksEpsilon = 3;
            const double fractionEpsilon = 0.00001;

            if (x.GetType() != y.GetType())
                return false;

            if (x is MetricTimeSpan xMetric && y is MetricTimeSpan yMetric)
                return Math.Abs(xMetric.TotalMicroseconds - yMetric.TotalMicroseconds) < microsecondsEpsilon;

            if (x is BarBeatTimeSpan xBarBeat && y is BarBeatTimeSpan yBarBeat)
                return xBarBeat.Bars == yBarBeat.Bars &&
                       xBarBeat.Beats == yBarBeat.Beats &&
                       Math.Abs(xBarBeat.Ticks - yBarBeat.Ticks) < ticksEpsilon;

            if (x is MusicalTimeSpan xMusical && y is MusicalTimeSpan yMusical)
                return Math.Abs(xMusical.Numerator / (double)xMusical.Denominator - yMusical.Numerator / (double)yMusical.Denominator) < fractionEpsilon;

            if (x is MidiTimeSpan xMidi && y is MidiTimeSpan yMidi)
                return Math.Abs(xMidi.TimeSpan - yMidi.TimeSpan) < ticksEpsilon;

            return false;
        }

        #endregion
    }
}
