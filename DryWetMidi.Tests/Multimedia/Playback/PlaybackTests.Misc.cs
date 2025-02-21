using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class PlaybackContext
        {
            public List<ReceivedEvent> ReceivedEvents { get; } = new List<ReceivedEvent>();

            public List<SentEvent> SentEvents { get; } = new List<SentEvent>();

            public object ReceivedEventsLockObject { get; } = new object();

            public Stopwatch Stopwatch { get; } = new Stopwatch();

            public TempoMap TempoMap { get; } = PlaybackTests.TempoMap;

            public List<TimeSpan> ExpectedTimes { get; } = new List<TimeSpan>();
        }

        private sealed class OutputDeviceWithExceptionOnSendEvent : IOutputDevice
        {
            public event EventHandler<MidiEventSentEventArgs> EventSent;

            private readonly Func<Exception> _createException;

            public OutputDeviceWithExceptionOnSendEvent(Func<Exception> createException)
            {
                _createException = createException;
            }

            public void PrepareForEventsSending()
            {
            }

            public void SendEvent(MidiEvent midiEvent)
            {
                throw _createException();
            }

            public void Dispose()
            {
            }
        }

        #endregion

        #region Delegates

        private delegate void PlaybackAction(PlaybackContext context, Playback playback);

        #endregion

        #region Constants

        private const int RetriesNumber = 3;

        private static readonly PlaybackAction NoPlaybackAction = (context, playback) => { };

        private static readonly object[] ParametersForDurationCheck =
        {
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(2) },
            new object[] { TimeSpan.FromMilliseconds(500), TimeSpan.FromSeconds(3) },
            new object[] { TimeSpan.Zero, TimeSpan.FromSeconds(10) },
            new object[] { TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(12) }
        };

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvents_NoNotesPlaybackEventsWithoutNotes()
        {
            var tempoMap = TempoMap;
            var objects = new[]
            {
                new TimedEvent(new TextEvent("A")),
                new TimedEvent(new TextEvent("B")).SetTime(new MetricTimeSpan(0, 0, 2), tempoMap),
            };

            var notesPlaybackStartedFired = false;
            var notesPlaybackFinishedFired = false;

            using (var playback = new Playback(objects, tempoMap))
            {
                playback.NotesPlaybackStarted += (_, __) => notesPlaybackStartedFired = true;
                playback.NotesPlaybackFinished += (_, __) => notesPlaybackFinishedFired = true;
                playback.TrackNotes = true;

                playback.Start();
                playback.MoveToTime(new MetricTimeSpan(0, 0, 1));

                WaitOperations.Wait(() => !playback.IsRunning);
            }

            Assert.IsFalse(notesPlaybackStartedFired, "NotesPlaybackStarted fired.");
            Assert.IsFalse(notesPlaybackFinishedFired, "NotesPlaybackFinished fired.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvents_DeviceErrorOccurred()
        {
            var exceptionMessage = "AAA";
            var outputDevice = new OutputDeviceWithExceptionOnSendEvent(() => new Exception(exceptionMessage));

            using (var playback = new[] { new TextEvent() }.GetPlayback(TempoMap, outputDevice))
            {
                Exception exception = null;

                playback.DeviceErrorOccurred += (_, e) => exception = e.Exception;

                playback.Start();

                var exceptionThrown = WaitOperations.Wait(() => exception != null, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                Assert.IsTrue(exceptionThrown, "Exception was not thrown.");

                Assert.AreEqual(exceptionMessage, exception.Message, "Exception message is invalid.");
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvents_Normal()
        {
            CheckPlaybackEvents(
                expectedStartedRaised: 2,
                expectedStoppedRaised: 1,
                expectedFinishedRaised: 1,
                expectedRepeatStartedRaised: 0,
                setupPlayback: NoPlaybackAction,
                beforeChecks: NoPlaybackAction,
                afterChecks: (context, playback) =>
                {
                    Assert.IsFalse(playback.IsRunning, "Playback is running after it should be finished.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvents_Looped()
        {
            CheckPlaybackEvents(
                expectedStartedRaised: 2,
                expectedStoppedRaised: 1,
                expectedFinishedRaised: 0,
                expectedRepeatStartedRaised: 5,
                setupPlayback: (context, playback) => playback.Loop = true,
                beforeChecks: (context, playback) => WaitOperations.Wait(TimeSpan.FromSeconds(5.5)),
                afterChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after waiting.");
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_NonBlocking(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new InstrumentNameEvent("No Instrument"), TimeSpan.FromMilliseconds(200)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(3)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlayback(
                eventsToSend,
                speed,
                beforePlaybackStarted: NoPlaybackAction,
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == eventsToSend.Length - 1, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_NonBlocking_CustomPlaybackStart(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            var expectedEventsTimes = new List<TimeSpan>
            {
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1.5),
                TimeSpan.FromSeconds(1.5)
            };

            CheckPlayback(
                eventsToSend,
                speed,
                beforePlaybackStarted: (context, playback) => playback.PlaybackStart = new MetricTimeSpan(0, 0, 1, 500),
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = expectedEventsTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == expectedEventsTimes.Count, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                expectedEventsTimes: expectedEventsTimes);
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_NonBlocking_CustomPlaybackEnd(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            var expectedEventsTimes = new List<TimeSpan>
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
            };

            CheckPlayback(
                eventsToSend,
                speed,
                beforePlaybackStarted: (context, playback) => playback.PlaybackEnd = new MetricTimeSpan(0, 0, 1, 500),
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = expectedEventsTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == expectedEventsTimes.Count, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, ApplySpeedToTimeSpan(TimeSpan.FromSeconds(0.5), speed) + SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                expectedEventsTimes: expectedEventsTimes);
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_NonBlocking_CustomPlaybackStartAndEnd(double speed)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                
                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(1))
            };

            var expectedEventsTimes = new List<TimeSpan>
            {
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1.5)
            };

            CheckPlayback(
                eventsToSend,
                speed,
                beforePlaybackStarted: (context, playback) =>
                {
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 1, 500);
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 3, 500);
                },
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = expectedEventsTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == expectedEventsTimes.Count, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, ApplySpeedToTimeSpan(TimeSpan.FromSeconds(0.5), speed) + SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                expectedEventsTimes: expectedEventsTimes);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void PlayRecordedData()
        {
            var tempoMap = TempoMap;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromSeconds(5))
            };

            MidiFile recordedFile = null;

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                SendReceiveUtilities.WarmUpDevice(outputDevice);

                using (var inputDevice = InputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
                {
                    var receivedEventsNumber = 0;

                    inputDevice.StartEventsListening();
                    inputDevice.EventReceived += (_, __) => receivedEventsNumber++;

                    using (var recording = new Recording(tempoMap, inputDevice))
                    {
                        var sendingThread = new Thread(() =>
                        {
                            SendReceiveUtilities.SendEvents(eventsToSend, outputDevice);
                        });

                        recording.Start();
                        sendingThread.Start();

                        WaitOperations.Wait(() => !sendingThread.IsAlive && receivedEventsNumber == eventsToSend.Length);
                        recording.Stop();

                        recordedFile = recording.ToFile();
                    }
                }
            }

            CheckPlayback(
                eventsToSend,
                1.0,
                beforePlaybackStarted: NoPlaybackAction,
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: NoPlaybackAction,
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count == eventsToSend.Length, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    var playbackStopped = WaitOperations.Wait(() => !playback.IsRunning, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                    Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                },
                createPlayback: (outputDevice, playbackSettings) => recordedFile.GetPlayback(outputDevice, playbackSettings));
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping(int repetitionsNumber)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent(), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(2)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            CheckPlayback(
                eventsToSend,
                speed: 1.0,
                beforePlaybackStarted: (context, playback) =>
                {
                    var originalExpectedTimes = context.ExpectedTimes.ToList();

                    for (int i = 1; i < repetitionsNumber; i++)
                    {
                        var lastTime = context.ExpectedTimes.Last();
                        context.ExpectedTimes.AddRange(originalExpectedTimes.Select(t => lastTime + t));
                    }

                    playback.Loop = true;
                },
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = context.ExpectedTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count >= eventsToSend.Length * repetitionsNumber, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running.");
                    playback.Stop();
                    Assert.IsFalse(playback.IsRunning, "Playback is running after stop.");

                    lock (context.ReceivedEventsLockObject)
                    {
                        var groupedReceivedEvents = context.ReceivedEvents.GroupBy(e => e.Event, new MidiEventEqualityComparer(new MidiEventEqualityCheckSettings { CompareDeltaTimes = false })).Take(eventsToSend.Length).ToArray();
                        Assert.IsTrue(groupedReceivedEvents.All(g => g.Count() >= repetitionsNumber), $"Events are not repeated {repetitionsNumber} times.");
                    }
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackStart(int repetitionsNumber)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),

                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            var expectedEventsTimes = new List<TimeSpan>
            {
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1.5),
                TimeSpan.FromSeconds(1.5)
            };

            var originalExpectedTimes = expectedEventsTimes.ToList();

            for (int i = 1; i < repetitionsNumber; i++)
            {
                var lastTime = expectedEventsTimes.Last();
                expectedEventsTimes.AddRange(originalExpectedTimes.Select(t => lastTime + t));
            }

            CheckPlayback(
                eventsToSend,
                speed: 1.0,
                beforePlaybackStarted: (context, playback) =>
                {
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 1, 500);
                    playback.Loop = true;
                },
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = expectedEventsTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count >= originalExpectedTimes.Count * repetitionsNumber, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running.");
                    playback.Stop();
                    Assert.IsFalse(playback.IsRunning, "Playback is running after stop.");

                    lock (context.ReceivedEventsLockObject)
                    {
                        var groupedReceivedEvents = context.ReceivedEvents.GroupBy(e => e.Event, new MidiEventEqualityComparer(new MidiEventEqualityCheckSettings { CompareDeltaTimes = false })).Take(eventsToSend.Length).ToArray();
                        Assert.IsTrue(groupedReceivedEvents.All(g => g.Count() >= repetitionsNumber), $"Events are not repeated {repetitionsNumber} times.");
                    }
                },
                expectedEventsTimes: expectedEventsTimes);
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackEnd(int repetitionsNumber)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),

                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero)
            };

            var expectedEventsTimes = new List<TimeSpan>
            {
                TimeSpan.Zero,
                TimeSpan.FromSeconds(1),
            };

            var originalExpectedTimes = expectedEventsTimes.ToList();

            for (int i = 1; i < repetitionsNumber; i++)
            {
                var lastTime = expectedEventsTimes.Last() + TimeSpan.FromMilliseconds(500);
                expectedEventsTimes.AddRange(originalExpectedTimes.Select(t => lastTime + t));
            }

            CheckPlayback(
                eventsToSend,
                speed: 1.0,
                beforePlaybackStarted: (context, playback) =>
                {
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 1, 500);
                    playback.Loop = true;
                },
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = expectedEventsTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count >= originalExpectedTimes.Count * repetitionsNumber, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running.");
                    playback.Stop();
                    Assert.IsFalse(playback.IsRunning, "Playback is running after stop.");

                    lock (context.ReceivedEventsLockObject)
                    {
                        var groupedReceivedEvents = context.ReceivedEvents.GroupBy(e => e.Event, new MidiEventEqualityComparer(new MidiEventEqualityCheckSettings { CompareDeltaTimes = false })).Take(eventsToSend.Length).ToArray();
                        Assert.IsTrue(groupedReceivedEvents.All(g => g.Count() >= repetitionsNumber), $"Events are not repeated {repetitionsNumber} times.");
                    }
                },
                expectedEventsTimes: expectedEventsTimes);
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackStartAndEnd(int repetitionsNumber)
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                new EventToSend(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(1)),

                new EventToSend(new NoteOnEvent(), TimeSpan.FromSeconds(1)),
                new EventToSend(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromSeconds(1)),

                new EventToSend(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(1))
            };

            var expectedEventsTimes = new List<TimeSpan>
            {
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(1.5)
            };

            var originalExpectedTimes = expectedEventsTimes.ToList();

            for (int i = 1; i < repetitionsNumber; i++)
            {
                var lastTime = expectedEventsTimes.Last() + TimeSpan.FromMilliseconds(500);
                expectedEventsTimes.AddRange(originalExpectedTimes.Select(t => lastTime + t));
            }

            CheckPlayback(
                eventsToSend,
                speed: 1.0,
                beforePlaybackStarted: (context, playback) =>
                {
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 1, 500);
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 3, 500);
                    playback.Loop = true;
                },
                startPlayback: (context, playback) => playback.Start(),
                afterPlaybackStarted: (context, playback) =>
                {
                    Assert.LessOrEqual(context.Stopwatch.Elapsed, SendReceiveUtilities.MaximumEventSendReceiveDelay, "Playback blocks current thread.");
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after start.");
                },
                waiting: (context, playback) =>
                {
                    var timeout = expectedEventsTimes.Last() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                    var areEventsReceived = WaitOperations.Wait(() => context.ReceivedEvents.Count >= originalExpectedTimes.Count * repetitionsNumber, timeout);
                    Assert.IsTrue(areEventsReceived, $"Events are not received for timeout {timeout}.");
                },
                finalChecks: (context, playback) =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running.");
                    playback.Stop();
                    Assert.IsFalse(playback.IsRunning, "Playback is running after stop.");

                    lock (context.ReceivedEventsLockObject)
                    {
                        var groupedReceivedEvents = context.ReceivedEvents.GroupBy(e => e.Event, new MidiEventEqualityComparer(new MidiEventEqualityCheckSettings { CompareDeltaTimes = false })).Take(eventsToSend.Length).ToArray();
                        Assert.IsTrue(groupedReceivedEvents.All(g => g.Count() >= repetitionsNumber), $"Events are not repeated {repetitionsNumber} times.");
                    }
                },
                expectedEventsTimes: expectedEventsTimes);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackStop()
        {
            var stopPeriod = TimeSpan.FromSeconds(3);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(6), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(6), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(2500), p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.FromSeconds(3) + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(3) + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(6) + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(6) + stopPeriod),
                },
                setupPlayback: playback => playback.InterruptNotesOnStop = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + noteOffDelay + stopPeriod),
                },
                setupPlayback: playback => playback.TrackNotes = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void InterruptNotesOnStop_PlaybackEnd()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(100, p =>
                        p.PlaybackEnd = (MetricTimeSpan)TimeSpan.FromMilliseconds(400)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontInterruptNotesOnStop()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(2) + stopPeriod),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void DontInterruptNotesOnStop_PlaybackEnd()
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(100, p =>
                        p.PlaybackEnd = (MetricTimeSpan)TimeSpan.FromMilliseconds(400)),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                },
                setupPlayback: playback => playback.InterruptNotesOnStop = false);
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void GetCurrentTime(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(800);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter, p =>
                    {
                        p.Stop();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed), "stopped");
                    }),
                    new PlaybackChangerBase(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed), "resumed");
                    }),
                    new PlaybackChangerBase(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed), "resumed");
                    }),
                    new PlaybackChangerBase(secondAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed), "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), ScaleTimeSpan(firstEventTime, speed)),
                    new ReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(firstEventTime + lastEventTime, 1 / speed) + stopPeriod),
                },
                afterStart: playback => CheckCurrentTime(playback, TimeSpan.Zero, "started"),
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCaseSource(nameof(ParametersForDurationCheck))]
        public void GetDuration(TimeSpan start, TimeSpan delayFromStart)
        {
            var tempoMap = TempoMap;

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), start),
                new EventToSend(new NoteOffEvent(), delayFromStart)
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            using (var playback = eventsForPlayback.GetPlayback(tempoMap, outputDevice))
            {
                var duration = playback.GetDuration<MetricTimeSpan>();
                Assert.IsTrue(
                    AreTimeSpansEqual(duration, start + delayFromStart),
                    $"Duration is invalid. Actual is {duration}. Expected is {start + delayFromStart}.");
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void ChangeOutputDeviceDuringPlayback()
        {
            var tempoMap = TempoMap;
            var stopwatch = new Stopwatch();

            var changeDeviceAfter = TimeSpan.FromSeconds(1);
            var firstEventDelay = TimeSpan.Zero;
            var secondEventDelay = TimeSpan.FromSeconds(2);

            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), firstEventDelay),
                new EventToSend(new NoteOffEvent(), secondEventDelay)
            };

            var eventsForPlayback = GetEventsForPlayback(eventsToSend, tempoMap);

            var sentEventsA = new List<SentEvent>();
            var sentEventsB = new List<SentEvent>();

            var receivedEventsA = new List<ReceivedEvent>();
            var receivedEventsB = new List<ReceivedEvent>();

            using (var outputDeviceA = OutputDevice.GetByName(MidiDevicesNames.DeviceA))
            {
                outputDeviceA.EventSent += (_, e) => sentEventsA.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var inputDeviceA = InputDevice.GetByName(MidiDevicesNames.DeviceA))
                {
                    inputDeviceA.StartEventsListening();
                    inputDeviceA.EventReceived += (_, e) => receivedEventsA.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                    using (var outputDeviceB = OutputDevice.GetByName(MidiDevicesNames.DeviceB))
                    {
                        outputDeviceB.EventSent += (_, e) => sentEventsB.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                        using (var inputDeviceB = InputDevice.GetByName(MidiDevicesNames.DeviceB))
                        {
                            inputDeviceB.StartEventsListening();
                            inputDeviceB.EventReceived += (_, e) => receivedEventsB.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

                            using (var playback = eventsForPlayback.GetPlayback(tempoMap))
                            {
                                Assert.IsNull(playback.OutputDevice, "Output device is not null on playback created.");

                                playback.OutputDevice = outputDeviceA;
                                Assert.AreSame(outputDeviceA, playback.OutputDevice, "Output device was not changed to Device A.");

                                playback.Start();
                                stopwatch.Start();

                                WaitOperations.Wait(changeDeviceAfter);

                                playback.OutputDevice = outputDeviceB;
                                Assert.AreSame(outputDeviceB, playback.OutputDevice, "Output device was not changed to Device B.");

                                var playbackStopped = WaitOperations.Wait(
                                    () => !playback.IsRunning && (receivedEventsA.Count + receivedEventsB.Count) == eventsToSend.Length,
                                    firstEventDelay + secondEventDelay + SendReceiveUtilities.MaximumEventSendReceiveDelay);
                                Assert.IsTrue(playbackStopped, "Playback is running after completed.");
                            }
                        }
                    }
                }
            }

            CompareSentReceivedEvents(sentEventsA, receivedEventsA, new[] { eventsToSend.First() });
            CompareSentReceivedEvents(sentEventsB, receivedEventsB, new[] { eventsToSend.Last() });
        }

        #endregion
    }
}
