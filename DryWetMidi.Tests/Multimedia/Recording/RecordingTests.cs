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
using Melanchall.DryWetMidi.Tests.Attributes;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed class RecordingTests
    {
        private sealed class TimestampedInputEndpoint : IInputEndpoint
        {
            public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

            public bool IsListeningForEvents { get; private set; }

            public long CurrentTimestamp { get; set; }

            public void StartEventsListening()
            {
                IsListeningForEvents = true;
            }

            public void StopEventsListening()
            {
                IsListeningForEvents = false;
            }

            public long GetCurrentTimestamp()
            {
                return CurrentTimestamp;
            }

            public void RaiseEvent(MidiEvent midiEvent)
            {
                EventReceived?.Invoke(
                    this,
                    new MidiEventReceivedEventArgs(midiEvent, CurrentTimestamp));
            }

            public void Dispose()
            {
            }
        }

        private static readonly object[] ParametersForDurationCheck =
        {
            new object[] { TimeSpan.Zero, TimeSpan.FromMilliseconds(300) },
            new object[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(2) },
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(1) },
            new object[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2) }
        };

        [Test]
        public void StartRecording_EndpointNotListeningEvents()
        {
            using (var inputEndpoint = InputEndpoint.GetByName(SendReceiveUtilities.EndpointToTestOnName))
            using (var recording = new Recording(TempoMap.Default, inputEndpoint))
            {
                ClassicAssert.Throws<InvalidOperationException>(() => recording.Start(), "Recording started on endpoint which is not listening events.");
            }
        }

        [TimingCritical]
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

            var inputEndpoint = TestDeviceManager.GetInputEndpoint("A");
            var outputEndpoint = TestDeviceManager.GetOutputEndpoint("A");

            inputEndpoint.StartEventsListening();
            inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

            using (var recording = new Recording(TempoMap.Default, inputEndpoint))
            {
                recording.Start();
                stopwatch.Start();
                SendReceiveUtilities.SendEvents(eventsToSend, outputEndpoint);

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

        [TimingCritical]
        [Test]
        public void CheckRecording([Values(0, 100, 500)] long delayAfterStartEventsListeningMs)
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
            };

            var timeout = expectedRecordedEvents.Max(e => e.Time) + SendReceiveUtilities.MaximumEventSendReceiveDelay;

            using (var inputEndpoint = TestDeviceManager.GetInputEndpoint("A"))
            using (var outputEndpoint = TestDeviceManager.GetOutputEndpoint("A"))
            {
                outputEndpoint.EventSent += (_, e) => sentEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

                inputEndpoint.EventReceived += (_, e) => receivedEvents.Add(e.GetReceivedTimestampedEvent(stopwatch));
                inputEndpoint.StartEventsListening();

                WaitOperations.Wait(TimeSpan.FromMilliseconds(delayAfterStartEventsListeningMs));

                using (var recording = new Recording(tempoMap, inputEndpoint))
                {
                    recording.EventRecorded += (_, e) => recordedEvents.Add(new TimestampedEvent(e.Event, stopwatch.Elapsed));

                    var sendingThread = new Thread(() =>
                    {
                        SendReceiveUtilities.SendEvents(eventsToSend, outputEndpoint);
                    });

                    stopwatch.Start();
                    recording.Start();

                    var receivedEventsTimestampBaseline = inputEndpoint.GetCurrentTimestamp();
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

                    var areEventsReceived = WaitOperations.Wait(() => receivedEvents.Count >= expectedRecordedEvents.Length, timeout);
                    ClassicAssert.IsTrue(areEventsReceived, $"Events are not received for [{timeout}] (received are: {string.Join(", ", receivedEvents)}).");

                    CompareSentReceivedEvents(sentEvents, receivedEvents, expectedRecordedEvents);
                    CompareSentReceivedEvents(sentEvents, recordedEvents, expectedRecordedEvents);

                    var events = recording.GetEvents();
                    CheckRecordedEvents(
                        events.ToList(),
                        expectedRecordedEvents.Select(e => (e.Event, e.Time)).ToList(),
                        tempoMap);

                    SendReceiveUtilities.CheckReceivedEventsTimestamps(
                        eventsToSend,
                        receivedEventsTimestampBaseline,
                        receivedEvents.ToArray());
                }
            }
        }

        [TimingCritical]
        [Test]
        public void RecordingStartsFromCurrentEndpointTimestamp()
        {
            const long listeningStartedAt = 1_000_000_000L;
            const long recordingStartedAt = listeningStartedAt + 10_000_000_000L;
            const long eventReceivedAt = recordingStartedAt + 25_000_000L;

            using (var inputEndpoint = new TimestampedInputEndpoint
            {
                CurrentTimestamp = listeningStartedAt
            })
            using (var recording = new Recording(TempoMap.Default, inputEndpoint))
            {
                inputEndpoint.StartEventsListening();

                inputEndpoint.CurrentTimestamp = recordingStartedAt;
                recording.Start();

                inputEndpoint.CurrentTimestamp = eventReceivedAt;
                inputEndpoint.RaiseEvent(new NoteOnEvent());

                var recordedEvent = recording.GetEvents().Single();

                var expectedTimestamp = TimeSpan.FromMilliseconds(25);
                var actualTimestamp = (TimeSpan)recordedEvent.TimeAs<MetricTimeSpan>(TempoMap.Default);
                ClassicAssert.IsTrue(
                    AreTimeSpansEqual(expectedTimestamp, actualTimestamp),
                    $"Duration is invalid. Actual is {actualTimestamp}. Expected is {expectedTimestamp}.");
            }
        }

        [TimingCritical]
        [Test]
        public void RecordingExcludesPausedTimeAndContinuesFromPreviousActiveDuration()
        {
            using (var inputEndpoint = new TimestampedInputEndpoint
            {
                CurrentTimestamp = 1_000_000_000L
            })
            using (var recording = new Recording(TempoMap.Default, inputEndpoint))
            {
                inputEndpoint.StartEventsListening();
                recording.Start();

                inputEndpoint.CurrentTimestamp += 100_000_000L;
                inputEndpoint.RaiseEvent(new NoteOnEvent());

                inputEndpoint.CurrentTimestamp += 900_000_000L;
                recording.Stop();

                inputEndpoint.CurrentTimestamp += 5_000_000_000L;
                recording.Start();

                inputEndpoint.CurrentTimestamp += 200_000_000L;
                inputEndpoint.RaiseEvent(new NoteOffEvent());

                var recordedEvents = recording.GetEvents().ToArray();

                ClassicAssert.AreEqual(2, recordedEvents.Length);

                var expectedFirstTimestamp = TimeSpan.FromMilliseconds(100);
                var actualFirstTimestamp = (TimeSpan)recordedEvents[0].TimeAs<MetricTimeSpan>(TempoMap.Default);
                ClassicAssert.IsTrue(
                    AreTimeSpansEqual(expectedFirstTimestamp, actualFirstTimestamp),
                    $"Duration is invalid. Actual is {actualFirstTimestamp}. Expected is {expectedFirstTimestamp}.");
                
                var expectedSecondTimestamp = TimeSpan.FromMilliseconds(1200);
                var actualSecondTimestamp = (TimeSpan)recordedEvents[1].TimeAs<MetricTimeSpan>(TempoMap.Default);
                ClassicAssert.IsTrue(
                    AreTimeSpansEqual(expectedSecondTimestamp, actualSecondTimestamp),
                    $"Duration is invalid. Actual is {actualSecondTimestamp}. Expected is {expectedSecondTimestamp}.");
            }
        }

        [TimingCritical]
        [Test]
        public void RecordingIgnoresEventsReceivedWhileStoppedAndKeepsCorrectDuration()
        {
            using (var inputEndpoint = new TimestampedInputEndpoint
            {
                CurrentTimestamp = 1_000_000_000L
            })
            using (var recording = new Recording(TempoMap.Default, inputEndpoint))
            {
                inputEndpoint.StartEventsListening();
                recording.Start();

                inputEndpoint.CurrentTimestamp += 200_000_000L;
                inputEndpoint.RaiseEvent(new NoteOnEvent());

                inputEndpoint.CurrentTimestamp += 100_000_000L;
                recording.Stop();

                inputEndpoint.CurrentTimestamp += 5_000_000_000L;
                inputEndpoint.RaiseEvent(new ProgramChangeEvent());

                inputEndpoint.CurrentTimestamp += 1_000_000_000L;
                recording.Start();

                inputEndpoint.CurrentTimestamp += 100_000_000L;
                inputEndpoint.RaiseEvent(new NoteOffEvent());

                var recordedEvents = recording.GetEvents().ToArray();
                var duration = recording.GetDuration<MetricTimeSpan>();

                ClassicAssert.AreEqual(2, recordedEvents.Length);
                ClassicAssert.IsFalse(recordedEvents.Any(e => e.Event is ProgramChangeEvent));
                
                var expectedFirstTimestamp = TimeSpan.FromMilliseconds(400);
                var actualFirstTimestamp = (TimeSpan)recordedEvents[1].TimeAs<MetricTimeSpan>(TempoMap.Default);
                ClassicAssert.IsTrue(
                    AreTimeSpansEqual(expectedFirstTimestamp, actualFirstTimestamp),
                    $"Duration is invalid. Actual is {actualFirstTimestamp}. Expected is {expectedFirstTimestamp}.");
                
                var expectedDuration = TimeSpan.FromMilliseconds(400);
                var actualDuration = (TimeSpan)duration;
                ClassicAssert.IsTrue(
                    AreTimeSpansEqual(expectedDuration, actualDuration),
                    $"Duration is invalid. Actual is {actualDuration}. Expected is {expectedDuration}.");
            }
        }

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
    }
}
