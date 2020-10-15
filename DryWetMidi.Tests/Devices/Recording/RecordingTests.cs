using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class RecordingTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        private static readonly object[] ParametersForDurationCheck =
        {
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(2) },
            new object[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(3) },
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(3) },
            new object[] { TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(1) }
        };

        #endregion

        #region Test methods

        [Test]
        public void StartRecording_DeviceNotListeningEvents()
        {
            using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var recording = new Recording(TempoMap.Default, inputDevice))
            {
                Assert.Throws<InvalidOperationException>(() => recording.Start(), "Recording started on device which is not listening events.");
            }
        }

        [Retry(RetriesNumber)]
        [TestCaseSource(nameof(ParametersForDurationCheck))]
        public void GetDuration(TimeSpan start, TimeSpan delayFromStart)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), start),
                new EventToSend(new NoteOffEvent(), delayFromStart)
            };

            var receivedEvents = new List<ReceivedEvent>();
            var stopwatch = new Stopwatch();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);

                using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                {
                    inputDevice.StartEventsListening();
                    inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                    using (var recording = new Recording(TempoMap.Default, inputDevice))
                    {
                        recording.Start();
                        stopwatch.Start();
                        SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);

                        var timeout = start + delayFromStart + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                        var areEventsReceived = SpinWait.SpinUntil(() => receivedEvents.Count == eventsToSend.Length, timeout);
                        Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                        recording.Stop();
                        Assert.IsFalse(recording.IsRunning, "Recording is running after stop.");

                        TimeSpan duration = recording.GetDuration<MetricTimeSpan>();
                        Assert.IsTrue(
                            AreTimeSpansEqual(duration, start + delayFromStart),
                            $"Duration is invalid. Actual is {duration}. Expected is {start + delayFromStart}.");
                    }
                }
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckRecording()
        {
            var tempoMap = TempoMap.Default;

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new TimingClockEvent(), TimeSpan.FromSeconds(5))
            };

            var sentEvents = new List<SentEvent>();
            var receivedEvents = new List<ReceivedEvent>();
            var stopwatch = new Stopwatch();

            var expectedTimes = new List<TimeSpan>();
            var expectedRecordedTimes = new List<TimeSpan>();
            var currentTime = TimeSpan.Zero;
            foreach (var eventToSend in eventsToSend)
            {
                currentTime += eventToSend.Delay;
                expectedTimes.Add(currentTime);
                expectedRecordedTimes.Add(currentTime > stopAfter ? currentTime - stopPeriod : currentTime);
            }

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                {
                    inputDevice.StartEventsListening();
                    inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                    using (var recording = new Recording(tempoMap, inputDevice))
                    {
                        var sendingThread = new Thread(() =>
                        {
                            SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                        });

                        stopwatch.Start();
                        recording.Start();
                        sendingThread.Start();
                        Thread.Sleep(stopAfter);

                        recording.Stop();
                        Thread.Sleep(stopPeriod);

                        recording.Start();

                        SpinWait.SpinUntil(() => !sendingThread.IsAlive);

                        CompareSentReceivedEvents(sentEvents, receivedEvents, expectedTimes);

                        var recordedEvents = recording.GetEvents();
                        CheckRecordedEvents(recordedEvents, expectedRecordedTimes, tempoMap);
                    }
                }
            }
        }

        #endregion

        #region Private methods

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

        private void CheckRecordedEvents(
            IReadOnlyList<TimedEvent> recordedEvents,
            IReadOnlyList<TimeSpan> expectedTimes,
            TempoMap tempoMap)
        {
            for (var i = 0; i < recordedEvents.Count; i++)
            {
                var recordedEvent = recordedEvents[i];
                var expectedTime = expectedTimes[i];

                var convertedRecordedTime = (TimeSpan)recordedEvent.TimeAs<MetricTimeSpan>(tempoMap);
                var offsetFromExpectedTime = (convertedRecordedTime - expectedTime).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was recorded at wrong time (at {convertedRecordedTime} instead of {expectedTime}).");
            }
        }

        private static bool AreTimeSpansEqual(TimeSpan timeSpan1, TimeSpan timeSpan2)
        {
            var epsilon = TimeSpan.FromMilliseconds(15);
            var delta = (timeSpan1 - timeSpan2).Duration();
            return delta <= epsilon;
        }

        #endregion
    }
}
