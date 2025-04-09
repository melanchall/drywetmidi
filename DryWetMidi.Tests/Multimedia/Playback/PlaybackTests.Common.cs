using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Common;
using System.Diagnostics;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class PlaybackAction
        {
            public PlaybackAction(
                TimeSpan period,
                Action<Playback> action)
                : this((int)period.TotalMilliseconds, action)
            {
            }

            public PlaybackAction(
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

        #endregion

        #region Constants

        private static readonly TempoMap TempoMap = new TempoMap(new TicksPerQuarterNoteTimeDivision(500));

        #endregion

        #region Private methods

        private void CheckPlayback(
            bool useOutputDevice,
            ICollection<ITimedObject> initialPlaybackObjects,
            PlaybackAction[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null,
            int? repeatsCount = null,
            PlaybackSettings playbackSettings = null,
            TimeSpan? sendReceiveTimeDelta = null) => CheckPlayback(
                useOutputDevice: useOutputDevice,
                createPlayback: outpuDevice => new Playback(initialPlaybackObjects, TempoMap, outpuDevice, playbackSettings),
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: setupPlayback,
                afterStart: afterStart,
                repeatsCount: repeatsCount,
                sendReceiveTimeDelta: sendReceiveTimeDelta);

        private void CheckPlayback(
            bool useOutputDevice,
            Func<IOutputDevice, Playback> createPlayback,
            PlaybackAction[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null,
            int? repeatsCount = null,
            Action<Playback> additionalChecks = null,
            TimeSpan? sendReceiveTimeDelta = null)
        {
            var outputDevice = useOutputDevice
                ? (IOutputDevice)OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName)
                : TestDeviceManager.GetOutputDevice(SendReceiveUtilities.DeviceToTestOnName);

            var stopwatch = new Stopwatch();
            var receivedEvents = new List<ReceivedEvent>();

            var actionTimes = Enumerable
                .Range(0, actions.Length)
                .Select(i => actions.Take(i + 1).Sum(a => a.PeriodMs))
                .ToArray();

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

                    for (var i = 0; i < actions.Length; i++)
                    {
                        while (stopwatch.ElapsedMilliseconds < actionTimes[i])
                        {
                        }

                        actions[i].Action(playback);
                        actionsExecutedCount++;
                    }

                    var timeout = TimeSpan.FromSeconds(30);
                    var stopped = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                    ClassicAssert.IsTrue(stopped, $"Playback is running after {timeout}.");

                    WaitOperations.Wait(SendReceiveUtilities.MaximumEventSendReceiveDelay);

                    ClassicAssert.AreEqual(
                        actions.Length,
                        actionsExecutedCount,
                        "Invalid number of actions executed.");

                    CheckReceivedEvents(
                        receivedEvents,
                        expectedReceivedEvents.ToList(),
                        sendReceiveTimeDelta: sendReceiveTimeDelta ?? (useOutputDevice
                            ? SendReceiveUtilities.MaximumEventSendReceiveDelay
                            : TimeSpan.FromMilliseconds(10)));

                    additionalChecks?.Invoke(playback);
                }
            }
        }

        private void CheckPlaybackEvents(
            int expectedStartedRaised,
            int expectedStoppedRaised,
            int expectedFinishedRaised,
            int expectedRepeatStartedRaised,
            Action<Playback> setupPlayback = null,
            Action<Playback> beforeChecks = null,
            Action<Playback> afterChecks = null)
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

            using (var playback = playbackEvents.GetPlayback(TempoMap))
            {
                setupPlayback?.Invoke(playback);

                playback.Started += (sender, args) => started++;
                playback.Stopped += (sender, args) => stopped++;
                playback.Finished += (sender, args) => finished++;
                playback.RepeatStarted += (sender, args) => repeatStarted++;

                playback.Start();
                playback.Stop();
                playback.Start();

                beforeChecks?.Invoke(playback);

                ClassicAssert.IsTrue(
                    WaitOperations.Wait(() => started == expectedStartedRaised && stopped == expectedStoppedRaised && finished == expectedFinishedRaised && repeatStarted == expectedRepeatStartedRaised, TimeSpan.FromSeconds(2)),
                    "Playback events are raised invalid number of times.");

                afterChecks?.Invoke(playback);
            }
        }

        private void CheckReceivedEvents(
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<ReceivedEvent> expectedReceivedEvents,
            TimeSpan? sendReceiveTimeDelta = null)
        {
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

            var actualEventsString = $"Actual events:{Environment.NewLine}{string.Join(Environment.NewLine, receivedEvents)}";

            CollectionAssert.IsEmpty(
                notReceivedEvents,
                $"Following events was not received:{Environment.NewLine}{string.Join(Environment.NewLine, notReceivedEvents)}{Environment.NewLine}" +
                actualEventsString);

            CollectionAssert.IsEmpty(
                actualEventsList,
                $"Following events are unexpectedly received:{Environment.NewLine}{string.Join(Environment.NewLine, actualEventsList)}{Environment.NewLine}" +
                actualEventsString);
        }

        private static void CheckCurrentTime(Playback playback, TimeSpan expectedCurrentTime, string afterPlaybackAction)
        {
            TimeSpan currentTime = (MetricTimeSpan)playback.GetCurrentTime(TimeSpanType.Metric);
            var epsilon = TimeSpan.FromMilliseconds(15);
            var delta = (currentTime - expectedCurrentTime).Duration();
            ClassicAssert.IsTrue(
                delta <= epsilon,
                $"Current time ({currentTime}) is invalid (expected is {expectedCurrentTime}): {afterPlaybackAction}.");
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
