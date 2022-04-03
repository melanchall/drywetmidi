using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitObjectsByStep_EmptyCollection() => CheckSplitObjectsByStep(
            inputObjects: Enumerable.Empty<ILengthedObject>(),
            step: (MidiTimeSpan)100,
            expectedObjects: Enumerable.Empty<ILengthedObject>());

        [Test]
        public void SplitObjectsByStep_Nulls() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                default(ILengthedObject),
                default(ILengthedObject)
            },
            step: (MidiTimeSpan)100,
            expectedObjects: new[]
            {
                default(ILengthedObject),
                default(ILengthedObject)
            });

        [Test]
        public void SplitObjectsByStep_ZeroStep_Note_ZeroLength() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70)
            },
            step: (MidiTimeSpan)0,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70)
            });

        [Test]
        public void SplitObjectsByStep_ZeroStep_Note_NonZeroLength() =>
            Assert.Throws<InvalidOperationException>(() => CheckSplitObjectsByStep(
                inputObjects: new[]
                {
                    new Note((SevenBitNumber)70, 100)
                },
                step: (MidiTimeSpan)0,
                expectedObjects: null));

        [Test]
        public void SplitObjectsByStep_StepGreaterThanObjectLength_SingleNote(
            [Values(0, 10)] long length,
            [Values(0, 100)] long time,
            [Values(0, 1, 10)] long stepMargin) => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, length, time)
            },
            step: (MidiTimeSpan)(length + stepMargin),
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, length, time)
            });

        [Test]
        public void SplitObjectsByStep_StepGreaterThanObjectLength_SingleChord(
            [Values(0, 10)] long length,
            [Values(0, 100)] long time,
            [Values(0, 100)] long secondNoteTimeShift,
            [Values(0, 1, 10)] long stepMargin) => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, length, time),
                    new Note((SevenBitNumber)50, length, time + secondNoteTimeShift))
            },
            step: (MidiTimeSpan)(length + stepMargin + secondNoteTimeShift),
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, length, time),
                    new Note((SevenBitNumber)50, length, time + secondNoteTimeShift))
            });

        [Test]
        public void SplitObjectsByStep_StepGreaterThanObjectLength_MixedObjects(
            [Values(0, 10)] long length,
            [Values(0, 100)] long time,
            [Values(0, 100)] long secondNoteTimeShift,
            [Values(0, 1, 10)] long stepMargin) => CheckSplitObjectsByStep(
            inputObjects: new ILengthedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, length, time),
                    new Note((SevenBitNumber)50, length, time + secondNoteTimeShift)),
                new Note((SevenBitNumber)10, length, time + secondNoteTimeShift),
                new Chord(
                    new Note((SevenBitNumber)90, length, time)),
            },
            step: (MidiTimeSpan)(length + stepMargin + secondNoteTimeShift),
            expectedObjects: new ILengthedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, length, time),
                    new Note((SevenBitNumber)50, length, time + secondNoteTimeShift)),
                new Note((SevenBitNumber)10, length, time + secondNoteTimeShift),
                new Chord(
                    new Note((SevenBitNumber)90, length, time)),
            });

        [Test]
        public void SplitObjectsByStep_EqualDivision_SingleNote(
            [Values(10, 100)] long length,
            [Values(2, 5)] long step,
            [Values(0, 20)] long time) => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, length, time)
            },
            step: (MidiTimeSpan)step,
            expectedObjects: Enumerable
                .Range(0, (int)(length / step))
                .Select(i => new Note((SevenBitNumber)70, step, time + i * step))
                .ToArray());

        [Test]
        public void SplitObjectsByStep_EqualDivision_SingleChord_1() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0),
                    new Note((SevenBitNumber)90, 100, 30),
                    new Note((SevenBitNumber)70, 100, 60))
            },
            step: (MidiTimeSpan)80,
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 80, 0),
                    new Note((SevenBitNumber)90, 50, 30),
                    new Note((SevenBitNumber)70, 20, 60)),
                new Chord(
                    new Note((SevenBitNumber)70, 20, 80),
                    new Note((SevenBitNumber)90, 50, 80),
                    new Note((SevenBitNumber)70, 80, 80))
            });

        [Test]
        public void SplitObjectsByStep_EqualDivision_SingleChord_2() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0),
                    new Note((SevenBitNumber)90, 100, 30),
                    new Note((SevenBitNumber)70, 100, 60))
            },
            step: (MidiTimeSpan)40,
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 40, 0),
                    new Note((SevenBitNumber)90, 10, 30)),
                new Chord(
                    new Note((SevenBitNumber)70, 40, 40),
                    new Note((SevenBitNumber)90, 40, 40),
                    new Note((SevenBitNumber)70, 20, 60)),
                new Chord(
                    new Note((SevenBitNumber)70, 20, 80),
                    new Note((SevenBitNumber)90, 40, 80),
                    new Note((SevenBitNumber)70, 40, 80)),
                new Chord(
                    new Note((SevenBitNumber)90, 10, 120),
                    new Note((SevenBitNumber)70, 40, 120))
            });

        [Test]
        public void SplitObjectsByStep_MixedObjects() => CheckSplitObjectsByStep(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 50, 5),
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0),
                    new Note((SevenBitNumber)90, 100, 30),
                    new Note((SevenBitNumber)70, 100, 60)),
                new Rest(10, 80, null, null)
            },
            step: (MidiTimeSpan)40,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 40, 5),
                new Note((SevenBitNumber)70, 10, 45),
                new Chord(
                    new Note((SevenBitNumber)70, 40, 0),
                    new Note((SevenBitNumber)90, 10, 30)),
                new Chord(
                    new Note((SevenBitNumber)70, 40, 40),
                    new Note((SevenBitNumber)90, 40, 40),
                    new Note((SevenBitNumber)70, 20, 60)),
                new Chord(
                    new Note((SevenBitNumber)70, 20, 80),
                    new Note((SevenBitNumber)90, 40, 80),
                    new Note((SevenBitNumber)70, 40, 80)),
                new Chord(
                    new Note((SevenBitNumber)90, 10, 120),
                    new Note((SevenBitNumber)70, 40, 120)),
                new Rest(10, 40, null, null),
                new Rest(50, 40, null, null)
            });

        [Test]
        public void SplitObjectsByStep_UnequalDivision_SingleNote_1() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, 100)
            },
            step: (MidiTimeSpan)70,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 70, 0),
                new Note((SevenBitNumber)70, 30, 70)
            });

        [Test]
        public void SplitObjectsByStep_UnequalDivision_SingleNote_2() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, 100)
            },
            step: (MidiTimeSpan)40,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 40, 0),
                new Note((SevenBitNumber)70, 40, 40),
                new Note((SevenBitNumber)70, 20, 80)
            });

        [Test]
        public void SplitObjectsByStep_UnequalDivision_SingleChord_1() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0),
                    new Note((SevenBitNumber)90, 100, 30),
                    new Note((SevenBitNumber)70, 100, 60))
            },
            step: (MidiTimeSpan)100,
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0),
                    new Note((SevenBitNumber)90, 70, 30),
                    new Note((SevenBitNumber)70, 40, 60)),
                new Chord(
                    new Note((SevenBitNumber)90, 30, 100),
                    new Note((SevenBitNumber)70, 60, 100))
            });

        [Test]
        public void SplitObjectsByStep_UnequalDivision_SingleChord_2() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 100, 0),
                    new Note((SevenBitNumber)90, 100, 30),
                    new Note((SevenBitNumber)70, 100, 60))
            },
            step: (MidiTimeSpan)70,
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 70, 0),
                    new Note((SevenBitNumber)90, 40, 30),
                    new Note((SevenBitNumber)70, 10, 60)),
                new Chord(
                    new Note((SevenBitNumber)70, 30, 70),
                    new Note((SevenBitNumber)90, 60, 70),
                    new Note((SevenBitNumber)70, 70, 70)),
                new Chord(
                    new Note((SevenBitNumber)70, 20, 140))
            });

        [Test]
        public void SplitObjectsByStep_ZeroStep_Chord_ZeroLength() => CheckSplitObjectsByStep(
            inputObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70))
            },
            step: (MidiTimeSpan)0,
            expectedObjects: new[]
            {
                new Chord(
                    new Note((SevenBitNumber)70))
            });

        [Test]
        public void SplitObjectsByStep_ZeroStep_Chord_NonZeroLength() =>
            Assert.Throws<InvalidOperationException>(() => CheckSplitObjectsByStep(
                inputObjects: new[]
                {
                    new Chord(
                        new Note((SevenBitNumber)70, 100))
                },
                step: (MidiTimeSpan)0,
                expectedObjects: null));

        [Test]
        public void SplitObjectsByStep_MixedObjects_TrackChunk_AllObjects() => CheckSplitObjectsByStep_TrackChunk(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 100, 30),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 100, 10))
            },
            objectType: ObjectType.Note | ObjectType.Chord,
            step: (MidiTimeSpan)70,
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), 70),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), 70),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), 70),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 70),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), 100),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), 110),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 130),
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    NotesMinCount = 2
                }
            });

        [Test]
        public void SplitObjectsByStep_MixedObjects_TrackChunk_Chord() => CheckSplitObjectsByStep_TrackChunk(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 100, 30),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 100, 10))
            },
            objectType: ObjectType.Chord,
            step: (MidiTimeSpan)70,
            expectedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 130),
                
                new TimedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), 70),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), 70),
                
                new TimedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), 70),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), 100),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 70),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity), 110),
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10,
                    NotesMinCount = 2
                }
            });

        #endregion

        #region Private methods

        private void CheckSplitObjectsByStep(
            IEnumerable<ILengthedObject> inputObjects,
            ITimeSpan step,
            IEnumerable<ILengthedObject> expectedObjects)
        {
            var actualObjects = inputObjects.SplitObjectsByStep(step, TempoMap.Default).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                true,
                0,
                "Invalid result objects.");
        }

        private void CheckSplitObjectsByStep_TrackChunk(
            IEnumerable<ILengthedObject> inputObjects,
            ObjectType objectType,
            ITimeSpan step,
            IEnumerable<TimedEvent> expectedEvents,
            ObjectDetectionSettings settings = null)
        {
            var trackChunk = inputObjects.ToTrackChunk();
            trackChunk.SplitObjectsByStep(objectType, step, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                expectedEvents.ToTrackChunk(),
                trackChunk,
                true,
                "Invalid result track chunk.");

            var trackChunks = new[] { inputObjects.ToTrackChunk() };
            trackChunks.SplitObjectsByStep(objectType, step, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                new[] { expectedEvents.ToTrackChunk() },
                trackChunks,
                true,
                "Invalid result track chunks.");

            var midiFile = inputObjects.ToFile();
            midiFile.SplitObjectsByStep(objectType, step, settings);
            MidiAsserts.AreEqual(
                expectedEvents.ToFile(),
                midiFile,
                true,
                "Invalid result track file.");
        }

        #endregion
    }
}
