using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using System.Linq;
using System.Diagnostics;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

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

        private sealed class EventsCollectingDevice : IOutputDevice
        {
            public event EventHandler<MidiEventSentEventArgs> EventSent;

            private readonly Stopwatch _stopwatch = new Stopwatch();

            public List<ReceivedEvent> ReceivedEvents { get; } = new List<ReceivedEvent>();

            public void StartCollecting()
            {
                _stopwatch.Start();
            }

            public void PrepareForEventsSending()
            {
            }

            public void SendEvent(MidiEvent midiEvent)
            {
                ReceivedEvents.Add(new ReceivedEvent(midiEvent, _stopwatch.Elapsed));
                EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
            }

            public void Dispose()
            {
                _stopwatch.Stop();
            }
        }

        #endregion

        #region Constants

        private static readonly TempoMap OnTheFlyChecksTempoMap = new TempoMap(new TicksPerQuarterNoteTimeDivision(500));
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

            using (var outputDevice = new EventsCollectingDevice())
            using (var playback = new Playback(collection, OnTheFlyChecksTempoMap, outputDevice))
            {
                setupPlayback?.Invoke(playback);

                if (repeatsCount > 0)
                    playback.Loop = true;

                var actualRepeatsCount = 0;
                playback.RepeatStarted += (_, __) =>
                {
                    actualRepeatsCount++;
                    if (actualRepeatsCount == repeatsCount)
                        playback.Loop = false;
                };

                var actionsExecutedCount = 0;

                playback.Start();
                outputDevice.StartCollecting();

                foreach (var action in actions)
                {
                    var stopwatch = Stopwatch.StartNew();

                    while (stopwatch.ElapsedMilliseconds < action.PeriodMs)
                    {
                    }
                    
                    action.Action(playback, collection);
                    actionsExecutedCount++;
                }

                var timeout = TimeSpan.FromSeconds(10);
                var stopped = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(stopped, $"Playback is running after {timeout}.");

                WaitOperations.Wait(SendReceiveUtilities.MaximumEventSendReceiveDelay);

                Assert.AreEqual(
                    actions.Length,
                    actionsExecutedCount,
                    "Invalid number of actions executed.");

                CompareReceivedEvents(
                    outputDevice.ReceivedEvents,
                    expectedReceivedEvents.ToList(),
                    sendReceiveTimeDelta: TimeSpan.FromMilliseconds(10));
            }
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
