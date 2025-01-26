using System;
using System.Diagnostics;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using Melanchall.DryWetMidi.Tests.Utilities;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
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
        public void NoteCallback_ReturnNull_ReturnSkipNote()
        {
            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                initialNoteCallback: (d, rt, rl, t) => null,
                actions: new[]
                {
                    new PlaybackChangerBase(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => NotePlaybackData.SkipNote),
                },
                expectedReceivedEvents: new ReceivedEvent[] { },
                expectedNotesStarted: new Note[] { },
                expectedNotesFinished: new Note[] { });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnNull_ReturnOriginal()
        {
            var tempoMap = TempoMap;
            var note = new Note((SevenBitNumber)100)
            {
                Velocity = (SevenBitNumber)80
            }
            .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), tempoMap)
            .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), tempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                initialNoteCallback: (d, rt, rl, t) => null,
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
                expectedNotesStarted: new Note[] { note },
                expectedNotesFinished: new Note[] { note });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_ReturnOriginal_ReturnNull()
        {
            var tempoMap = TempoMap;
            var note = new Note(SevenBitNumber.MinValue)
            {
                Velocity = SevenBitNumber.MinValue
            }
            .SetTime((MetricTimeSpan)TimeSpan.Zero, tempoMap)
            .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), tempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                initialNoteCallback: (d, rt, rl, t) => d,
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
                expectedNotesStarted: new Note[] { note },
                expectedNotesFinished: new Note[] { note });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_Transpose()
        {
            var tempoMap = TempoMap;

            var note1 = new Note(TransposeBy)
            {
                Velocity = SevenBitNumber.MinValue
            }
            .SetTime((MetricTimeSpan)TimeSpan.Zero, tempoMap)
            .SetLength((MetricTimeSpan)TimeSpan.FromSeconds(1), tempoMap);

            var note2 = new Note((SevenBitNumber)(100 + TransposeBy))
            {
                Velocity = (SevenBitNumber)80
            }
            .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), tempoMap)
            .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(500), tempoMap);

            CheckNoteCallback(
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent()).SetTime((MetricTimeSpan)TimeSpan.Zero, TempoMap),
                    new TimedEvent(new NoteOffEvent()).SetTime((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1500), TempoMap),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)).SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(2000), TempoMap),
                },
                initialNoteCallback: NoteCallback,
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
                expectedNotesStarted: new Note[] { note1, note2 },
                expectedNotesFinished: new Note[] { note1, note2 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void NoteCallback_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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

            CheckPlayback(
                useOutputDevice: false,
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
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        #endregion

        #region Private methods

        private void CheckNoteCallback(
            ICollection<ITimedObject> initialPlaybackObjects,
            NoteCallback initialNoteCallback,
            PlaybackChangerBase[] actions,
            ICollection<ReceivedEvent> expectedReceivedEvents,
            ICollection<Note> expectedNotesStarted,
            ICollection<Note> expectedNotesFinished)
        {
            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: initialPlaybackObjects,
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: playback =>
                {
                    playback.NoteCallback = initialNoteCallback;

                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            MidiAsserts.AreEqual(notesStarted, expectedNotesStarted, "Invalid notes started.");
            MidiAsserts.AreEqual(notesFinished, expectedNotesFinished, "Invalid notes finished.");
        }

        #endregion
    }
}
