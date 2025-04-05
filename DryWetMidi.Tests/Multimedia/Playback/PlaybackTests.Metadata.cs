using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System.Diagnostics;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class TimedEventWithTrackChunkIndex : TimedEvent, IMetadata
        {
            public TimedEventWithTrackChunkIndex(MidiEvent midiEvent, long time, int trackChunkIndex)
                : base(midiEvent, time)
            {
                Metadata = trackChunkIndex;
            }

            public object Metadata { get; set; }

            public override ITimedObject Clone()
            {
                return new TimedEventWithTrackChunkIndex(Event.Clone(), Time, (int)Metadata);
            }
        }

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_EventPlayed() => CheckPlaybackMetadata(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 25 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 })),
            actions: Array.Empty<PlaybackAction>(),
            expectedEvents: new (MidiEvent, object)[]
            {
                (new NoteOnEvent(), 0),
                (new NoteOffEvent(), 0),
                (new NoteOnEvent(), 0),
                (new NoteOffEvent(), 0),
            });

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_EventPlayed_SendNoteOnEventsForActiveNotes() => CheckPlaybackMetadata(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 25 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 })),
            actions: Array.Empty<PlaybackAction>(),
            expectedEvents: new (MidiEvent, object)[]
            {
                (new NoteOnEvent(), 0),
                (new NoteOnEvent(), 1),
                (new NoteOffEvent(), 0),
                (new NoteOnEvent(), 0),
                (new NoteOnEvent(), 1),
                (new NoteOffEvent(), 0),
            },
            setupPlayback: playback => playback.SendNoteOnEventsForActiveNotes = true);

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_EventPlayed_SendNoteOffEventsForNonActiveNotes() => CheckPlaybackMetadata(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 25 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 })),
            actions: Array.Empty<PlaybackAction>(),
            expectedEvents: new (MidiEvent, object)[]
            {
                (new NoteOnEvent(), 0),
                (new NoteOffEvent(), 0),
                (new NoteOffEvent(), 1),
                (new NoteOnEvent(), 0),
                (new NoteOffEvent(), 0),
                (new NoteOffEvent(), 1),
            },
            setupPlayback: playback => playback.SendNoteOffEventsForNonActiveNotes = true);

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_EventPlayed_SendNoteOffEventsForNonActiveNotes_SendNoteOnEventsForActiveNotes() => CheckPlaybackMetadata(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 25 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 })),
            actions: Array.Empty<PlaybackAction>(),
            expectedEvents: new (MidiEvent, object)[]
            {
                (new NoteOnEvent(), 0),
                (new NoteOnEvent(), 1),
                (new NoteOffEvent(), 0),
                (new NoteOffEvent(), 1),
                (new NoteOnEvent(), 0),
                (new NoteOnEvent(), 1),
                (new NoteOffEvent(), 0),
                (new NoteOffEvent(), 1),
            },
            setupPlayback: playback =>
            {
                playback.SendNoteOffEventsForNonActiveNotes = true;
                playback.SendNoteOnEventsForActiveNotes = true;
            });

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_NoControlChanges_MoveToTime([Values(0, 100)] int moveFromMs) => CheckPlaybackMetadata(
            midiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk(new TextEvent { DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1)) })),
            actions: new[]
            {
                new PlaybackAction(moveFromMs,
                    p => p.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500))),
            },
            expectedEvents: new (MidiEvent, object)[]
            {
                (new TextEvent(), 1)
            });

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_NoControlChanges_MoveToStart() => CheckPlaybackMetadata(
            midiFile: new MidiFile(
                new TrackChunk(new TextEvent()),
                new TrackChunk(new ProgramChangeEvent { DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1)) })),
            actions: new[]
            {
                new PlaybackAction(500,
                    p => p.MoveToStart()),
            },
            expectedEvents: new (MidiEvent, object)[]
            {
                (new TextEvent(), 0),
                (new TextEvent(), 0),
                (new ProgramChangeEvent(), 1)
            });

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_ControlChangeAtZero_MoveToTime()
        {
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(new ControlChangeEvent(controlNumber, controlValue)),
                    new TrackChunk(new TextEvent { DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1)) })),
                actions: new[]
                {
                    new PlaybackAction(100,
                        p => p.MoveToTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500))),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_ControlChangeAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlChangeDelay = TimeSpan.FromMilliseconds(800);
            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber1, controlValue1)),
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber2, controlValue2)
                        {
                            Channel = (FourBitNumber)10,
                            DeltaTime = GetDeltaTime(controlChangeDelay)
                        },
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay - controlChangeDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, 1),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_ControlChangesAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber1, controlValue1)),
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber2, controlValue2),
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber2, controlValue2), 1),
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber2, controlValue2), 1),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromBeforeControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber, controlValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(controlChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - controlChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber, controlValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(controlChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - controlChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_Default_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber, controlValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(controlChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - controlChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromAfterControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)0;
            var controlValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber, controlValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(controlChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromAfterControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ControlChangeEvent(controlNumber, controlValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(controlChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - controlChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, null),
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(0)]
        [TestCase(100)]
        public void CheckPlaybackMetadata_TrackPitchValue_NoPitchBend_MoveToTime(int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_NoPitchBend_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_PitchBendAtZero_MoveToTime()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(pitchValue)),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_PitchBendAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(pitchValue)),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue), 0),
                    (new PitchBendEvent(pitchValue), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromBeforePitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(pitchValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(pitchBendTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime((noteOffTime - pitchBendTime))
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromBeforePitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(pitchValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(pitchBendTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - pitchBendTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromAfterPitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(pitchValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(pitchBendTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromAfterPitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new PitchBendEvent(pitchValue)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(pitchBendTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - pitchBendTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new PitchBendEvent() { Channel = (FourBitNumber)4 }, null),
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(0)]
        [TestCase(100)]
        public void CheckPlaybackMetadata_TrackProgram_NoProgramChanges_MoveToTime(int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_NoProgramChanges_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_ProgramChangeAtZero_MoveToTime()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ProgramChangeEvent(programNumber)),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_ProgramChangeAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ProgramChangeEvent(programNumber)),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber), 0),
                    (new ProgramChangeEvent(programNumber), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromBeforeProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ProgramChangeEvent(programNumber)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(programChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - programChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromBeforeProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ProgramChangeEvent(programNumber)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(programChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - programChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromAfterProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ProgramChangeEvent(programNumber)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(programChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromAfterProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new ProgramChangeEvent(programNumber)
                        {
                            Channel = (FourBitNumber)4,
                            DeltaTime = GetDeltaTime(programChangeTime)
                        }),
                    new TrackChunk(
                        new TextEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffTime - programChangeTime)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, null),
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardToNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber, noteOnVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent(noteNumber, noteOffVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1)
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackToNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(300);

            var moveFrom = TimeSpan.FromSeconds(1);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber, noteOnVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent(noteNumber, noteOffVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        },
                        new PitchBendEvent()
                        {
                            DeltaTime = GetDeltaTime(pitchBendDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new PitchBendEvent(), 1)
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardFromNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(400);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber, noteOnVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent(noteNumber, noteOffVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        },
                        new PitchBendEvent()
                        {
                            DeltaTime = GetDeltaTime(pitchBendDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new PitchBendEvent(), 1)
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackFromNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(400);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(1200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber, noteOnVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent(noteNumber, noteOffVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1)
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardToSameNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber, noteOnVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent(noteNumber, noteOffVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackToSameNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(400);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber, noteOnVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent(noteNumber, noteOffVelocity)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardFromNoteToNote()
        {
            var noteNumber1 = (SevenBitNumber)60;
            var noteOnDelay1 = TimeSpan.Zero;
            var noteOnVelocity1 = (SevenBitNumber)100;
            var noteOffDelay1 = TimeSpan.FromSeconds(1);
            var noteOffVelocity1 = (SevenBitNumber)80;

            var noteNumber2 = (SevenBitNumber)70;
            var noteOnDelay2 = TimeSpan.FromMilliseconds(200);
            var noteOnVelocity2 = (SevenBitNumber)95;
            var noteOffDelay2 = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity2 = (SevenBitNumber)85;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1400);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber1, noteOnVelocity1)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay1)
                        },
                        new NoteOffEvent(noteNumber1, noteOffVelocity1)
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay1)
                        }),
                    new TrackChunk(
                        new NoteOnEvent(noteNumber2, noteOnVelocity2)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay1 + noteOffDelay1 + noteOnDelay2)
                        },
                        new NoteOffEvent(noteNumber2, noteOffVelocity2)
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay2)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber1, noteOnVelocity1), 0),
                    (new NoteOffEvent(noteNumber1, noteOffVelocity1), 0),
                    (new NoteOnEvent(noteNumber2, noteOnVelocity2), 1),
                    (new NoteOffEvent(noteNumber2, noteOffVelocity2), 1)
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackFromNoteToNote()
        {
            var noteNumber1 = (SevenBitNumber)60;
            var noteOnDelay1 = TimeSpan.Zero;
            var noteOnVelocity1 = (SevenBitNumber)100;
            var noteOffDelay1 = TimeSpan.FromSeconds(1);
            var noteOffVelocity1 = (SevenBitNumber)80;

            var noteNumber2 = (SevenBitNumber)70;
            var noteOnDelay2 = TimeSpan.FromMilliseconds(200);
            var noteOnVelocity2 = (SevenBitNumber)95;
            var noteOffDelay2 = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity2 = (SevenBitNumber)85;

            var moveFrom = TimeSpan.FromMilliseconds(1400);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(noteNumber1, noteOnVelocity1)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay1)
                        },
                        new NoteOffEvent(noteNumber1, noteOffVelocity1)
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay1)
                        }),
                    new TrackChunk(
                        new NoteOnEvent(noteNumber2, noteOnVelocity2)
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay1 + noteOffDelay1 + noteOnDelay2)
                        },
                        new NoteOffEvent(noteNumber2, noteOffVelocity2)
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay2)
                        })),
                actions: new[]
                {
                    new PlaybackAction(moveFrom,
                        p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber1, noteOnVelocity1), 0),
                    (new NoteOffEvent(noteNumber1, noteOffVelocity1), 0),
                    (new NoteOnEvent(noteNumber2, noteOnVelocity2), 1),
                    (new NoteOffEvent(noteNumber2, noteOffVelocity2), 1),
                    (new NoteOnEvent(noteNumber1, noteOnVelocity1), 0),
                    (new NoteOffEvent(noteNumber1, noteOffVelocity1), 0),
                    (new NoteOnEvent(noteNumber2, noteOnVelocity2), 1),
                    (new NoteOffEvent(noteNumber2, noteOffVelocity2), 1)
                },
                setupPlayback: playback => playback.TrackNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_StopStart_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 1),
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 1)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.InterruptNotesOnStop = true;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_StopStart_DontInterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay)
                        }),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(noteOnDelay + noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 1)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.InterruptNotesOnStop = false;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_ReturnNull_ReturnSkipNote()
        {
            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap)
                        }),
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500))
                        },
                        new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        })),
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => NotePlaybackData.SkipNote),
                },
                expectedEvents: new (MidiEvent, object)[0],
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NoteCallback = (d, rt, rl, t) => null;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_ReturnNull_ReturnOriginal()
        {
            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap)
                        }),
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500))
                        },
                        new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        })),
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => d),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), 1),
                    (new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), 1)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NoteCallback = (d, rt, rl, t) => null;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_ReturnOriginal_ReturnNull()
        {
            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap)
                        }),
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500))
                        },
                        new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        })),
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = (d, rt, rl, t) => null),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 0)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NoteCallback = (d, rt, rl, t) => d;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_Transpose()
        {
            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap)
                        }),
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500))
                        },
                        new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        })),
                actions: new[]
                {
                    new PlaybackAction(TimeSpan.FromMilliseconds(500),
                        p => p.NoteCallback = NoteCallback),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)80), 1),
                    (new NoteOffEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)0), 1)
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
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
        public void CheckPlaybackMetadata_NoteCallback_InterruptNotesOnStop_SendNoteOffEventsForNonActiveNotes()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent
                        {
                            DeltaTime = GetDeltaTime(noteOffDelay)
                        })),
                actions: new[]
                {
                    new PlaybackAction(stopAfter,
                        p => p.Stop()),
                    new PlaybackAction(stopPeriod,
                        p => p.Start()),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.SendNoteOffEventsForNonActiveNotes = true;
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToStart()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(firstEventTime)
                        }),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(firstEventTime + lastEventTime)
                        })),
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
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, thirdAfterResumeDelay, "resumed")),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToStart_SendNoteOnEventsForActiveNotes()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(firstEventTime)
                        }),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(firstEventTime + lastEventTime)
                        })),
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
                        p => CheckCurrentTime(p, firstAfterResumeDelay, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveToStart();
                            CheckCurrentTime(p, TimeSpan.Zero, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, thirdAfterResumeDelay, "resumed")),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveForward()
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

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(firstEventTime)
                        }),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(firstEventTime + lastEventTime)
                        })),
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
                            CheckCurrentTime(p, stopAfter + stepAfterStop, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p => CheckCurrentTime(p, stopAfter + firstAfterResumeDelay + stepAfterStop, "resumed")),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveForward((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + stepAfterStop + stepAfterResumed, "resumed");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay + stepAfterStop + stepAfterResumed, "resumed")),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveForward_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(10);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveForward_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap)
                        }),
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap)
                        },
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
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
        public void CheckPlaybackMetadata_NoteCallback_MoveBack()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromMilliseconds(5500);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(lastEventTime)
                        })),
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
                            CheckCurrentTime(p, stopAfter - stepAfterStop, "stopped");
                        }),
                    new PlaybackAction(firstAfterResumeDelay,
                        p =>
                        {
                            Assert.IsTrue(p.IsRunning, "Playback is not running after resumed.");
                            CheckCurrentTime(p, stopAfter + firstAfterResumeDelay - stepAfterStop, "resumed on first span");
                        }),
                    new PlaybackAction(secondAfterResumeDelay,
                        p =>
                        {
                            p.MoveBack((MetricTimeSpan)stepAfterResumed);
                            CheckCurrentTime(p, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay - stepAfterStop - stepAfterResumed, "resumed on second span");
                        }),
                    new PlaybackAction(thirdAfterResumeDelay,
                        p => CheckCurrentTime(p, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterStop - stepAfterResumed, "resumed on third span")),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveBack_BeyondZero()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromMilliseconds(600);

            var stepAfterStop = TimeSpan.FromMilliseconds(2000);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(300);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(lastEventTime)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveBack_BeyondZero_SendNoteOnEventsForActiveNotes()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromMilliseconds(600);

            var stepAfterStop = TimeSpan.FromMilliseconds(2000);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(2);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(300);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(lastEventTime)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.InterruptNotesOnStop = false;
                    playback.SendNoteOnEventsForActiveNotes = true;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveBack_BeyondPlaybackStart()
        {
            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromMilliseconds(200))
                        }),
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromMilliseconds(400))
                        },
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromMilliseconds(200))
                        })),
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(500), "A");
                        p.MoveBack(new MetricTimeSpan(0, 0, 0, 400));
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                    }),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 2),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 2),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackStart = new MetricTimeSpan(0, 0, 0, 300);
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveBack_BeyondPlaybackStart_SendNoteOnEventsForActiveNotes()
        {
            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromMilliseconds(200))
                        }),
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromMilliseconds(400))
                        },
                        new NoteOffEvent()
                        {
                            DeltaTime = GetDeltaTime(TimeSpan.FromMilliseconds(200))
                        })),
                actions: new PlaybackAction[]
                {
                    new PlaybackAction(200, p =>
                    {
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(500), "A");
                        p.MoveBack(new MetricTimeSpan(0, 0, 0, 400));
                        CheckCurrentTime(p, TimeSpan.FromMilliseconds(300), "B");
                    }),
                },
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 2),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 2),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 2),
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
        public void CheckPlaybackMetadata_NoteCallback_MoveToTime()
        {
            var lastEventTime = TimeSpan.FromMilliseconds(2000);

            var stopAfter = TimeSpan.FromMilliseconds(800);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var moveTime1 = TimeSpan.FromMilliseconds(200);
            var moveTime2 = TimeSpan.FromMilliseconds(1600);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(200);
            var secondAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(200);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)lastEventTime, TempoMap)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToTime_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToTime_BeyondPlaybackEnd()
        {
            var stopAfter = TimeSpan.FromMilliseconds(200);
            var stopPeriod = TimeSpan.FromMilliseconds(200);

            CheckPlaybackMetadata(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent()),
                    new TrackChunk(
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap)
                        }),
                    new TrackChunk(
                        new NoteOnEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap)
                        },
                        new NoteOffEvent()
                        {
                            DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(700), TempoMap)
                        })),
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
                expectedEvents: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                },
                setupPlayback: playback =>
                {
                    playback.TrackNotes = false;
                    playback.NoteCallback = NoteCallback;
                    playback.PlaybackEnd = new MetricTimeSpan(0, 0, 0, 450);
                });
        }

        #endregion

        #region Private methods

        private static IEnumerable<TimedEvent> GetTimedEvents(MidiFile midiFile) => midiFile
            .GetTrackChunks()
            .SelectMany((c, i) => c.GetTimedEvents().Select(e => new TimedEventWithTrackChunkIndex(e.Event, e.Time, i)))
            .OrderBy(e => e.Time);

        private static void CheckRegisteredMetadata(
            (MidiEvent, object)[] expectedMetadata,
            (MidiEvent, object)[] actualMetadata)
        {
            Assert.AreEqual(
                expectedMetadata.Length,
                actualMetadata.Length,
                $"Metadata count is invalid.{Environment.NewLine}Actual metadata:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, actualMetadata) +
                    $"{Environment.NewLine}Expected metadata:{Environment.NewLine}" +
                    string.Join(Environment.NewLine, expectedMetadata));

            for (var i = 0; i < expectedMetadata.Length; i++)
            {
                var expectedRecord = expectedMetadata[i];
                var actualRecord = actualMetadata[i];

                MidiAsserts.AreEqual(expectedRecord.Item1, actualRecord.Item1, false, $"Record {i}: Event is invalid.");
                Assert.AreEqual(expectedRecord.Item2, actualRecord.Item2, $"Record {i}: Metadata is invalid.");
            }
        }

        private long GetDeltaTime(TimeSpan deltaTime) =>
            TimeConverter.ConvertFrom((MetricTimeSpan)deltaTime, TempoMap);

        private void CheckPlaybackMetadata(
            MidiFile midiFile,
            PlaybackAction[] actions,
            (MidiEvent, object)[] expectedEvents,
            Action<Playback> setupPlayback = null,
            Action<Playback> afterStart = null)
        {
            var timedEvents = GetTimedEvents(midiFile);
            var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
            var stopwatch = new Stopwatch();

            var actualEvents = new List<(MidiEvent, object)>();

            using (var playback = new Playback(timedEvents, TempoMap, new OutputDeviceMock()))
            {
                setupPlayback?.Invoke(playback);

                playback.EventPlayed += (_, e) => actualEvents.Add((e.Event, (int?)e.Metadata));

                stopwatch.Start();
                playback.Start();

                afterStart?.Invoke(playback);

                foreach (var action in actions)
                {
                    var waitStopwatch = Stopwatch.StartNew();

                    while (waitStopwatch.ElapsedMilliseconds < action.PeriodMs)
                    {
                    }

                    action.Action(playback);
                }

                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CheckRegisteredMetadata(expectedEvents, actualEvents.ToArray());
        }

        #endregion
    }
}
