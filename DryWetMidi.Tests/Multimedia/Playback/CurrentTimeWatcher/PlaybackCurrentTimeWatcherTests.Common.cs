using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Common;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackCurrentTimeWatcherTests
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

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                times.Add(e.Times.First().Time);

            using (var playback = events.GetPlayback(TempoMap.Default))
            {
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Metric);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();
                WaitOperations.Wait(waitingTime + epsilon);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
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

            var playback = events.GetPlayback(TempoMap.Default);

            PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, TimeSpanType.Midi);

            playback.Start();
            PlaybackCurrentTimeWatcher.Instance.Start();

            var timeout = TimeSpan.FromSeconds(30);
            var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
            Assert.IsTrue(playbackFinished, $"Playback is not finished for {timeout}.");

            var times = new List<ITimeSpan>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                times.Add(e.Times.First().Time);

            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

            WaitOperations.Wait(waitingTime + epsilon);

            PlaybackCurrentTimeWatcher.Instance.Stop();
            PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;

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

            var playback1 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 400 } }.GetPlayback(tempoMap);
            var playback2 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 200 } }.GetPlayback(tempoMap);

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

            WaitOperations.Wait(() => !playback1.IsRunning && !playback2.IsRunning);

            try
            {
                WaitExpectedTimes(expectedTimes1, times[playback1]);
                WaitExpectedTimes(expectedTimes2, times[playback2]);
            }
            finally
            {
                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);

                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= currentTimeChangedHandler;

                playback1.Dispose();
                playback2.Dispose();
            }

            CheckTimes(expectedTimes1, times[playback1], "Playback 1.");
            CheckTimes(expectedTimes2, times[playback2], "Playback 2.");
        }

        [Test]
        [Retry(RetriesNumber)]
        public void WatchMultiplePlaybacks_RemoveOne()
        {
            var tempoMap = TempoMap.Default;

            var playback1 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 400 } }.GetPlayback(tempoMap);
            var playback2 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 500 } }.GetPlayback(tempoMap);

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

            TimeSpan removeAfter = TimeConverter.ConvertTo<MetricTimeSpan>(200, tempoMap);

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

            WaitOperations.Wait(removeAfter);
            PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);

            WaitOperations.Wait(() => !playback1.IsRunning && !playback2.IsRunning);

            try
            {
                WaitExpectedTimes(expectedTimes1, times[playback1]);
                WaitExpectedTimes(expectedTimes2, times[playback2]);
            }
            finally
            {

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);

                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= currentTimeChangedHandler;

                playback1.Dispose();
                playback2.Dispose();
            }

            CheckTimes(expectedTimes1, times[playback1]);
            CheckTimes(expectedTimes2, times[playback2]);
        }

        [Test]
        public void DisposePlaybackCurrentTimeWatcher()
        {
            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var objects = new List<object>();
            var objectsCount = 0;

            using (var playback = events.GetPlayback(TempoMap.Default))
            using (var watcher = new PlaybackCurrentTimeWatcher())
            {
                watcher.AddPlayback(playback);
                watcher.CurrentTimeChanged += (_, __) => objects.Add(new object());

                watcher.Start();
                WaitOperations.WaitPrecisely(TimeSpan.FromSeconds(1));
                watcher.Stop();

                objectsCount = objects.Count;
                Assert.Greater(objectsCount, 0, "Objects count is invalid.");
            }

            WaitOperations.Wait(TimeSpan.FromSeconds(1));
            Assert.AreEqual(objectsCount, objects.Count, "New objects added after watcher disposed.");
        }

        #endregion

        #region Private methods

        private static void CheckWatchCurrentTime(
            ITimeSpan playbackLength,
            ITimeSpan pollingInterval,
            TimeSpanType timeType,
            ICollection<ITimeSpan> expectedTimes) =>
            CheckWatchCurrentTime(
                playbackLength,
                pollingInterval,
                playback => PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, timeType),
                expectedTimes);

        private static void CheckWatchCurrentTimeWithCommonTimeType(
            ITimeSpan playbackLength,
            ITimeSpan pollingInterval,
            TimeSpanType timeType,
            ICollection<ITimeSpan> expectedTimes) =>
            CheckWatchCurrentTime(
                playbackLength,
                pollingInterval,
                playback =>
                {
                    PlaybackCurrentTimeWatcher.Instance.TimeType = timeType;
                    PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback);
                },
                expectedTimes);

        private static void CheckWatchCurrentTime(
            ITimeSpan playbackLength,
            ITimeSpan pollingInterval,
            Action<Playback> addPlayback,
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

            using (var playback = events.GetPlayback(tempoMap))
            {
                PlaybackCurrentTimeWatcher.Instance.PollingInterval = TimeConverter.ConvertTo<MetricTimeSpan>(pollingInterval, tempoMap);
                addPlayback(playback);

                EventHandler<PlaybackCurrentTimeChangedEventArgs> currentTimeChangedHandler = (_, e) => times.Add(e.Times.First().Time);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += currentTimeChangedHandler;

                playback.Start();
                PlaybackCurrentTimeWatcher.Instance.Start();

                var timeout = TimeSpan.FromSeconds(30);
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, $"Playback is not finished for {timeout}.");

                try
                {
                    WaitExpectedTimes(expectedTimes, times);
                }
                finally
                {
                    PlaybackCurrentTimeWatcher.Instance.Stop();
                    PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= currentTimeChangedHandler;
                    PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
                }
            }

            CheckTimes(expectedTimes, times);
        }

        private static void WaitExpectedTimes(ICollection<ITimeSpan> expectedTimes, ICollection<ITimeSpan> times)
        {
            var timeout = TimeSpan.FromMilliseconds(30);
            var timesReceived = WaitOperations.Wait(() => times.Count >= expectedTimes.Count, timeout);
            Assert.IsTrue(timesReceived, $"Times are not received for [{timeout}] (actual count is {times.Count} ({string.Join(", ", times)}); expected is {expectedTimes.Count} ({string.Join(", ", expectedTimes)})).");
        }

        private static void CheckTimes(ICollection<ITimeSpan> expectedTimes, ICollection<ITimeSpan> actualTimes, string message = null)
        {
            Assert.AreEqual(expectedTimes.Count, actualTimes.Count, $"Count of times is invalid. {message} (actual are {string.Join(", ", actualTimes)}, expected are {string.Join(", ", expectedTimes)})");

            foreach (var expectedActual in expectedTimes.Zip(actualTimes, (e, a) => new { Expected = e, Actual = a }))
            {
                var expected = expectedActual.Expected;
                var actual = expectedActual.Actual;

                var expectedType = expected.GetType();
                var actualType = actual.GetType();
                Assert.AreEqual(expectedType, actualType, "Types are different.");

                Assert.IsTrue(AreTimeSpansEqual(expected, actual), $"Time is invalid. Expected [{expected}] but received [{actual}]. {message}");
            }
        }

        private static bool AreTimeSpansEqual(ITimeSpan x, ITimeSpan y)
        {
            const long microsecondsEpsilon = 20000;
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

        private static Playback GetLongPlayback() =>
            new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = int.MaxValue }
            }
            .GetPlayback(TempoMap.Default);

        #endregion
    }
}
