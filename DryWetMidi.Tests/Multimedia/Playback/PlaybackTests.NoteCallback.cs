using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Utilities;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested enums

        private enum EventType
        {
            Started,
            Finished,
        }

        #endregion

        #region Constants

        private static readonly SevenBitNumber TransposeBy = (SevenBitNumber)20;

        private static readonly NoteCallback NoteCallback = (d, rt, rl, t) =>
        {
            return new NotePlaybackData((SevenBitNumber)(d.NoteNumber + TransposeBy), d.Velocity, d.OffVelocity, d.Channel);
        };

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnSkipNote() => CheckNoteCallback(
            initialPlaybackObjects: new[]
            {
                new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
            },
            actions: new[]
            {
                new PlaybackAction(TimeSpan.FromMilliseconds(500),
                    p => p.NoteCallback = (d, rt, rl, t) => NotePlaybackData.SkipNote),
            },
            expectedReceivedEvents: Array.Empty<ReceivedEvent>(),
            expectedNotesEvents: Array.Empty<(EventType, Note, Note, bool)>(),
            setupPlayback: playback => playback.NoteCallback = (d, rt, rl, t) => null);

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnOriginal()
        {
            var note = new Note((SevenBitNumber)100) { Velocity = (SevenBitNumber)80 }
                .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, note, false),
                    (EventType.Finished, note, note, false),
                },
                setupPlayback: playback => playback.NoteCallback = (d, rt, rl, t) => null);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnOriginal_ReturnNull()
        {
            var note = new Note(SevenBitNumber.MinValue) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => null),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(1))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, note, false),
                    (EventType.Finished, note, note, false),
                },
                setupPlayback: playback => playback.NoteCallback = (d, rt, rl, t) => d);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_Transpose()
        {
            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap);
            
            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var originalNote2 = new Note((SevenBitNumber)100) { Velocity = (SevenBitNumber)80 }
                .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap);

            var note2 = (Note)originalNote2.Clone();
            note2.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = NoteCallback),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromSeconds(1)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, false),
                    (EventType.Finished, note1, originalNote1, false),
                    (EventType.Started, note2, originalNote2, false),
                    (EventType.Finished, note2, originalNote2, false),
                },
                setupPlayback: playback => playback.NoteCallback = NoteCallback);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteOffDelay, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_SendNoteOffEventsForNonActiveNotes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteOffDelay, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_TrackNotes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteOffDelay, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveToStart(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromMilliseconds(1500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(100);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToStart();
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay, p =>
                        CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed 1")),
                    new PlaybackAction(secondAfterResumeDelay, p =>
                    {
                        ClassicAssert.IsTrue(p.IsRunning, "Playback is not running 1.");
                        p.MoveToStart();
                        CheckCurrentTime(p, TimeSpan.Zero, "resumed 2");
                        ClassicAssert.IsTrue(p.IsRunning, "Playback is not running 2.");
                    }),
                    new PlaybackAction(thirdAfterResumeDelay, p =>
                    {
                        ClassicAssert.IsTrue(p.IsRunning, "Playback is not running 3.");
                        CheckCurrentTime(p, ScaleTimeSpan(thirdAfterResumeDelay, speed), "resumed 3");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), firstEventTime),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed)),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Started, note, originalNote, false),
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveForward(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(300);

            var stepAfterStop = TimeSpan.FromMilliseconds(400);
            var stepAfterResumed = TimeSpan.FromMilliseconds(300);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(4);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)firstEventTime, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), firstEventTime),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveForward_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(10);

            var noteLength = TimeSpan.FromSeconds(4);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)noteLength, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveForward_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);

            var noteLength = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)noteLength, TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)(noteLength + TimeSpan.FromMilliseconds(200)), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteLength + TimeSpan.FromMilliseconds(300)), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromMilliseconds(450), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveBack(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(700);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            ClassicAssert.LessOrEqual(stepAfterStop, ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(100);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var lastEventTime = TimeSpan.FromMilliseconds(3000);
            ClassicAssert.GreaterOrEqual(lastEventTime, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveBack_BeyondZero()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromMilliseconds(600);

            var stepAfterStop = TimeSpan.FromMilliseconds(2000);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(300);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)lastEventTime, TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterResumed, "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), lastEventTime + stopAfter + stopPeriod + stepAfterResumed),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveBack_BeyondPlaybackStart()
        {
            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime(new MetricTimeSpan(0, 0, 0, 400), TempoMap)
                .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(200), TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                },
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(500), "A");
                        p.MoveBack(new MetricTimeSpan(0, 0, 0, 400));
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(100)),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(300)),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromMilliseconds(500))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 300);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromMilliseconds(2000);

            var stopAfter = TimeSpan.FromMilliseconds(800);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var moveTime1 = TimeSpan.FromMilliseconds(200);
            var moveTime2 = TimeSpan.FromMilliseconds(1600);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p =>
                    {
                        p.Stop();
                        p.MoveToTime((MetricTimeSpan)moveTime1);
                        CheckCurrentTime(p, moveTime1, "first move");
                    }),
                    new PlaybackAction(stopPeriod, p =>
                    {
                        p.Start();
                        CheckCurrentTime(p, moveTime1, "stopped");
                    }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, moveTime1 + firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            CheckCurrentTime(p, moveTime1 + firstAfterResumeDelay + secondAfterResumeDelay, "resumed");
                            p.MoveToTime((MetricTimeSpan)moveTime2);
                            CheckCurrentTime(p, moveTime2, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, moveTime2 + thirdAfterResumeDelay, "resumed"))
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + lastEventTime - moveTime2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 10));
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap),
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromMilliseconds(450), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, false),
                    (EventType.Finished, note, originalNote, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_ChangeCallbackDuringPlayback()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var noteLength1 = TimeSpan.FromSeconds(4);
            var noteLength2 = TimeSpan.FromSeconds(2);

            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength1, TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var note2 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)noteLength1, TempoMap)
                .SetLength((MetricTimeSpan)noteLength2, TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)noteLength1, TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteLength1, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteLength1 + noteLength2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackAction(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOnEvent(), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), noteLength1 + stopPeriod + noteLength2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, false),
                    (EventType.Finished, note1, originalNote1, false),
                    (EventType.Started, note2, note2, false),
                    (EventType.Finished, note2, note2, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_ChangeCallbackDuringPlayback_SendNoteOffEventsForNonActiveNotes()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var noteLength1 = TimeSpan.FromSeconds(4);
            var noteLength2 = TimeSpan.FromSeconds(2);

            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength1, TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var note2 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)noteLength1, TempoMap)
                .SetLength((MetricTimeSpan)noteLength2, TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)noteLength1, TempoMap),
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteLength1, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteLength1 + noteLength2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackAction(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), noteLength1 + stopPeriod + noteLength2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, false),
                    (EventType.Finished, note1, originalNote1, false),
                    (EventType.Finished, note1, originalNote1, false),
                    (EventType.Started, note2, note2, false),
                    (EventType.Finished, note2, note2, false),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnSkipNote_Notes() => CheckNoteCallback(
            initialPlaybackObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)0) { Velocity = (SevenBitNumber)0 }
                    .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap)
                    .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
            },
            actions: new[]
            {
                new PlaybackAction(TimeSpan.FromMilliseconds(500),
                    p => p.NoteCallback = (d, rt, rl, t) => NotePlaybackData.SkipNote),
            },
            expectedReceivedEvents: Array.Empty<ReceivedEvent>(),
            expectedNotesEvents: Array.Empty<(EventType, Note, Note, bool)>(),
            setupPlayback: playback => playback.NoteCallback = (d, rt, rl, t) => null);

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnOriginal_Notes()
        {
            var note1 = new Note((SevenBitNumber)0) { Velocity = (SevenBitNumber)0 }
                .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap);

            var note2 = new Note((SevenBitNumber)100) { Velocity = (SevenBitNumber)80 }
                .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    note1,
                    note2,
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note2, note2, true),
                    (EventType.Finished, note2, note2, true),
                },
                setupPlayback: playback => playback.NoteCallback = (d, rt, rl, t) => null);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnOriginal_ReturnNull_Notes()
        {
            var note1 = new Note((SevenBitNumber)0) { Velocity = (SevenBitNumber)0 }
                .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap);

            var note2 = new Note((SevenBitNumber)100) { Velocity = (SevenBitNumber)80 }
                .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    note1,
                    note2,
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => null),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(), TimeSpan.FromSeconds(1))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, note1, true),
                    (EventType.Finished, note1, note1, true),
                },
                setupPlayback: playback => playback.NoteCallback = (d, rt, rl, t) => d);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_Transpose_Notes()
        {
            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var originalNote2 = new Note((SevenBitNumber)100) { Velocity = (SevenBitNumber)80 }
                .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap)
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap);

            var note2 = (Note)originalNote2.Clone();
            note2.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    originalNote2,
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = NoteCallback),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromSeconds(1)),
                    new ReceivedEvent(new NoteOnEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)80), TimeSpan.FromMilliseconds(1500)),
                    new ReceivedEvent(new NoteOffEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)0), TimeSpan.FromMilliseconds(2000))
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, false),
                    (EventType.Finished, note1, originalNote1, false),
                    (EventType.Started, note2, originalNote2, true),
                    (EventType.Finished, note2, originalNote2, true),
                },
                setupPlayback: playback => playback.NoteCallback = NoteCallback);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_Notes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteOffDelay, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_Notes_TrackNotes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteOffDelay, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopPeriod + noteOffDelay),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_Notes_SendNoteOffEventsForNonActiveNotes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteOffDelay, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteOnDelay + stopAfter + stopPeriod + noteOffDelay - stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveToStart_Notes(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromMilliseconds(1500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(100);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToStart();
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(thirdAfterResumeDelay, speed), "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), firstEventTime),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod + firstAfterResumeDelay + secondAfterResumeDelay + ScaleTimeSpan(lastEventTime, 1.0 / speed)),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Started, note, originalNote, true),
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveForward_Notes(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(300);

            var stepAfterStop = TimeSpan.FromMilliseconds(400);
            var stepAfterResumed = TimeSpan.FromMilliseconds(300);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(4);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(300);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), firstEventTime),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), ScaleTimeSpan(lastEventTime - stepAfterStop - stepAfterResumed, 1.0 / speed) + stopPeriod),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveForward_BeyondDuration_Notes()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(10);

            var noteLength = TimeSpan.FromSeconds(4);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveForward_BeyondPlaybackEnd_Notes()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);

            var noteLength = TimeSpan.FromMilliseconds(400);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                    new Note((SevenBitNumber)70)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromMilliseconds(450), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveBack_Notes(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(700);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            ClassicAssert.LessOrEqual(stepAfterStop, ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(100);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            var lastEventTime = TimeSpan.FromMilliseconds(3000);
            ClassicAssert.GreaterOrEqual(lastEventTime, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)lastEventTime, TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), ScaleTimeSpan(lastEventTime + stepAfterStop + stepAfterResumed, 1.0 / speed) + stopPeriod),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime_BeyondDuration_Notes()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 10));
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime_BeyondPlaybackEnd_Notes()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                    new Note((SevenBitNumber)70)
                        .SetTime(new MetricTimeSpan(0, 0, 0, 600), TempoMap)
                        .SetLength(new MetricTimeSpan(0, 0, 0, 200), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                        }),
                    new PlaybackAction(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromMilliseconds(450), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note, originalNote, true),
                    (EventType.Finished, note, originalNote, true),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_ChangeCallbackDuringPlayback_Notes_1()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var noteLength1 = TimeSpan.FromSeconds(4);
            var noteLength2 = TimeSpan.FromSeconds(2);

            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength1, TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var note2 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)noteLength1, TempoMap)
                .SetLength((MetricTimeSpan)noteLength2, TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new ITimedObject[]
                {
                    originalNote1,
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteLength1, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteLength1 + noteLength2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackAction(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOnEvent(), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), noteLength1 + stopPeriod + noteLength2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, true),
                    (EventType.Finished, note1, originalNote1, true),
                    (EventType.Started, note2, note2, false),
                    (EventType.Finished, note2, note2, false),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_ChangeCallbackDuringPlayback_Notes_SendNoteOffEventsForNonActiveNotes_1()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var noteLength1 = TimeSpan.FromSeconds(4);
            var noteLength2 = TimeSpan.FromSeconds(2);

            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength1, TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var note2 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)noteLength1, TempoMap)
                .SetLength((MetricTimeSpan)noteLength2, TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new ITimedObject[]
                {
                    originalNote1,
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)noteLength1, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)(noteLength1 + noteLength2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackAction(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), noteLength1 + stopPeriod + noteLength2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, true),
                    (EventType.Finished, note1, originalNote1, true),
                    (EventType.Finished, note1, originalNote1, true),
                    (EventType.Started, note2, note2, false),
                    (EventType.Finished, note2, note2, false),
                },
                setupPlayback: playback =>
                {
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_ChangeCallbackDuringPlayback_Notes_2()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var noteLength1 = TimeSpan.FromSeconds(4);
            var noteLength2 = TimeSpan.FromSeconds(2);

            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength1, TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var note2 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)noteLength1, TempoMap)
                .SetLength((MetricTimeSpan)noteLength2, TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new ITimedObject[]
                {
                    originalNote1,
                    note2,
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackAction(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOnEvent(), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), noteLength1 + stopPeriod + noteLength2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, true),
                    (EventType.Finished, note1, originalNote1, true),
                    (EventType.Started, note2, note2, true),
                    (EventType.Finished, note2, note2, true),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop_ChangeCallbackDuringPlayback_Notes_SendNoteOffEventsForNonActiveNotes_2()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var noteLength1 = TimeSpan.FromSeconds(4);
            var noteLength2 = TimeSpan.FromSeconds(2);

            var originalNote1 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)noteLength1, TempoMap);

            var note1 = (Note)originalNote1.Clone();
            note1.NoteNumber += TransposeBy;

            var note2 = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetTime((MetricTimeSpan)noteLength1, TempoMap)
                .SetLength((MetricTimeSpan)noteLength2, TempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new ITimedObject[]
                {
                    originalNote1,
                    note2,
                },
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackAction(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOnEvent(), noteLength1 + stopPeriod),
                    new ReceivedEvent(new NoteOffEvent(), noteLength1 + stopPeriod + noteLength2),
                },
                expectedNotesEvents: new[]
                {
                    (EventType.Started, note1, originalNote1, true),
                    (EventType.Finished, note1, originalNote1, true),
                    (EventType.Finished, note1, originalNote1, true),
                    (EventType.Started, note2, note2, true),
                    (EventType.Finished, note2, note2, true),
                },
                setupPlayback: playback =>
                {
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        #endregion

        #region Private methods

        private void CheckNoteCallback(
            ICollection<ITimedObject> initialPlaybackObjects,
            PlaybackAction[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            ICollection<(EventType EventType, Note Note, Note OriginalNote, bool CheckOriginalNoteByReference)> expectedNotesEvents,
            Action<Playback> setupPlayback = null)
        {
            var notesEvents = new List<(EventType EventType, Note Note, Note OriginalNote)>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: initialPlaybackObjects,
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: playback =>
                {
                    setupPlayback?.Invoke(playback);

                    playback.NotesPlaybackStarted += (_, e) => notesEvents.AddRange(Enumerable.Range(0, e.Notes.Count).Select(i => (EventType.Started, e.Notes.ElementAt(i), e.OriginalNotes.ElementAt(i))));
                    playback.NotesPlaybackFinished += (_, e) => notesEvents.AddRange(Enumerable.Range(0, e.Notes.Count).Select(i => (EventType.Finished, e.Notes.ElementAt(i), e.OriginalNotes.ElementAt(i))));
                });

            ClassicAssert.AreEqual(
                expectedNotesEvents.Count,
                notesEvents.Count,
                $"Notes events count is invalid.{Environment.NewLine}Actual events:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, notesEvents) +
                    $"{Environment.NewLine}Expected events:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, expectedNotesEvents));

            for (var i = 0; i < expectedNotesEvents.Count; i++)
            {
                var expectedEvent = expectedNotesEvents.ElementAt(i);
                var actualEvent = notesEvents[i];

                ClassicAssert.AreEqual(
                    expectedEvent.EventType,
                    actualEvent.EventType,
                    $"Event type is invalid at index {i}.");

                MidiAsserts.AreEqual(
                    expectedEvent.Note,
                    actualEvent.Note,
                    $"Note is invalid at index {i}.");

                if (expectedEvent.CheckOriginalNoteByReference)
                {
                    ClassicAssert.AreSame(
                        expectedEvent.OriginalNote,
                        actualEvent.OriginalNote,
                        $"Original note is invalid at index {i}.");
                }
                else
                {
                    ClassicAssert.AreNotSame(
                        expectedEvent.OriginalNote,
                        actualEvent.OriginalNote,
                        $"Original note is invalid at index {i}.");
                    MidiAsserts.AreEqual(
                        expectedEvent.OriginalNote,
                        actualEvent.OriginalNote,
                        $"Original note is invalid at index {i}.");
                }
            }
        }

        #endregion
    }
}
