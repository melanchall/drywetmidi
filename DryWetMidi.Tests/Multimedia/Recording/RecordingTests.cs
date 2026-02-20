using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
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

        [MultimediaTestRetry]
        [TestCaseSource(nameof(ParametersForDurationCheck))]
        public void GetDuration(TimeSpan start, TimeSpan delayFromStart)
        {
            var eventsToSend = new[]
            {
                new TimestampedEvent(new NoteOnEvent(), start),
                new TimestampedEvent(new NoteOffEvent(), start + delayFromStart)
            };

            var receivedEvents = new List<TimestampedEvent>();
            var stopwatch = new Stopwatch();

            var inputDevice = TestDeviceManager.GetInputDevice("A");
            var outputDevice = TestDeviceManager.GetOutputDevice("A");

            inputDevice.StartEventsListening();
            inputDevice.EventReceived += (_, e) => receivedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

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

        [MultimediaTestRetry]
        [Test]
        public void CheckRecording()
        {
            var tempoMap = TempoMap.Default;

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var eventsToSend = new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new TimestampedEvent(new TimingClockEvent(), TimeSpan.FromMilliseconds(5500))
            };

            var sentEvents = new List<TimestampedEvent>();
            var receivedEvents = new List<TimestampedEvent>();
            var recordedEvents = new List<TimestampedEvent>();
            var stopwatch = new Stopwatch();

            var expectedRecordedEvents = new[]
            {
                new TimestampedEvent(new NoteOnEvent(), TimeSpan.Zero),
                new TimestampedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new TimestampedEvent(new TimingClockEvent(), TimeSpan.FromMilliseconds(3500))
            }.ToList();

            var timeout = expectedRecordedEvents.Max(e => e.Time) + SendReceiveUtilities.MaximumEventSendReceiveDelay;

            using (var inputDevice = TestDeviceManager.GetInputDevice("A"))
            using (var outputDevice = TestDeviceManager.GetOutputDevice("A"))
            {
                outputDevice.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

                inputDevice.StartEventsListening();
                inputDevice.EventReceived += (_, e) => receivedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

                using (var recording = new Recording(tempoMap, inputDevice))
                {
                    recording.EventRecorded += (_, e) => recordedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

                    var sendingThread = new Thread(() =>
                    {
                        SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                    });

                    stopwatch.Start();
                    recording.Start();
                    sendingThread.Start();
                    WaitOperations.Wait(stopAfter);

                    recording.Stop();
                    stopwatch.Stop();
                    WaitOperations.Wait(stopPeriod);

                    recording.Start();
                    stopwatch.Start();

                    var threadAliveTimeout = timeout + TimeSpan.FromSeconds(30);
                    var threadExited = WaitOperations.Wait(() => !sendingThread.IsAlive, threadAliveTimeout);
                    ClassicAssert.IsTrue(threadExited, $"Sending thread is alive after [{threadAliveTimeout}].");

                    var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count >= expectedRecordedEvents.Count, timeout);
                    ClassicAssert.IsTrue(areEventsReceived, $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}).");

                    CompareSentReceivedEvents(sentEvents, receivedEvents, expectedRecordedEvents);
                    CompareSentReceivedEvents(sentEvents, recordedEvents, expectedRecordedEvents);

                    var events = recording.GetEvents();
                    CheckRecordedEvents(
                        events.ToList(),
                        expectedRecordedEvents.Select(e => (e.Event, e.Time)).ToList(),
                        tempoMap);
                }
            }
        }

        #endregion

        #region Private methods

        private void CompareSentReceivedEvents(
            IReadOnlyList<TimestampedEvent> sentEvents,
            IReadOnlyList<TimestampedEvent> receivedEvents,
            IReadOnlyList<TimestampedEvent> expectedRecordedEvents)
        {
            ClassicAssert.AreEqual(expectedRecordedEvents.Count, receivedEvents.Count, "Received events count is invalid.");

            for (var i = 0; i < sentEvents.Count; i++)
            {
                var sentEvent = sentEvents[i];
                var receivedEvent = receivedEvents[i];
                var expectedRecordedEvent = expectedRecordedEvents[i];

                MidiAsserts.AreEqual(sentEvent.Event, receivedEvent.Event, false, $"Received event [{receivedEvent.Event}] doesn't match sent one [{sentEvent.Event}].");
                MidiAsserts.AreEqual(expectedRecordedEvent.Event, receivedEvent.Event, false, $"Received event [{receivedEvent.Event}] doesn't match expected recorded one [{expectedRecordedEvent.Event}].");

                var offsetFromExpectedTime = (sentEvent.Time - expectedRecordedEvent.Time).Duration();
                ClassicAssert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was sent at wrong time ({sentEvent.Time}; expected is {expectedRecordedEvent.Time}).");
            }
        }

        private void CheckRecordedEvents(
            IReadOnlyList<TimedEvent> recordedEvents,
            IReadOnlyList<(MidiEvent MidiEvent, TimeSpan Time)> expectedRecordedEvents,
            TempoMap tempoMap)
        {
            for (var i = 0; i < recordedEvents.Count; i++)
            {
                var recordedEvent = recordedEvents[i];
                var expectedRecordedEvent = expectedRecordedEvents[i];

                var convertedRecordedTime = (TimeSpan)recordedEvent.TimeAs<MetricTimeSpan>(tempoMap);
                var convertedExpectedRecordedTime = expectedRecordedEvent.Time;

                var offsetFromExpectedTime = (convertedRecordedTime - convertedExpectedRecordedTime).Duration();
                ClassicAssert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Event was recorded at wrong time (at {convertedRecordedTime} instead of {convertedExpectedRecordedTime}).");
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
