using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackCurrentTimeWatcherTests
    {
        #region Test methods

        [Test]
        [Retry(RetriesNumber)]
        public void ChangeGlobalTimeType_SinglePlayback()
        {
            var changeAfter = TimeSpan.FromMilliseconds(200);
            var waitAfterChange = TimeSpan.FromMilliseconds(200);

            var oldTimeType = TimeSpanType.BarBeatFraction;
            var newTimeType = TimeSpanType.Metric;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new List<ITimeSpan>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                times.Add(e.Times.First().Time);

            using (var playback = events.GetPlayback(TempoMap.Default))
            {
                PlaybackCurrentTimeWatcher.Instance.TimeType = oldTimeType;

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();

                WaitOperations.Wait(changeAfter);
                PlaybackCurrentTimeWatcher.Instance.TimeType = newTimeType;
                WaitOperations.Wait(waitAfterChange);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            }

            Assert.IsInstanceOf<BarBeatFractionTimeSpan>(times.First(), "Invalid first time type.");
            Assert.IsInstanceOf<MetricTimeSpan>(times.Last(), "Invalid last time type.");
        }

        [Test]
        [Retry(RetriesNumber)]
        public void ChangeGlobalTimeType_MultiplePlaybacks_AllOnCommonTimeType()
        {
            var changeAfter = TimeSpan.FromMilliseconds(200);
            var waitAfterChange = TimeSpan.FromMilliseconds(200);

            var oldTimeType = TimeSpanType.BarBeatFraction;
            var newTimeType = TimeSpanType.Metric;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new Dictionary<Playback, List<ITimeSpan>>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                e.Times.ToList().ForEach(t => times[t.Playback].Add(t.Time));

            using (var playback1 = events.GetPlayback(TempoMap.Default))
            using (var playback2 = events.GetPlayback(TempoMap.Default))
            {
                times[playback1] = new List<ITimeSpan>();
                times[playback2] = new List<ITimeSpan>();

                PlaybackCurrentTimeWatcher.Instance.TimeType = oldTimeType;

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback2);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();

                WaitOperations.Wait(changeAfter);
                PlaybackCurrentTimeWatcher.Instance.TimeType = newTimeType;
                WaitOperations.Wait(waitAfterChange);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);
            }

            foreach (var t in times)
            {
                Assert.IsInstanceOf<BarBeatFractionTimeSpan>(t.Value.First(), "Invalid first time type.");
                Assert.IsInstanceOf<MetricTimeSpan>(t.Value.Last(), "Invalid last time type.");
            }
        }

        [Test]
        [Retry(RetriesNumber)]
        public void ChangeGlobalTimeType_MultiplePlaybacks_OneOnCommonTimeType()
        {
            var changeAfter = TimeSpan.FromMilliseconds(200);
            var waitAfterChange = TimeSpan.FromMilliseconds(200);

            var oldTimeType = TimeSpanType.BarBeatFraction;
            var newTimeType = TimeSpanType.Metric;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new Dictionary<Playback, List<ITimeSpan>>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                e.Times.ToList().ForEach(t => times[t.Playback].Add(t.Time));

            using (var playback1 = events.GetPlayback(TempoMap.Default))
            using (var playback2 = events.GetPlayback(TempoMap.Default))
            {
                times[playback1] = new List<ITimeSpan>();
                times[playback2] = new List<ITimeSpan>();

                PlaybackCurrentTimeWatcher.Instance.TimeType = oldTimeType;

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback2, TimeSpanType.Musical);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();

                WaitOperations.Wait(changeAfter);
                PlaybackCurrentTimeWatcher.Instance.TimeType = newTimeType;
                WaitOperations.Wait(waitAfterChange);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);
            }

            Assert.IsInstanceOf<BarBeatFractionTimeSpan>(times.First().Value.First(), "Invalid first time type on first playback.");
            Assert.IsInstanceOf<MetricTimeSpan>(times.First().Value.Last(), "Invalid last time type on first playback.");

            var singleTimeType = times.Last().Value.Select(t => t.GetType()).Distinct().Single();
            Assert.AreEqual(typeof(MusicalTimeSpan), singleTimeType, "Invalid time type on second playback.");
        }

        [Test]
        [Retry(RetriesNumber)]
        public void ChangeLocalTimeType_SinglePlayback()
        {
            var changeAfter = TimeSpan.FromMilliseconds(200);
            var waitAfterChange = TimeSpan.FromMilliseconds(200);

            var oldTimeType = TimeSpanType.BarBeatFraction;
            var newTimeType = TimeSpanType.Metric;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new List<ITimeSpan>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                times.Add(e.Times.First().Time);

            using (var playback = events.GetPlayback(TempoMap.Default))
            {

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback, oldTimeType);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();

                WaitOperations.Wait(changeAfter);
                PlaybackCurrentTimeWatcher.Instance.SetPlaybackTimeType(playback, newTimeType);
                WaitOperations.Wait(waitAfterChange);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback);
            }

            Assert.IsInstanceOf<BarBeatFractionTimeSpan>(times.First(), "Invalid first time type.");
            Assert.IsInstanceOf<MetricTimeSpan>(times.Last(), "Invalid last time type.");
        }

        [Test]
        [Retry(RetriesNumber)]
        public void ChangeLocalTimeType_MultiplePlaybacks_AllOnCommonTimeType()
        {
            var changeAfter = TimeSpan.FromMilliseconds(200);
            var waitAfterChange = TimeSpan.FromMilliseconds(200);

            var oldTimeType = TimeSpanType.BarBeatFraction;
            var newTimeType = TimeSpanType.Metric;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new Dictionary<Playback, List<ITimeSpan>>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                e.Times.ToList().ForEach(t => times[t.Playback].Add(t.Time));

            using (var playback1 = events.GetPlayback(TempoMap.Default))
            using (var playback2 = events.GetPlayback(TempoMap.Default))
            {
                times[playback1] = new List<ITimeSpan>();
                times[playback2] = new List<ITimeSpan>();

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback1, oldTimeType);
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback2, oldTimeType);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();

                WaitOperations.Wait(changeAfter);
                PlaybackCurrentTimeWatcher.Instance.SetPlaybackTimeType(playback1, newTimeType);
                PlaybackCurrentTimeWatcher.Instance.SetPlaybackTimeType(playback2, newTimeType);
                WaitOperations.Wait(waitAfterChange);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);
            }

            foreach (var t in times)
            {
                Assert.IsInstanceOf<BarBeatFractionTimeSpan>(t.Value.First(), "Invalid first time type.");
                Assert.IsInstanceOf<MetricTimeSpan>(t.Value.Last(), "Invalid last time type.");
            }
        }

        [Test]
        [Retry(RetriesNumber)]
        public void ChangeLocalTimeType_MultiplePlaybacks_OneOnCommonTimeType()
        {
            var changeAfter = TimeSpan.FromMilliseconds(200);
            var waitAfterChange = TimeSpan.FromMilliseconds(200);

            var oldTimeType = TimeSpanType.BarBeatFraction;
            var newTimeType = TimeSpanType.Metric;

            var events = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent()
            };

            var times = new Dictionary<Playback, List<ITimeSpan>>();

            EventHandler<PlaybackCurrentTimeChangedEventArgs> onCurrentTimeChanged = (_, e) =>
                e.Times.ToList().ForEach(t => times[t.Playback].Add(t.Time));

            using (var playback1 = events.GetPlayback(TempoMap.Default))
            using (var playback2 = events.GetPlayback(TempoMap.Default))
            {
                times[playback1] = new List<ITimeSpan>();
                times[playback2] = new List<ITimeSpan>();

                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback1, oldTimeType);
                PlaybackCurrentTimeWatcher.Instance.AddPlayback(playback2, TimeSpanType.Musical);
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged += onCurrentTimeChanged;

                PlaybackCurrentTimeWatcher.Instance.Start();

                WaitOperations.Wait(changeAfter);
                PlaybackCurrentTimeWatcher.Instance.SetPlaybackTimeType(playback1, newTimeType);
                WaitOperations.Wait(waitAfterChange);

                PlaybackCurrentTimeWatcher.Instance.Stop();
                PlaybackCurrentTimeWatcher.Instance.CurrentTimeChanged -= onCurrentTimeChanged;
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback1);
                PlaybackCurrentTimeWatcher.Instance.RemovePlayback(playback2);
            }

            Assert.IsInstanceOf<BarBeatFractionTimeSpan>(times.First().Value.First(), "Invalid first time type on first playback.");
            Assert.IsInstanceOf<MetricTimeSpan>(times.First().Value.Last(), "Invalid last time type on first playback.");

            var singleTimeType = times.Last().Value.Select(t => t.GetType()).Distinct().Single();
            Assert.AreEqual(typeof(MusicalTimeSpan), singleTimeType, "Invalid time type on second playback.");
        }

        #endregion
    }
}
