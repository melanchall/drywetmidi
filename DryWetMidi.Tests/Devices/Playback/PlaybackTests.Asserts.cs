using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        private IEnumerable<SnapPoint> GetActiveSnapPoints(Playback playback)
        {
            var time = TimeSpan.Zero;
            var result = new List<SnapPoint>();
            SnapPoint snapPoint = null;

            while ((snapPoint = playback.Snapping.GetNextSnapPoint(time)) != null)
            {
                result.Add(snapPoint);
                time = snapPoint.Time;
            }

            return result;
        }

        private void CheckEventCallback(
            ICollection<EventToSend> eventsToSend,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            TimeSpan changeCallbackAfter,
            EventCallback eventCallback,
            EventCallback secondEventCallback)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);

                    playback.EventCallback = eventCallback;

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };

                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= changeCallbackAfter);
                        playback.EventCallback = secondEventCallback;

                        var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, timeout);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareReceivedEvents(receivedEvents, expectedReceivedEvents.ToList());
        }

        private void CheckNoteCallback(
            ICollection<EventToSend> eventsToSend,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            TimeSpan changeCallbackAfter,
            NoteCallback noteCallback,
            NoteCallback secondNoteCallback,
            ICollection<Note> expectedNotesStarted,
            ICollection<Note> expectedNotesFinished)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);

                    playback.NoteCallback = noteCallback;

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };

                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= changeCallbackAfter);
                        playback.NoteCallback = secondNoteCallback;

                        var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, timeout);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareReceivedEvents(receivedEvents, expectedReceivedEvents.ToList());

            Assert.IsTrue(NoteEquality.AreEqual(notesStarted, expectedNotesStarted), "Invalid notes started.");
            Assert.IsTrue(NoteEquality.AreEqual(notesFinished, expectedNotesFinished), "Invalid notes finished.");
        }

        private void CheckEventPlayedEvent(
            ICollection<EventToSend> eventsToSend,
            ICollection<ReceivedEvent> expectedPlayedEvents)
        {
            var playbackContext = new PlaybackContext();

            var playedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.EventPlayed += (_, e) =>
                    {
                        lock (playbackContext.ReceivedEventsLockObject)
                        {
                            playedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                        }
                    };

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        var timeout = TimeSpan.FromTicks(eventsToSend.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, timeout);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareReceivedEvents(playedEvents, expectedPlayedEvents.ToList());
        }

        private void CheckTrackNotes(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished,
            bool useOutputDevice)
        {
            if (useOutputDevice)
                CheckTrackNotesWithOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo, notesWillBeStarted, notesWillBeFinished);
            else
                CheckTrackNotesWithoutOutputDevice(eventsToSend, eventsWillBeSent, moveFrom, moveTo, notesWillBeStarted, notesWillBeFinished);
        }

        private void CheckTrackNotesWithOutputDevice(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notes = eventsForPlayback.GetNotes().ToArray();
            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };
                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= moveFrom);
                        playback.MoveToTime((MetricTimeSpan)moveTo);

                        var timeout = TimeSpan.FromTicks(eventsWillBeSent.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsWillBeSent.Count, timeout);
                        Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                        stopwatch.Stop();

                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents, eventsWillBeSent.ToList());

            Assert.IsTrue(NoteEquality.AreEqual(notesStarted, notesWillBeStarted.Select(i => notes[i])), "Invalid notes started.");
            Assert.IsTrue(NoteEquality.AreEqual(notesFinished, notesWillBeFinished.Select(i => notes[i])), "Invalid notes finished.");
        }

        private void CheckTrackNotesWithoutOutputDevice(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished)
        {
            var playbackContext = new PlaybackContext();

            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var notes = eventsForPlayback.GetNotes().ToArray();
            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            using (var playback = new Playback(eventsForPlayback, tempoMap))
            {
                playback.TrackNotes = true;
                playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);

                stopwatch.Start();
                playback.Start();

                SpinWait.SpinUntil(() => stopwatch.Elapsed >= moveFrom);
                playback.MoveToTime((MetricTimeSpan)moveTo);

                Thread.Sleep(TimeSpan.FromTicks(eventsWillBeSent.Sum(e => e.Delay.Ticks)) + SendReceiveUtilities.MaximumEventSendReceiveDelay);

                stopwatch.Stop();

                var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(playbackStopped, "Playback is running after completed.");
            }

            Assert.IsTrue(NoteEquality.AreEqual(notesStarted, notesWillBeStarted.Select(i => notes[i])), "Invalid notes started.");
            Assert.IsTrue(NoteEquality.AreEqual(notesFinished, notesWillBeFinished.Select(i => notes[i])), "Invalid notes finished.");
        }

        private void CheckPlaybackEvents(
            int expectedStartedRaised,
            int expectedStoppedRaised,
            int expectedFinishedRaised,
            int expectedRepeatStartedRaised,
            PlaybackAction setupPlayback,
            PlaybackAction beforeChecks,
            PlaybackAction afterChecks)
        {
            var started = 0;
            var stopped = 0;
            var finished = 0;
            var repeatStarted = 0;

            var playbackEvents = new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent
                {
                    DeltaTime = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, 1), TempoMap.Default)
                }
            };

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var playback = new Playback(playbackEvents, TempoMap.Default, outputDevice))
            {
                setupPlayback(null, playback);

                playback.Started += (sender, args) => started++;
                playback.Stopped += (sender, args) => stopped++;
                playback.Finished += (sender, args) => finished++;
                playback.RepeatStarted += (sender, args) => repeatStarted++;

                playback.Start();
                playback.Stop();
                playback.Start();

                beforeChecks(null, playback);

                Assert.IsTrue(
                    SpinWait.SpinUntil(() => started == expectedStartedRaised && stopped == expectedStoppedRaised && finished == expectedFinishedRaised && repeatStarted == expectedRepeatStartedRaised, TimeSpan.FromSeconds(2)),
                    "Playback events are raised invalid number of times.");

                afterChecks(null, playback);
            }
        }

        private void CheckPlayback(
            ICollection<EventToSend> eventsToSend,
            double speed,
            PlaybackAction beforePlaybackStarted,
            PlaybackAction startPlayback,
            PlaybackAction afterPlaybackStarted,
            PlaybackAction waiting,
            PlaybackAction finalChecks,
            Func<TickGenerator> createTickGeneratorCallback = null,
            Func<OutputDevice, MidiClockSettings, Playback> createPlayback = null)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = new List<MidiEvent>();
            var expectedTimes = playbackContext.ExpectedTimes;
            var currentTime = TimeSpan.Zero;

            foreach (var eventToSend in eventsToSend.Where(e => !(e.Event is MetaEvent)))
            {
                var midiEvent = eventToSend.Event.Clone();
                midiEvent.DeltaTime = LengthConverter.ConvertFrom((MetricTimeSpan)eventToSend.Delay, (MetricTimeSpan)currentTime, tempoMap);
                currentTime += eventToSend.Delay;
                eventsForPlayback.Add(midiEvent);
                expectedTimes.Add(TimeSpan.FromTicks(MathUtilities.RoundToLong(currentTime.Ticks / speed)));
            }

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                var clockSettings = createTickGeneratorCallback != null
                    ? new MidiClockSettings { CreateTickGeneratorCallback = createTickGeneratorCallback }
                    : null;

                using (var playback = createPlayback?.Invoke(outputDevice, clockSettings) ?? new Playback(eventsForPlayback, tempoMap, outputDevice, clockSettings))
                {
                    playback.Speed = speed;
                    beforePlaybackStarted(playbackContext, playback);

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };
                        inputDevice.StartEventsListening();
                        stopwatch.Start();

                        startPlayback(playbackContext, playback);
                        afterPlaybackStarted(playbackContext, playback);
                        waiting(playbackContext, playback);

                        stopwatch.Stop();

                        finalChecks(playbackContext, playback);
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents.Take(expectedTimes.Count).ToList(), receivedEvents.Take(expectedTimes.Count).ToList(), expectedTimes);
        }

        private void CheckPlaybackStop(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan stopAfter,
            TimeSpan stopPeriod,
            PlaybackAction setupPlayback,
            PlaybackAction afterStart,
            PlaybackAction afterStop,
            PlaybackAction afterResume,
            IEnumerable<Tuple<TimeSpan, PlaybackAction>> runningAfterResume = null,
            ICollection<TimeSpan> explicitExpectedTimes = null,
            double speed = 1.0,
            ICollection<ReceivedEvent> expectedReceivedEvents = null,
            ICollection<ReceivedEvent> expectedPlayedEvents = null)
        {
            var playbackContext = new PlaybackContext();

            var playedEvents = new List<ReceivedEvent>();
            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);
            var expectedTimes = playbackContext.ExpectedTimes;

            if (explicitExpectedTimes != null || expectedReceivedEvents != null || expectedPlayedEvents != null)
                expectedTimes.AddRange(explicitExpectedTimes ?? (expectedReceivedEvents?.Select(e => e.Time) ?? expectedPlayedEvents.Select(e => e.Time)));
            else
            {
                var currentTime = TimeSpan.Zero;

                foreach (var eventWillBeSent in eventsWillBeSent)
                {
                    currentTime += eventWillBeSent.Delay;
                    var scaledCurrentTime = ScaleTimeSpan(currentTime, 1.0 / speed);
                    expectedTimes.Add(currentTime > stopAfter ? scaledCurrentTime + stopPeriod : scaledCurrentTime);
                }
            }

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = new Playback(eventsForPlayback, tempoMap, outputDevice))
                {
                    playback.Speed = speed;
                    setupPlayback(playbackContext, playback);

                    if (expectedPlayedEvents != null)
                        playback.EventPlayed += (_, e) => playedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                    using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                    {
                        inputDevice.EventReceived += (_, e) =>
                        {
                            lock (playbackContext.ReceivedEventsLockObject)
                            {
                                receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));
                            }
                        };
                        inputDevice.StartEventsListening();
                        stopwatch.Start();
                        playback.Start();

                        afterStart(playbackContext, playback);

                        SpinWait.SpinUntil(() => stopwatch.Elapsed >= stopAfter);
                        playback.Stop();

                        afterStop(playbackContext, playback);

                        Thread.Sleep(stopPeriod);
                        playback.Start();

                        afterResume(playbackContext, playback);

                        if (runningAfterResume != null)
                        {
                            foreach (var check in runningAfterResume)
                            {
                                Thread.Sleep(check.Item1);
                                check.Item2(playbackContext, playback);
                            }
                        }

                        var timeout = expectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == expectedTimes.Count, timeout);
                        Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                        stopwatch.Stop();

                        var playbackStopped = SpinWait.SpinUntil(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                        Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                    }
                }
            }

            CompareSentReceivedEvents(sentEvents, receivedEvents, expectedTimes);

            if (expectedReceivedEvents != null)
                CompareReceivedEvents(receivedEvents, expectedReceivedEvents.ToList());

            if (expectedPlayedEvents != null)
                CompareReceivedEvents(playedEvents, expectedPlayedEvents.ToList());
        }

        private static IEnumerable<MidiEvent> GetEventsForPlayback(IEnumerable<EventToSend> eventsToSend, TempoMap tempoMap)
        {
            var eventsForPlayback = new List<MidiEvent>();
            var currentTime = TimeSpan.Zero;

            foreach (var eventToSend in eventsToSend)
            {
                var midiEvent = eventToSend.Event.Clone();
                midiEvent.DeltaTime = LengthConverter.ConvertFrom((MetricTimeSpan)eventToSend.Delay, (MetricTimeSpan)currentTime, tempoMap);
                currentTime += eventToSend.Delay;
                eventsForPlayback.Add(midiEvent);
            }

            return eventsForPlayback;
        }

        private void CompareSentReceivedEvents(
            IReadOnlyList<SentEvent> sentEvents,
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<EventToSend> expectedEvents)
        {
            var currentTime = TimeSpan.Zero;

            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedEvent = expectedEvents[i];
                var expectedTime = (currentTime += expectedEvent.Delay);

                MidiAsserts.AreEventsEqual(sentEvent.Event, expectedEvent.Event, false, $"Sent event {sentEvent.Event} doesn't match expected one {expectedEvent.Event}.");
                MidiAsserts.AreEventsEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event {receivedEvent.Event} doesn't match sent one {sentEvent.Event}.");

                var offsetFromExpectedTime = (sentEvent.Time - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was sent at wrong time (at {sentEvent.Time} instead of {expectedTime}).");
            }
        }

        private void CompareSentReceivedEvents(
            IReadOnlyList<SentEvent> sentEvents,
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<TimeSpan> expectedTimes)
        {
            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedTime = expectedTimes[i];

                MidiAsserts.AreEventsEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event {receivedEvent.Event} doesn't match sent one {sentEvent.Event}.");

                var offsetFromExpectedTime = (sentEvent.Time - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was sent at wrong time (at {sentEvent.Time} instead of {expectedTime}).");
            }
        }

        private void CompareReceivedEvents(
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<ReceivedEvent> expectedReceivedEvents)
        {
            Assert.AreEqual(expectedReceivedEvents.Count, receivedEvents.Count, "Received events count is invalid.");

            for (var i = 0; i < receivedEvents.Count; i++)
            {
                var receivedEvent = receivedEvents[i];
                var expectedReceivedEvent = expectedReceivedEvents[i];

                MidiAsserts.AreEventsEqual(expectedReceivedEvent.Event, receivedEvent.Event, false, $"Received event {receivedEvent.Event} doesn't match expected one {expectedReceivedEvent.Event}.");

                var expectedTime = expectedReceivedEvent.Time;
                var offsetFromExpectedTime = (receivedEvent.Time - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was received at wrong time (at {receivedEvent.Time} instead of {expectedTime}).");
            }
        }

        private static void CheckCurrentTime(Playback playback, TimeSpan expectedCurrentTime, string afterPlaybackAction)
        {
            TimeSpan currentTime = (MetricTimeSpan)playback.GetCurrentTime(TimeSpanType.Metric);
            Assert.IsTrue(
                AreTimeSpansEqual(currentTime, expectedCurrentTime),
                $"Current time ({currentTime}) is invalid after playback {afterPlaybackAction} ({expectedCurrentTime}).");
        }

        private static bool AreTimeSpansEqual(TimeSpan timeSpan1, TimeSpan timeSpan2)
        {
            var epsilon = TimeSpan.FromMilliseconds(15);
            var delta = (timeSpan1 - timeSpan2).Duration();
            return delta <= epsilon;
        }

        private static TimeSpan ScaleTimeSpan(TimeSpan timeSpan, double scaleValue)
        {
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks * scaleValue));
        }
    }
}
