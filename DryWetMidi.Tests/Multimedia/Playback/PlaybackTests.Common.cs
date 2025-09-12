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
using System.IO;

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

        [OneTimeSetUp]
        public static void GlobalSetup()
        {
            var playbackTracesDirectoryPath = GetPlaybackTracesDirectoryPath();
            if (Directory.Exists(playbackTracesDirectoryPath))
                Directory.Delete(playbackTracesDirectoryPath, true);

            Directory.CreateDirectory(playbackTracesDirectoryPath);
        }

        private void CheckPlayback(
            bool useOutputDevice,
            ICollection<ITimedObject> initialPlaybackObjects,
            PlaybackAction[] actions,
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null,
            int? repeatsCount = null,
            Action<Playback, ICollection<SentReceivedEvent>> additionalChecks = null,
            PlaybackSettings playbackSettings = null,
            TimeSpan? sendReceiveTimeDelta = null,
            bool checkFromFile = true)
        {
            CheckPlayback(
                useOutputDevice: useOutputDevice,
                createPlayback: outputDevice => new Playback(initialPlaybackObjects, TempoMap, outputDevice, playbackSettings),
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: setupPlayback,
                afterStart: afterStart,
                repeatsCount: repeatsCount,
                sendReceiveTimeDelta: sendReceiveTimeDelta,
                label: "FromObjects",
                additionalChecks: additionalChecks);

            checkFromFile &= DryWetMidi.Common.Random.Instance.Next(2) == 1;

            if (!checkFromFile)
                return;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            WaitOperations.Wait(TimeSpan.FromSeconds(5));

            var midiFile = initialPlaybackObjects.ToFile();
            midiFile.ReplaceTempoMap(TempoMap);

            CheckPlayback(
                useOutputDevice: useOutputDevice,
                createPlayback: outputDevice => midiFile.GetPlayback(outputDevice, playbackSettings),
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: setupPlayback,
                afterStart: afterStart,
                repeatsCount: repeatsCount,
                sendReceiveTimeDelta: sendReceiveTimeDelta,
                label: "FromFile",
                additionalChecks: additionalChecks);
        }

        private void CheckPlayback(
            bool useOutputDevice,
            Func<IOutputDevice, Playback> createPlayback,
            PlaybackAction[] actions,
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null,
            int? repeatsCount = null,
            Action<Playback, ICollection<SentReceivedEvent>> additionalChecks = null,
            TimeSpan? sendReceiveTimeDelta = null,
            string label = null)
        {
            var outputDevice = useOutputDevice
                ? OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName)
                : TestDeviceManager.GetOutputDevice(SendReceiveUtilities.DeviceToTestOnName);

            var stopwatch = new Stopwatch();
            var delayStopwatch = new Stopwatch();
            var receivedEvents = new List<SentReceivedEvent>();

            long Measure(Action action)
            {
                var start = delayStopwatch.ElapsedMilliseconds;
                action();
                var end = delayStopwatch.ElapsedMilliseconds;
                return end - start;
            }

            var actionTimes = Enumerable
                .Range(0, actions.Length)
                .Select(i => actions.Take(i + 1).Sum(a => a.PeriodMs))
                .ToArray();

            var labelString = string.IsNullOrEmpty(label) ? string.Empty : $"{label}. ";

            using (outputDevice)
            {
                outputDevice.EventSent += (_, e) => receivedEvents.Add(new SentReceivedEvent(e.Event.Clone(), stopwatch.Elapsed));
                
                using (var playback = createPlayback(outputDevice))
                {
                    try
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
                        var delays = new Dictionary<long, long>();
                    
                        playback.Start();
#if TRACE
                        playback.ActionsTracer.SetStopwatch(stopwatch);
#endif
                        stopwatch.Start();

                        delayStopwatch.Start();
                        delays[0] = Measure(() => afterStart?.Invoke(playback));

                        for (var i = 0; i < actions.Length; i++)
                        {
                            var waitTime = actions[i].PeriodMs;
                            TracePlaybackAction(playback, $"waiting action {i} for {waitTime}...");
                            WaitOperations.WaitPrecisely(waitTime);

                            TracePlaybackAction(playback, $"performing action {i}...");
                            var actionDuration = delays[actionTimes[i]] = Measure(() => actions[i].Action(playback));
                            TracePlaybackAction(playback, $"performed action {i} for {actionDuration}");

                            actionsExecutedCount++;
                        }

                        var timeout = TimeSpan.FromSeconds(30);
                        var stopped = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                        ClassicAssert.IsTrue(stopped, $"{labelString}Playback is running after {timeout}.");

                        WaitOperations.Wait(SendReceiveUtilities.MaximumEventSendReceiveDelay);

                        ClassicAssert.AreEqual(
                            actions.Length,
                            actionsExecutedCount,
                            $"{labelString}Invalid number of actions executed.");

                        foreach (var delay in delays.OrderBy(d => d.Key))
                        {
                            foreach (var expectedReceivedEvent in expectedReceivedEvents.Where(e => (int)e.Time.TotalMilliseconds >= delay.Key))
                            {
                                expectedReceivedEvent.DelayMs += delay.Value;
                            }
                        }

                        SendReceiveUtilities.CheckReceivedEvents(
                            receivedEvents,
                            expectedReceivedEvents.ToList(),
                            sendReceiveTimeDelta: sendReceiveTimeDelta ?? (useOutputDevice
                                ? SendReceiveUtilities.MaximumEventSendReceiveDelay
                                : TimeSpan.FromMilliseconds(20)),
                            label);

                        additionalChecks?.Invoke(playback, receivedEvents);
                    }
                    catch
                    {
                        SavePlaybackTraces(
                            playback,
                            label,
                            expectedReceivedEvents,
                            receivedEvents);

                        throw;
                    }
                }
            }
        }

        [Conditional("TRACE")]
        private static void TracePlaybackAction(Playback playback, string action)
        {
#if TRACE
            playback.ActionsTracer?.TraceAction(playback.GetCurrentTime<MetricTimeSpan>(), action);
#endif
        }

        private static void CheckPlaybackEvents(
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

        private static void CheckCurrentTime(Playback playback, TimeSpan expectedCurrentTime, string afterPlaybackAction)
        {
            TracePlaybackAction(playback, $"checking current time ({afterPlaybackAction})...");

            TimeSpan currentTime = playback.GetCurrentTime<MetricTimeSpan>();
            TracePlaybackAction(playback, $"checked current time ({afterPlaybackAction}) = {currentTime}");

            var epsilon = TimeSpan.FromMilliseconds(15);
            var delta = (currentTime - expectedCurrentTime).Duration();
            ClassicAssert.IsTrue(
                delta <= epsilon,
                $"Current time ({currentTime}) is invalid (expected is {expectedCurrentTime}): {afterPlaybackAction}.");
        }

        private static TimeSpan ScaleTimeSpan(TimeSpan timeSpan, double scaleValue)
        {
            return timeSpan.MultiplyBy(scaleValue);
        }

        private static TimeSpan ApplySpeedToTimeSpan(TimeSpan timeSpan, double speed)
        {
            return timeSpan.DivideBy(speed);
        }

#endregion
    }
}
