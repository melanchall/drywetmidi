using NUnit.Framework;
using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Linq;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class PlaybackChangerBase
        {
            public PlaybackChangerBase(
                TimeSpan period,
                Action<Playback> action)
                : this((int)period.TotalMilliseconds, action)
            {
            }

            public PlaybackChangerBase(
                int periodMs,
                Action<Playback> action)
            {
                PeriodMs = periodMs;
                Action = action;
            }

            public int PeriodMs { get; }

            public Action<Playback> Action { get; }

            public override string ToString() =>
                $"After {PeriodMs} ms";
        }

        private sealed class PlaybackChanger
        {
            public PlaybackChanger(
                int periodMs,
                Action<Playback, ObservableTimedObjectsCollection> action)
            {
                PeriodMs = periodMs;
                Action = action;
            }

            public int PeriodMs { get; }

            public Action<Playback, ObservableTimedObjectsCollection> Action { get; }

            public override string ToString() =>
                $"After {PeriodMs} ms";
        }

        private sealed class EmptyOutputDevice : IOutputDevice
        {
            public event EventHandler<MidiEventSentEventArgs> EventSent;

            public void PrepareForEventsSending()
            {
            }

            public void SendEvent(MidiEvent midiEvent)
            {
                EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
            }

            public void Dispose()
            {
            }
        }

        #endregion

        #region Constants

        private const int OnTheFlyChecksRetriesNumber = 5;

        #endregion

        #region Private methods

        private void CheckPlaybackDataChangesOnTheFly(
            ICollection<ITimedObject> initialObjects,
            PlaybackChanger[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            int? repeatsCount = null)
        {
            var collection = new ObservableTimedObjectsCollection(initialObjects);

            CheckPlayback(
                useOutputDevice: false,
                createPlayback: outputDevice => new Playback(collection, TempoMap, outputDevice),
                actions: actions
                    .Select(a => new PlaybackChangerBase(a.PeriodMs, p => a.Action(p, collection)))
                    .ToArray(),
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: setupPlayback,
                repeatsCount: repeatsCount);
        }

        private void CheckDuration(
            TimeSpan expectedDuration,
            Playback playback) =>
            Assert.Less(
                (expectedDuration - (TimeSpan)playback.GetDuration<MetricTimeSpan>()).Duration(),
                TimeSpan.FromMilliseconds(4),
                "Invalid duration after note added.");

        #endregion
    }
}
