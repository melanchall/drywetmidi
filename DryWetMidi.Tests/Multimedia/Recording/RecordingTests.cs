using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Common;
using System.Linq;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class RecordingTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        private static readonly object[] ParametersForDurationCheck =
        {
            new object[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(300) },
            new object[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(2) },
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(1) },
            new object[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }
        };

        #endregion

        #region Test methods

        [Test]
        public void StartRecording_DeviceNotListeningEvents()
        {
            using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var recording = new Recording(TempoMap.Default, inputDevice))
            {
                ClassicAssert.Throws<InvalidOperationException>(() => recording.Start(), "Recording started on device which is not listening events.");
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

            var inputDevice = TestDeviceManager.GetInputDevice("A");
            var outputDevice = TestDeviceManager.GetOutputDevice("A");

            inputDevice.StartEventsListening();
            inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

            using (var recording = new Recording(TempoMap.Default, inputDevice))
            {
                recording.Start();
                stopwatch.Start();
                SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);

                var timeout = start + delayFromStart + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count == eventsToSend.Length, timeout);
                ClassicAssert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");

                recording.Stop();
                ClassicAssert.IsFalse(recording.IsRunning, "Recording is running after stop.");

                TimeSpan duration = recording.GetDuration<MetricTimeSpan>();
                ClassicAssert.IsTrue(
                    AreTimeSpansEqual(duration, start + delayFromStart),
                    $"Duration is invalid. Actual is {duration}. Expected is {start + delayFromStart}.");
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
            var recordedEvents = new List<ReceivedEvent>();
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

            var timeout = expectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;

            var inputDevice = TestDeviceManager.GetInputDevice("A");
            var outputDevice = TestDeviceManager.GetOutputDevice("A");

            outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

            inputDevice.StartEventsListening();
            inputDevice.EventReceived += (_, e) => receivedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

            using (var recording = new Recording(tempoMap, inputDevice))
            {
                recording.EventRecorded += (_, e) => recordedEvents.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                var sendingThread = new Thread(() =>
                {
                    SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                });

                stopwatch.Start();
                recording.Start();
                sendingThread.Start();
                WaitOperations.Wait(stopAfter);

                recording.Stop();
                WaitOperations.Wait(stopPeriod);

                recording.Start();

                var threadAliveTimeout = timeout + TimeSpan.FromSeconds(30);
                var threadExited = WaitOperations.Wait(() => !sendingThread.IsAlive, threadAliveTimeout);
                ClassicAssert.IsTrue(threadExited, $"Sending thread is alive after [{threadAliveTimeout}].");

                var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count >= expectedTimes.Count, timeout);
                ClassicAssert.IsTrue(areEventsReceived, $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}).");

                CompareSentReceivedEvents(sentEvents, receivedEvents, expectedTimes);
                CompareSentReceivedEvents(sentEvents, recordedEvents, expectedTimes);

                var events = recording.GetEvents();
                CheckRecordedEvents(events.ToList(), expectedRecordedTimes, tempoMap);
            }
        }

        #endregion

        #region Private methods

        private void CompareSentReceivedEvents(
            IReadOnlyList<SentEvent> sentEvents,
            IReadOnlyList<ReceivedEvent> receivedEvents,
            IReadOnlyList<TimeSpan> expectedTimes)
        {
            ClassicAssert.AreEqual(expectedTimes.Count, receivedEvents.Count, "Received events count is invalid.");

            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedTime = expectedTimes[i];

                MidiAsserts.AreEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event [{receivedEvent.Event}] doesn't match sent one [{sentEvent.Event}].");

                var offsetFromExpectedTime = (sentEvent.Time - expectedTime).Duration();
                ClassicAssert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was sent at wrong time ({sentEvent.Time}; expected is {expectedTime}).");
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
                ClassicAssert.LessOrEqual(
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
