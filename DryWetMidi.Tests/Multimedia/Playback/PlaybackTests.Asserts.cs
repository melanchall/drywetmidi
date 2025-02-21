using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Common;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Constants

        private static readonly TempoMap TempoMap = new TempoMap(new TicksPerQuarterNoteTimeDivision(500));

        #endregion

        #region Private methods

        private void CheckPlayback(
            bool useOutputDevice,
            ICollection<ITimedObject> initialPlaybackObjects,
            PlaybackChangerBase[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null,
            int? repeatsCount = null) => CheckPlayback(
                useOutputDevice,
                outpuDevice => new Playback(initialPlaybackObjects, TempoMap, outpuDevice),
                actions,
                expectedReceivedEvents,
                setupPlayback,
                afterStart,
                repeatsCount);

        private void CheckPlayback(
            bool useOutputDevice,
            Func<IOutputDevice, Playback> createPlayback,
            PlaybackChangerBase[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null,
            int? repeatsCount = null,
            Action<Playback> additionalChecks = null)
        {
            var outputDevice = useOutputDevice
                ? (IOutputDevice)OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName)
                : new EmptyOutputDevice();

            var stopwatch = new Stopwatch();
            var receivedEvents = new List<ReceivedEvent>();

            using (outputDevice)
            {
                if (useOutputDevice)
                    SendReceiveUtilities.WarmUpDevice((OutputDevice)outputDevice);

                outputDevice.EventSent += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event.Clone(), stopwatch.Elapsed));

                using (var playback = createPlayback(outputDevice))
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
                    stopwatch.Start();

                    afterStart?.Invoke(playback);

                    foreach (var action in actions)
                    {
                        var waitStopwatch = Stopwatch.StartNew();

                        while (waitStopwatch.ElapsedMilliseconds < action.PeriodMs)
                        {
                        }

                        action.Action(playback);
                        actionsExecutedCount++;
                    }

                    var timeout = TimeSpan.FromSeconds(30);
                    var stopped = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                    Assert.IsTrue(stopped, $"Playback is running after {timeout}.");

                    WaitOperations.Wait(SendReceiveUtilities.MaximumEventSendReceiveDelay);

                    Assert.AreEqual(
                        actions.Length,
                        actionsExecutedCount,
                        "Invalid number of actions executed.");

                    CompareReceivedEvents(
                        receivedEvents,
                        expectedReceivedEvents.ToList(),
                        sendReceiveTimeDelta: useOutputDevice
                            ? SendReceiveUtilities.MaximumEventSendReceiveDelay
                            : TimeSpan.FromMilliseconds(10));

                    additionalChecks?.Invoke(playback);
                }
            }
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
                    DeltaTime = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, 1), TempoMap)
                }
            };

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var playback = playbackEvents.GetPlayback(TempoMap, outputDevice))
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
                    WaitOperations.Wait(() => started == expectedStartedRaised && stopped == expectedStoppedRaised && finished == expectedFinishedRaised && repeatStarted == expectedRepeatStartedRaised, TimeSpan.FromSeconds(2)),
                    "Playback events are raised invalid number of times.");

                afterChecks(null, playback);
            }
        }

        // TODO: eliminate
        private void CheckPlayback(
            ICollection<EventToSend> eventsToSend,
            double speed,
            PlaybackAction beforePlaybackStarted,
            PlaybackAction startPlayback,
            PlaybackAction afterPlaybackStarted,
            PlaybackAction waiting,
            PlaybackAction finalChecks,
            Func<TickGenerator> createTickGeneratorCallback = null,
            Func<OutputDevice, PlaybackSettings, Playback> createPlayback = null,
            List<TimeSpan> expectedEventsTimes = null)
        {
            var playbackContext = new PlaybackContext();

            var receivedEvents = playbackContext.ReceivedEvents;
            var sentEvents = playbackContext.SentEvents;
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var eventsForPlayback = new List<MidiEvent>();
            var expectedTimes = expectedEventsTimes ?? playbackContext.ExpectedTimes;
            var currentTime = TimeSpan.Zero;

            foreach (var eventToSend in eventsToSend.Where(e => !(e.Event is MetaEvent)))
            {
                var midiEvent = eventToSend.Event.Clone();
                midiEvent.DeltaTime = LengthConverter.ConvertFrom((MetricTimeSpan)eventToSend.Delay, (MetricTimeSpan)currentTime, tempoMap);
                currentTime += eventToSend.Delay;
                eventsForPlayback.Add(midiEvent);

                if (expectedEventsTimes == null)
                    expectedTimes.Add(ApplySpeedToTimeSpan(currentTime, speed));
            }

            if (expectedEventsTimes != null)
            {
                for (var i = 0; i < expectedTimes.Count; i++)
                {
                    expectedTimes[i] = ApplySpeedToTimeSpan(expectedTimes[i], speed);
                }
            }

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                var playbackSettings = createTickGeneratorCallback != null
                    ? new PlaybackSettings { ClockSettings = new MidiClockSettings { CreateTickGeneratorCallback = createTickGeneratorCallback } }
                    : null;

                using (var playback = createPlayback?.Invoke(outputDevice, playbackSettings) ?? eventsForPlayback.GetPlayback(tempoMap, outputDevice, playbackSettings))
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

            Assert.AreEqual(expectedEvents.Count, receivedEvents.Count, "Invalid received events count.");
            Assert.AreEqual(expectedEvents.Count, sentEvents.Count, "Invalid sent events count.");

            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedEvent = expectedEvents[i];
                var expectedTime = (currentTime += expectedEvent.Delay);

                MidiAsserts.AreEqual(sentEvent.Event, expectedEvent.Event, false, $"Sent event [{sentEvent.Event}] doesn't match expected one [{expectedEvent.Event}].");
                MidiAsserts.AreEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event [{receivedEvent.Event}] doesn't match sent one [{sentEvent.Event}].");

                var offsetFromExpectedTime = (sentEvent.Time - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event '{sentEvent.Event}' was sent at wrong time ({sentEvent.Time}; expected is {expectedTime}).");
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

                MidiAsserts.AreEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event [{receivedEvent.Event}] doesn't match sent one [{sentEvent.Event}].");

                var offsetFromExpectedTime = (sentEvent.Time - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was sent at wrong time ({sentEvent.Time}; expected is {expectedTime}).");
            }
        }

        private void CompareReceivedEvents(
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<ReceivedEvent> expectedReceivedEvents,
            TimeSpan? sendReceiveTimeDelta = null)
        {
            Assert.AreEqual(
                expectedReceivedEvents.Count,
                receivedEvents.Count,
                $"Received events count is invalid.{Environment.NewLine}Actual events:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, receivedEvents) +
                    $"{Environment.NewLine}Expected events:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, expectedReceivedEvents));

            var equalityCheckSettings = new MidiEventEqualityCheckSettings { CompareDeltaTimes = false };
            var timeDelta = sendReceiveTimeDelta ?? SendReceiveUtilities.MaximumEventSendReceiveDelay;

            var actualEventsList = receivedEvents.ToList();
            var notReceivedEvents = new List<ReceivedEvent>();

            foreach (var expectedReceivedEvent in expectedReceivedEvents)
            {
                var actualEvent = actualEventsList.FirstOrDefault(e =>
                {
                    if (!MidiEvent.Equals(expectedReceivedEvent.Event, e.Event, equalityCheckSettings))
                        return false;

                    var expectedTime = expectedReceivedEvent.Time;
                    var offsetFromExpectedTime = (e.Time - expectedTime).Duration();

                    return offsetFromExpectedTime <= timeDelta;
                });

                if (actualEvent == null)
                    notReceivedEvents.Add(expectedReceivedEvent);
                else
                    actualEventsList.Remove(actualEvent);
            }

            CollectionAssert.IsEmpty(
                notReceivedEvents,
                $"Following events was not received:{Environment.NewLine}{string.Join(Environment.NewLine, notReceivedEvents)}{Environment.NewLine}" +
                $"Actual events:{Environment.NewLine}{string.Join(Environment.NewLine, receivedEvents)}");
        }

        private static void CheckCurrentTime(Playback playback, TimeSpan expectedCurrentTime, string afterPlaybackAction)
        {
            TimeSpan currentTime = (MetricTimeSpan)playback.GetCurrentTime(TimeSpanType.Metric);
            Assert.IsTrue(
                AreTimeSpansEqual(currentTime, expectedCurrentTime),
                $"Current time ({currentTime}) is invalid after playback {afterPlaybackAction} (expected is {expectedCurrentTime}).");
        }

        private static bool AreTimeSpansEqual(TimeSpan timeSpan1, TimeSpan timeSpan2)
        {
            var epsilon = SendReceiveUtilities.MaximumEventSendReceiveDelay;
            var delta = (timeSpan1 - timeSpan2).Duration();
            return delta <= epsilon;
        }

        private static TimeSpan ScaleTimeSpan(TimeSpan timeSpan, double scaleValue)
        {
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks * scaleValue));
        }

        private static TimeSpan ApplySpeedToTimeSpan(TimeSpan timeSpan, double speed)
        {
            return TimeSpan.FromTicks(MathUtilities.RoundToLong(timeSpan.Ticks / speed));
        }

        #endregion
    }
}
