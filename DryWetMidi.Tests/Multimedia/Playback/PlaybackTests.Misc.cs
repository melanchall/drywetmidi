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

        private const int RetriesNumber = 3;

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
                afterChecks: playback =>
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
                setupPlayback: playback => playback.Loop = true,
                beforeChecks: playback => WaitOperations.Wait(TimeSpan.FromSeconds(5.5)),
                afterChecks: playback =>
                {
                    Assert.IsTrue(playback.IsRunning, "Playback is not running after waiting.");
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
                    .Select(e => new ReceivedEvent(
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
                    new ReceivedEvent(new NoteOnEvent(), ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new ReceivedEvent(new NoteOffEvent(),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(1500), speed)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)50),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(1500), speed)),
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
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)20) { Channel = (FourBitNumber)5 }, ApplySpeedToTimeSpan(TimeSpan.Zero, speed)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)10) { Channel = (FourBitNumber)5 }, ApplySpeedToTimeSpan(TimeSpan.FromSeconds(1), speed)),
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
                    new ReceivedEvent(new NoteOnEvent(), ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)50),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(500), speed)),
                    new ReceivedEvent(new NoteOffEvent(),  ApplySpeedToTimeSpan(TimeSpan.FromMilliseconds(1500), speed)),
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

            var loopbackDevice = new LoopbackDeviceMock();

            var receivedEventsNumber = 0;

            loopbackDevice.Input.StartEventsListening();
            loopbackDevice.Input.EventReceived += (_, __) => receivedEventsNumber++;

            using (var recording = new Recording(TempoMap, loopbackDevice.Input))
            {
                var sendingThread = new Thread(() =>
                {
                    SendReceiveUtilities.SendEvents(eventsToSend, loopbackDevice.Output);
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
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromMilliseconds(500)),
                    new ReceivedEvent(new ProgramChangeEvent((SevenBitNumber)40), TimeSpan.FromSeconds(2.5))
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
                        .Select(obj => new ReceivedEvent(
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
                        .Select(obj => new ReceivedEvent(
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
                        .Select(obj => new ReceivedEvent(
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
        public void CheckPlaybackLooping_CustomPlaybackStartAndEnd(int repeatsCount)
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
                        .Select(obj => new ReceivedEvent(
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
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter),
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
                    new ReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter),
                    new ReceivedEvent(new NoteOffEvent(), noteOnDelay + noteOffDelay + stopPeriod),
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
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
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
                    new PlaybackAction(100, p =>
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
                Assert.AreEqual(
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

            var receivedEvents1 = new List<ReceivedEvent>();
            var outputDevice1 = new OutputDeviceMock();
            outputDevice1.EventSent += (_, e) => receivedEvents1.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

            var receivedEvents2 = new List<ReceivedEvent>();
            var outputDevice2 = new OutputDeviceMock();
            outputDevice2.EventSent += (_, e) => receivedEvents2.Add(new ReceivedEvent(e.Event, stopwatch.Elapsed));

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

            CheckReceivedEvents(
                receivedEvents1,
                new[] { new ReceivedEvent(new NoteOnEvent(), firstEventDelay) });

            CheckReceivedEvents(
                receivedEvents2,
                new[] { new ReceivedEvent(new NoteOffEvent(), firstEventDelay + secondEventDelay) });
        }

        #endregion
    }
}
