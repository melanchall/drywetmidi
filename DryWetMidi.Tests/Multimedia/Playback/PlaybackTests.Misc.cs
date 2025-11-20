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
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Melanchall.DryWetMidi.Tools;
using Melanchall.DryWetMidi.Tests.Utilities;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

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

        #region Constants

        private const int RetriesNumber = 5;

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
        public void CheckPlaybackEvent_ThrowException_Started()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new TextEvent("A")).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.Started += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.Started += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 1, PlaybackErrorSite.Started, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK" }, normalData, "Normal data is invalid.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvent_ThrowException_Stopped()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new TextEvent("A")).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(300), p =>
                    {
                        p.Stop();
                        p.Start();
                    })
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.Stopped += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.Stopped += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 1, PlaybackErrorSite.Stopped, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK" }, normalData, "Normal data is invalid.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvent_ThrowException_Finished()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new TextEvent("A")).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.Finished += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.Finished += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 1, PlaybackErrorSite.Finished, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK" }, normalData, "Normal data is invalid.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvent_ThrowException_EventPlayed()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new TextEvent("A")).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.EventPlayed += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.EventPlayed += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 2, PlaybackErrorSite.EventPlayed, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK", "OK" }, normalData, "Normal data is invalid.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvent_ThrowException_NotesPlaybackStarted()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.NotesPlaybackStarted += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.NotesPlaybackStarted += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 1, PlaybackErrorSite.NotesPlaybackStarted, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK" }, normalData, "Normal data is invalid.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvent_ThrowException_NotesPlaybackFinished()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(0), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.FromMilliseconds(0)),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(300)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.NotesPlaybackFinished += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.NotesPlaybackFinished += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 1, PlaybackErrorSite.NotesPlaybackFinished, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK" }, normalData, "Normal data is invalid.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEvent_ThrowException_RepeatStarted()
        {
            var errorOccurredData = new List<PlaybackErrorOccurredEventArgs>();
            var errorMessage = "FAIL!";

            var normalData = new List<string>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)33)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new TextEvent("A")).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)33), TimeSpan.FromMilliseconds(600)),
                    new SentReceivedEvent(new TextEvent("A"), TimeSpan.FromMilliseconds(800)),
                },
                setupPlayback: playback =>
                {
                    errorOccurredData.Clear();
                    normalData.Clear();

                    playback.RepeatStarted += (_, __) => throw new InvalidOperationException(errorMessage);
                    playback.RepeatStarted += (_, __) => normalData.Add("OK");
                    playback.ErrorOccurred += (s, e) => errorOccurredData.Add(e);
                },
                additionalChecks: (p, e) =>
                {
                    CheckErrors(errorOccurredData, 1, PlaybackErrorSite.RepeatStarted, errorMessage);
                    CollectionAssert.AreEqual(new[] { "OK" }, normalData, "Normal data is invalid.");
                },
                repeatsCount: 1);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackEventsOrder_FromFile([Values(1, 10, 100)] int tailSize)
        {
            var midiFile = new MidiFile(
                new TrackChunk(new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteAftertouchEvent(),
                    new NoteOffEvent(),
                    new PitchBendEvent(),
                }
                .Concat(Enumerable
                    .Range(0, tailSize)
                    .SelectMany(i => new MidiEvent[]
                    {
                        new ControlChangeEvent(),
                        new ChannelAftertouchEvent(),
                    }))));

            CheckPlayback(
                useOutputDevice: false,
                createPlayback: outputDevice => midiFile.GetPlayback(outputDevice),
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteAftertouchEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new PitchBendEvent(), TimeSpan.Zero),
                }
                .Concat(Enumerable
                    .Range(0, tailSize)
                    .SelectMany(i => new[]
                    {
                        new SentReceivedEvent(new ControlChangeEvent(), TimeSpan.Zero),
                        new SentReceivedEvent(new ChannelAftertouchEvent(), TimeSpan.Zero),
                    }))
                .Concat(new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.Zero),
                })
                .ToArray(),
                additionalChecks: (playback, receivedEvents) => CollectionAssert.AreEqual(
                    new[]
                    {
                        MidiEventType.NoteAftertouch,
                        MidiEventType.PitchBend,
                    }
                    .Concat(Enumerable
                        .Range(0, tailSize)
                        .SelectMany(i => new[]
                        {
                            MidiEventType.ControlChange,
                            MidiEventType.ChannelAftertouch,
                        }))
                    .Concat(new[]
                    {
                        MidiEventType.NoteOn,
                        MidiEventType.NoteOff,
                    })
                    .ToArray(),
                    receivedEvents.Select(e => e.Event.EventType).ToArray(),
                    "Invalid order of received events."));
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackOnUnsortedObjects()
        {
            var timedObjects = new ITimedObject[]
            {
                new TimedEvent(new NoteOffEvent(), 500),
                new TimedEvent(new NoteOffEvent(), 100),
                new TimedEvent(new TextEvent("A"), 50),
                new TimedEvent(new NoteOnEvent(), 300),
                new TimedEvent(new NoteOnEvent(), 0),
            };

            var notesStarted = new List<Note>();

            using (var playback = new Playback(timedObjects, TempoMap.Default))
            {
                playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.OriginalNotes);

                playback.Start();

                var timeout = TimeSpan.FromSeconds(10);
                var stopped = WaitOperations.Wait(
                    () => !playback.IsRunning,
                    timeout);

                ClassicAssert.IsTrue(stopped, $"Playback is running after {timeout}.");
            }

            MidiAsserts.AreEqual(
                new[]
                {
                    new Note(SevenBitNumber.MinValue, 100, 0) { Velocity = SevenBitNumber.MinValue },
                    new Note(SevenBitNumber.MinValue, 200, 300) { Velocity = SevenBitNumber.MinValue },
                },
                notesStarted,
                "Invalid notes started.");
        }

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

            ClassicAssert.IsFalse(notesPlaybackStartedFired, "NotesPlaybackStarted fired.");
            ClassicAssert.IsFalse(notesPlaybackFinishedFired, "NotesPlaybackFinished fired.");
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

                playback.ErrorOccurred += (_, e) => exception = e.Exception;

                playback.Start();

                var exceptionThrown = WaitOperations.Wait(() => exception != null, SendReceiveUtilities.MaximumEventSendReceiveDelay);
                ClassicAssert.IsTrue(exceptionThrown, "Exception was not thrown.");

                ClassicAssert.AreEqual(exceptionMessage, exception.Message, "Exception message is invalid.");
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
                afterChecks: playback =>
                {
                    ClassicAssert.IsFalse(playback.IsRunning, "Playback is running after it should be finished.");
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
                setupPlayback: playback => playback.Loop = true,
                beforeChecks: playback => WaitOperations.Wait(TimeSpan.FromSeconds(5.5)),
                afterChecks: playback =>
                {
                    ClassicAssert.IsTrue(playback.IsRunning, "Playback is not running after waiting.");
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback(double speed)
        {
            var playbackEvents = new[]
            {
                new TimedEvent(new InstrumentNameEvent("No Instrument"))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1200), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1200), TempoMap),
            };

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: playbackEvents,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: playbackEvents
                    .Select(e => new SentReceivedEvent(
                        e.Event,
                        ApplySpeedToTimeSpan((TimeSpan)e.TimeAs<MetricTimeSpan>(TempoMap), speed)))
                    .ToArray(),
                setupPlayback: playback => playback.Speed = speed);
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_CustomPlaybackStart(double speed)
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new SentReceivedEvent(new NoteOffEvent(),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(1500), speed)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(1500), speed)),
                },
                setupPlayback: playback =>
                {
                    playback.Speed = speed;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 1, 500);
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_CustomPlaybackEnd(double speed)
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, ApplySpeedToTimeSpan(TimeSpan.Zero, speed)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, ApplySpeedToTimeSpan(TimeSpan.FromSeconds(1), speed)),
                },
                setupPlayback: playback =>
                {
                    playback.Speed = speed;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 1, 500);
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void CheckPlayback_CustomPlaybackStartAndEnd(double speed)
        {
            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(2), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(3), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                        .SetTime((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap),
                },
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new SentReceivedEvent(new NoteOffEvent(),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(1500), speed)),
                },
                setupPlayback: playback =>
                {
                    playback.Speed = speed;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 1, 500);
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 3, 500);
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void PlayRecordedData()
        {
            var eventsToSend = new[]
            {
                new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                new EventToSend(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                new EventToSend(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromSeconds(2))
            };

            MidiFile recordedFile = null;

            var inputDevice = TestDeviceManager.GetInputDevice("A");
            var outputDevice = TestDeviceManager.GetOutputDevice("A");

            var receivedEventsNumber = 0;

            inputDevice.StartEventsListening();
            inputDevice.EventReceived += (_, __) => receivedEventsNumber++;

            using (var recording = new Recording(TempoMap, inputDevice))
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

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: recordedFile.GetTimedEvents().ToArray(),
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromSeconds(2.5))
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping(int repeatsCount)
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(800), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(800), TempoMap),
            };

            var lastTime = (TimeSpan)playbackObjects.Last().TimeAs<MetricTimeSpan>(TempoMap);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: playbackObjects,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: Enumerable
                    .Range(0, repeatsCount + 1)
                    .SelectMany(i => playbackObjects
                        .Select(obj => new SentReceivedEvent(
                            obj.Event,
                            (TimeSpan)obj.TimeAs<MetricTimeSpan>(TempoMap) + ScaleTimeSpan(lastTime, i))))
                    .ToArray(),
                repeatsCount: repeatsCount);
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackStart(int repeatsCount)
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
            };

            var lastTime = (TimeSpan)playbackObjects.Last().TimeAs<MetricTimeSpan>(TempoMap);
            var playbackStart = new MetricTimeSpan(0, 0, 0, 400);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: playbackObjects,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: Enumerable
                    .Range(0, repeatsCount + 1)
                    .SelectMany(i => playbackObjects
                        .SkipWhile(obj => obj.TimeAs<MetricTimeSpan>(TempoMap) < playbackStart)
                        .Select(obj => new SentReceivedEvent(
                            obj.Event,
                            (TimeSpan)(obj.TimeAs<MetricTimeSpan>(TempoMap) - playbackStart) + ScaleTimeSpan(lastTime - (TimeSpan)playbackStart, i))))
                    .ToArray(),
                repeatsCount: repeatsCount,
                setupPlayback: playback => playback.PlaybackStart = playbackStart);
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackEnd(int repeatsCount)
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
            };

            var lastTime = (TimeSpan)playbackObjects.Last().TimeAs<MetricTimeSpan>(TempoMap);
            var playbackEnd = new MetricTimeSpan(0, 0, 0, 400);

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: playbackObjects,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: Enumerable
                    .Range(0, repeatsCount + 1)
                    .SelectMany(i => playbackObjects
                        .TakeWhile(obj => obj.TimeAs<MetricTimeSpan>(TempoMap) <= playbackEnd)
                        .Select(obj => new SentReceivedEvent(
                            obj.Event,
                            (TimeSpan)obj.TimeAs<MetricTimeSpan>(TempoMap) + ScaleTimeSpan((TimeSpan)playbackEnd, i))))
                    .ToArray(),
                repeatsCount: repeatsCount,
                setupPlayback: playback => playback.PlaybackEnd = playbackEnd);
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackStartAndEnd_1(int repeatsCount)
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap),
            };

            var lastTime = (TimeSpan)playbackObjects.Last().TimeAs<MetricTimeSpan>(TempoMap);
            var playbackStart = new MetricTimeSpan(0, 0, 0, 400);
            var playbackEnd = new MetricTimeSpan(0, 0, 0, 800);

            var takenObject = playbackObjects
                .SkipWhile(obj => obj.TimeAs<MetricTimeSpan>(TempoMap) < playbackStart)
                .TakeWhile(obj => obj.TimeAs<MetricTimeSpan>(TempoMap) <= playbackEnd);
            var windowSize = playbackEnd - playbackStart;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: playbackObjects,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: Enumerable
                    .Range(0, repeatsCount + 1)
                    .SelectMany(i => takenObject
                        .Select(obj => new SentReceivedEvent(
                            obj.Event,
                            (TimeSpan)(obj.TimeAs<MetricTimeSpan>(TempoMap) - playbackStart) + ScaleTimeSpan((TimeSpan)windowSize, i))))
                    .ToArray(),
                repeatsCount: repeatsCount,
                setupPlayback: playback =>
                {
                    playback.PlaybackStart = playbackStart;
                    playback.PlaybackEnd = playbackEnd;
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOnEventsForActiveNotes = true;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(5)]
        public void CheckPlaybackLooping_CustomPlaybackStartAndEnd_2(int repeatsCount)
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 })
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new MarkerEvent("A"))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                new TimedEvent(new MarkerEvent("B"))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap),
            };

            var lastTime = (TimeSpan)playbackObjects.Last().TimeAs<MetricTimeSpan>(TempoMap);
            var playbackStart = new MetricTimeSpan(0, 0, 0, 400);
            var playbackEnd = new MetricTimeSpan(0, 0, 0, 800);

            var takenObject = playbackObjects
                .SkipWhile(obj => obj.TimeAs<MetricTimeSpan>(TempoMap) < playbackStart)
                .TakeWhile(obj => obj.TimeAs<MetricTimeSpan>(TempoMap) <= playbackEnd);
            var windowSize = playbackEnd - playbackStart;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: playbackObjects,
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: Enumerable
                    .Range(0, repeatsCount + 1)
                    .SelectMany(i => takenObject
                        .Select(obj => new SentReceivedEvent(
                            obj.Event,
                            (TimeSpan)(obj.TimeAs<MetricTimeSpan>(TempoMap) - playbackStart) + ScaleTimeSpan((TimeSpan)windowSize, i))))
                    .ToArray(),
                repeatsCount: repeatsCount,
                setupPlayback: playback =>
                {
                    playback.PlaybackStart = playbackStart;
                    playback.PlaybackEnd = playbackEnd;
                    playback.InterruptNotesOnStop = false;
                });
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
                    new PlaybackAction(TimeSpan.FromMilliseconds(2500), p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, TimeSpan.FromSeconds(2)),
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.FromSeconds(3) + stopPeriod),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(3) + stopPeriod),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(6) + stopPeriod),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50), TimeSpan.FromSeconds(6) + stopPeriod),
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
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter),
                },
                setupPlayback: playback => playback.TrackNotes = false);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void InterruptNotesOnStop_SendNoteOffEventsForNonActiveNotes()
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
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter),
                    new SentReceivedEvent(new NoteOffEvent(), noteOnDelay + noteOffDelay + stopPeriod),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                });
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
                    new PlaybackAction(100, p =>
                        p.PlaybackEnd = (MetricTimeSpan)TimeSpan.FromMilliseconds(400)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(400)),
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
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(2) + stopPeriod),
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
                    new PlaybackAction(100, p =>
                        p.PlaybackEnd = (MetricTimeSpan)TimeSpan.FromMilliseconds(400)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                },
                setupPlayback: playback => playback.InterruptNotesOnStop = false);
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void GetCurrentTime(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(400);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);

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
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed), "stopped");
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed), "resumed");
                    }),
                    new PlaybackAction(firstAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed), "resumed");
                    }),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed), "resumed");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), ScaleTimeSpan(firstEventTime, speed)),
                    new SentReceivedEvent(new NoteOffEvent(), ScaleTimeSpan(firstEventTime + lastEventTime, 1 / speed) + stopPeriod),
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
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)start, TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)(start + delayFromStart), TempoMap),
            };

            using (var playback = new Playback(playbackObjects, TempoMap))
            {
                var duration = playback.GetDuration<MetricTimeSpan>();
                ClassicAssert.AreEqual(
                    start + delayFromStart,
                    (TimeSpan)duration,
                    $"Duration is invalid.");
            }
        }

        [Retry(RetriesNumber)]
        [Test]
        public void ChangeOutputDeviceDuringPlayback()
        {
            var changeDeviceAfter = TimeSpan.FromSeconds(1);
            var firstEventDelay = TimeSpan.Zero;
            var secondEventDelay = TimeSpan.FromSeconds(2);

            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)firstEventDelay, TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)(firstEventDelay + secondEventDelay), TempoMap),
            };

            var stopwatch = new Stopwatch();

            var receivedEvents1 = new List<SentReceivedEvent>();
            var outputDevice1 = TestDeviceManager.GetOutputDevice("A");
            outputDevice1.EventSent += (_, e) => receivedEvents1.Add(new SentReceivedEvent(e.Event, stopwatch.Elapsed));

            var receivedEvents2 = new List<SentReceivedEvent>();
            var outputDevice2 = TestDeviceManager.GetOutputDevice("B");
            outputDevice2.EventSent += (_, e) => receivedEvents2.Add(new SentReceivedEvent(e.Event, stopwatch.Elapsed));

            ClassicAssert.AreNotSame(
                outputDevice1,
                outputDevice2,
                "Output devices are the same.");

            using (var playback = new Playback(playbackObjects, TempoMap))
            {
                playback.OutputDevice = outputDevice1;

                playback.Start();
                stopwatch.Start();

                WaitOperations.WaitPrecisely(changeDeviceAfter);

                playback.OutputDevice = outputDevice2;

                WaitOperations.Wait(() => !playback.IsRunning);
                WaitOperations.Wait(SendReceiveUtilities.MaximumEventSendReceiveDelay);
            }

            SendReceiveUtilities.CheckReceivedEvents(
                receivedEvents1,
                new[] { new SentReceivedEvent(new NoteOnEvent(), firstEventDelay) });

            SendReceiveUtilities.CheckReceivedEvents(
                receivedEvents2,
                new[] { new SentReceivedEvent(new NoteOffEvent(), firstEventDelay + secondEventDelay) });
        }

        [Retry(RetriesNumber)]
        [TestCaseSource(nameof(GetFilesForTesting))]
        public void CheckFilePlayback(string filePath)
        {
            var midiFile = MidiFile
                .Read(filePath)
                .TakePart(new MetricTimeSpan(0, 0, 10));
            midiFile.Sanitize();

            var playbackObjects = midiFile
                .GetTimedEvents()
                .ToArray();

            var tempoMap = midiFile.GetTempoMap();

            CheckPlayback(
                useOutputDevice: false,
                createPlayback: outputDevice => midiFile.GetPlayback(outputDevice),
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: playbackObjects
                    .Select(e => new SentReceivedEvent(e.Event, e.TimeAs<MetricTimeSpan>(tempoMap)))
                    .ToArray(),
                setupPlayback: playback =>
                {
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.SendNoteOnEventsForActiveNotes = true;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CalculateTempoMap_False()
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
            };

            CheckPlayback(
                useOutputDevice: false,
                createPlayback: outputDevice => new Playback(playbackObjects, TempoMap, outputDevice),
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                },
                additionalChecks: (playback, _) =>
                {
                    ClassicAssert.AreNotSame(TempoMap, playback.TempoMap, "Tempo map is the same as the original one.");
                    ClassicAssert.AreEqual(TempoMap, playback.TempoMap, "Invalid tempo map.");
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CalculateTempoMap_True()
        {
            var playbackObjects = new[]
            {
                new TimedEvent(new NoteOnEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                new TimedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                new TimedEvent(new NoteOffEvent())
                    .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap),
            };

            CheckPlayback(
                useOutputDevice: false,
                createPlayback: outputDevice => new Playback(playbackObjects, TempoMap, outputDevice, new PlaybackSettings
                {
                    CalculateTempoMap = true,
                }),
                actions: Array.Empty<PlaybackAction>(),
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new SentReceivedEvent(new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2), TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(350)),
                },
                additionalChecks: (playback, _) =>
                {
                    ClassicAssert.AreNotEqual(TempoMap, playback.TempoMap, "Invalid tempo map.");
                    ClassicAssert.AreEqual(
                        AddTempoChanges(TempoMap, (TimeSpan.FromMilliseconds(200), new Tempo(SetTempoEvent.DefaultMicrosecondsPerQuarterNote / 2))),
                        playback.TempoMap,
                        "Invalid tempo map.");
                });
        }

        #endregion

        #region Private methods

        private static object[] GetFilesForTesting()
        {
            return TestFilesProvider
                .GetValidFilesPaths(MidiFileFormat.MultiTrack)
                .Where(f =>
                {
                    var midiFile = MidiFile.Read(f);
                    var duration = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>();
                    return duration > TimeSpan.FromSeconds(10);
                })
                .Take(5)
                .ToArray();
        }

        #endregion
    }
}
