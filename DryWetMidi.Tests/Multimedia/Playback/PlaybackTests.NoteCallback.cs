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
                new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(stopAfter, p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod, p => p.Start()),
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
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveToStart(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var firstEventTime = TimeSpan.Zero;

            // TODO: changing to 2 sec breaks the test
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToStart();
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveForward(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromSeconds(1);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(8);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
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
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveBack(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            Debug.Assert(stepAfterStop <= ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromMilliseconds(5500);
            Debug.Assert(lastEventTime >= ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
            var stopAfter = TimeSpan.FromSeconds(3);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(5);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_MoveToTime()
        {
            var stopAfter = TimeSpan.FromSeconds(4);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(10), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(10), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 1));
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(1), "stopped");
                        }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => CheckCurrentTime(p, TimeSpan.FromSeconds(2), "resumed")),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(2),
                        p =>
                        {
                            p.MoveToTime(new MetricTimeSpan(0, 0, 8));
                            CheckCurrentTime(p, TimeSpan.FromSeconds(8), "resumed");
                        }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => CheckCurrentTime(p, TimeSpan.FromSeconds(9), "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromSeconds(11)),
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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 10));
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
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
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackChangerBase(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod,
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
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = false;
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
                new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
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
                    new PlaybackChangerBase(stopAfter, p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod, p => p.Start()),
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
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveToStart_Notes(double speed)
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var firstEventTime = TimeSpan.Zero;

            // TODO: changing to 2 sec breaks the test
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToStart();
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(firstAfterResumeDelay, speed), "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
                    playback.Speed = speed;
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveForward_Notes(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromSeconds(1);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(8);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) + stepAfterStop, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) + stepAfterStop, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveForward((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
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
        [TestCase(1.0)]
        [TestCase(2.0)]
        [TestCase(0.5)]
        public void NoteCallback_MoveBack_Notes(double speed)
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            Debug.Assert(stepAfterStop <= ScaleTimeSpan(stopAfter, speed), "Step after stop is invalid.");

            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromMilliseconds(5500);
            Debug.Assert(lastEventTime >= ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "Last event time is invalid.");

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter, speed) - stepAfterStop, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay, speed) - stepAfterStop, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, ScaleTimeSpan(stopAfter + firstAfterResumeDelay + secondAfterResumeDelay, speed) - stepAfterStop - stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
        public void NoteCallback_MoveBack_BeyondZero_Notes()
        {
            var stopAfter = TimeSpan.FromSeconds(3);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(5);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveBack((MetricTimeSpan)stepAfterStop);
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.Zero, "stopped");
                        }),
                    new PlaybackChangerBase(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackChangerBase(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                        }),
                    new PlaybackChangerBase(thirdAfterResumeDelay,
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
                    (EventType.Started, note, originalNote, true),
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
        public void NoteCallback_MoveToTime_Notes()
        {
            var stopAfter = TimeSpan.FromSeconds(4);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var originalNote = new Note((SevenBitNumber)0) { Velocity = SevenBitNumber.MinValue }
                .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(10), TempoMap);

            var note = (Note)originalNote.Clone();
            note.NoteNumber += TransposeBy;

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    originalNote,
                },
                actions: new[]
                {
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 1));
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(1), "stopped");
                        }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => CheckCurrentTime(p, TimeSpan.FromSeconds(2), "resumed")),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(2),
                        p =>
                        {
                            p.MoveToTime(new MetricTimeSpan(0, 0, 8));
                            CheckCurrentTime(p, TimeSpan.FromSeconds(8), "resumed");
                        }),
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => CheckCurrentTime(p, TimeSpan.FromSeconds(9), "resumed")),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.FromSeconds(11)),
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
                    new PlaybackChangerBase(stopAfter,
                        p =>
                        {
                            p.Stop();
                            p.MoveToTime(new MetricTimeSpan(0, 0, 10));
                        }),
                    new PlaybackChangerBase(stopPeriod,
                        p =>
                        {
                            p.Start();
                            CheckCurrentTime(p, TimeSpan.FromSeconds(4), "stopped");
                        }),
                },
                expectedReceivedEvents: new[]
                {
                    new ReceivedEvent(new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), TimeSpan.Zero),
                    new ReceivedEvent(new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), stopAfter + stopPeriod),
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
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackChangerBase(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod,
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
                    playback.InterruptNotesOnStop = true;
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
                    new PlaybackChangerBase(TimeSpan.FromSeconds(1),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                    new PlaybackChangerBase(stopAfter - TimeSpan.FromSeconds(1),
                        p => p.Stop()),
                    new PlaybackChangerBase(stopPeriod,
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
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        #endregion

        #region Private methods

        private void CheckNoteCallback(
            ICollection<ITimedObject> initialPlaybackObjects,
            PlaybackChangerBase[] actions,
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

            Assert.AreEqual(
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

                Assert.AreEqual(
                    expectedEvent.EventType,
                    actualEvent.EventType,
                    $"Event type is invalid at index {i}.");

                MidiAsserts.AreEqual(
                    expectedEvent.Note,
                    actualEvent.Note,
                    $"Note is invalid at index {i}.");

                if (expectedEvent.CheckOriginalNoteByReference)
                {
                    Assert.AreSame(
                        expectedEvent.OriginalNote,
                        actualEvent.OriginalNote,
                        $"Original note is invalid at index {i}.");
                }
                else
                {
                    Assert.AreNotSame(
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
