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
    // TODO: check metric length type
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitObjectsByPartsNumber_EmptyCollection([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: Array.Empty<ITimedObject>(),
            partsNumber: 10,
            lengthType: lengthType,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void SplitObjectsByPartsNumber_Nulls([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: new[]
            {
                default(ITimedObject),
                default(ITimedObject)
            },
            partsNumber: 10,
            lengthType: lengthType,
            expectedObjects: new[]
            {
                default(ITimedObject),
                default(ITimedObject)
            });

        [Test]
        public void SplitObjectsByPartsNumber_OnePart([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 100, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 150, 10))
            },
            partsNumber: 1,
            lengthType: lengthType,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 100, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 150, 10))
            });

        [Test]
        public void SplitObjectsByPartsNumber_ZeroLength([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 0, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 10),
                    new Note((SevenBitNumber)90, 0, 10))
            },
            partsNumber: 2,
            lengthType: lengthType,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 0, 20),
                new Note((SevenBitNumber)70, 0, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 10),
                    new Note((SevenBitNumber)90, 0, 10)),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 10),
                    new Note((SevenBitNumber)90, 0, 10))
            });

        [Test]
        public void SplitObjectsByPartsNumber_EqualDivision_SingleNote_Midi() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 30, 20),
            },
            partsNumber: 5,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 6, 20),
                new Note((SevenBitNumber)70, 6, 26),
                new Note((SevenBitNumber)70, 6, 32),
                new Note((SevenBitNumber)70, 6, 38),
                new Note((SevenBitNumber)70, 6, 44),
            });

        [Test]
        public void SplitObjectsByPartsNumber_EqualDivision_Midi() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 30, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 40, 0),
                    new Note((SevenBitNumber)90, 50, 10))
            },
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 15, 20),
                new Note((SevenBitNumber)70, 15, 35),
                new Chord(
                    new Note((SevenBitNumber)80, 30, 0),
                    new Note((SevenBitNumber)90, 20, 10)),
                new Chord(
                    new Note((SevenBitNumber)80, 10, 30),
                    new Note((SevenBitNumber)90, 30, 30))
            });

        [Test]
        public void SplitObjectsByPartsNumber_EqualDivision_BarBeatFraction()
        {
            var oneAndHalfBeatLength = LengthConverter.ConvertFrom(new BarBeatFractionTimeSpan(0, 1.5), 0, TempoMap.Default);
            CheckSplitObjectsByPartsNumber(
                inputObjects: new ITimedObject[]
                {
                    new Note(
                        (SevenBitNumber)70,
                        LengthConverter.ConvertFrom(new BarBeatFractionTimeSpan(0, 3), 20, TempoMap.Default),
                        20),
                },
                partsNumber: 2,
                lengthType: TimeSpanType.BarBeatFraction,
                expectedObjects: new ITimedObject[]
                {
                    new Note(
                        (SevenBitNumber)70,
                        oneAndHalfBeatLength,
                        20),
                    new Note(
                        (SevenBitNumber)70,
                        oneAndHalfBeatLength,
                        20 + oneAndHalfBeatLength),
                });
        }

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleNote_Midi() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 30, 20),
            },
            partsNumber: 8,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 4, 20),
                new Note((SevenBitNumber)70, 4, 24),
                new Note((SevenBitNumber)70, 4, 28),
                new Note((SevenBitNumber)70, 4, 32),
                new Note((SevenBitNumber)70, 4, 36),
                new Note((SevenBitNumber)70, 3, 40),
                new Note((SevenBitNumber)70, 4, 43),
                new Note((SevenBitNumber)70, 3, 47),
            });

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleNote_Midi_SmallLength() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 5, 0),
            },
            partsNumber: 8,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 1, 0),
                new Note((SevenBitNumber)70, 1, 1),
                new Note((SevenBitNumber)70, 1, 2),
                new Note((SevenBitNumber)70, 0, 3),
                new Note((SevenBitNumber)70, 1, 3),
                new Note((SevenBitNumber)70, 0, 4),
                new Note((SevenBitNumber)70, 1, 4),
                new Note((SevenBitNumber)70, 0, 5),
            });

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleChord_Midi_SmallLength_1() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 3, 0),
                    new Note((SevenBitNumber)80, 3, 0)),
            },
            partsNumber: 5,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 1, 0),
                    new Note((SevenBitNumber)80, 1, 0)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 1),
                    new Note((SevenBitNumber)80, 1, 1)),
                new Chord(
                    new Note((SevenBitNumber)70, 0, 2),
                    new Note((SevenBitNumber)80, 0, 2)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 2),
                    new Note((SevenBitNumber)80, 1, 2)),
                new Chord(
                    new Note((SevenBitNumber)70, 0, 3),
                    new Note((SevenBitNumber)80, 0, 3)),
            });

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleChord_Midi_SmallLength_2() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 3, 0),
                    new Note((SevenBitNumber)80, 3, 1)),
            },
            partsNumber: 5,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 1, 0)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 1),
                    new Note((SevenBitNumber)80, 1, 1)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 2),
                    new Note((SevenBitNumber)80, 1, 2)),
                new Chord(
                    new Note((SevenBitNumber)80, 1, 3)),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 4)),
            });

        [Test]
        public void SplitObjectsByPartsNumber_TimedEventsAndNotes() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)70, 30, 20),
                new Note((SevenBitNumber)50, 20, 70),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            },
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)70, 15, 20),
                new Note((SevenBitNumber)70, 15, 35),
                new Note((SevenBitNumber)50, 10, 70),
                new Note((SevenBitNumber)50, 10, 80),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            });

        [Test]
        public void CheckSplitObjectsByPartsNumber_TrackChunk_Simple() => CheckSplitObjectsByPartsNumber_TrackChunk(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)70, 30, 20),
                new Note((SevenBitNumber)50, 20, 70),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            },
            objectType: ObjectType.Note,
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)70, 15, 20),
                new Note((SevenBitNumber)70, 15, 35),
                new Note((SevenBitNumber)50, 10, 70),
                new Note((SevenBitNumber)50, 10, 80),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            });

        [Test]
        public void CheckSplitObjectsByPartsNumber_TrackChunk_Settings_1() => CheckSplitObjectsByPartsNumber_TrackChunk(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)70, 30, 20),
                new Note((SevenBitNumber)50, 20, 70),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            },
            objectType: ObjectType.Chord,
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(new Note((SevenBitNumber)70, 15, 20)),
                new Chord(new Note((SevenBitNumber)70, 15, 35)),
                new Chord(new Note((SevenBitNumber)50, 10, 70)),
                new Chord(new Note((SevenBitNumber)50, 10, 80)),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 1
                }
            });

        [Test]
        public void CheckSplitObjectsByPartsNumber_TrackChunk_Settings_2() => CheckSplitObjectsByPartsNumber_TrackChunk(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)70, 30, 20),
                new Note((SevenBitNumber)50, 20, 70),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            },
            objectType: ObjectType.Chord,
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(new Note((SevenBitNumber)70, 30, 20)),
                new Chord(new Note((SevenBitNumber)50, 20, 70)),
                new TimedEvent(new TextEvent("B"), 100),
                new TimedEvent(new TextEvent("C"), 200),
            },
            settings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 1,
                    NotesTolerance = 100
                }
            });

        [Test]
        public void CheckSplitObjectsByPartsNumber_TrackChunks_Simple() => CheckSplitObjectsByPartsNumber_TrackChunks(
            inputObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 30, 20),
                },
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)50, 20, 70),
                    new TimedEvent(new TextEvent("B"), 100),
                    new TimedEvent(new TextEvent("C"), 200),
                }
            },
            objectType: ObjectType.Note,
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 15, 20),
                    new Note((SevenBitNumber)70, 15, 35),
                },
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)50, 10, 70),
                    new Note((SevenBitNumber)50, 10, 80),
                    new TimedEvent(new TextEvent("B"), 100),
                    new TimedEvent(new TextEvent("C"), 200),
                }
            });

        [Test]
        public void CheckSplitObjectsByPartsNumber_TrackChunks_Simple_Settings_1() => CheckSplitObjectsByPartsNumber_TrackChunks(
            inputObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 30, 20),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 70),
                    new TimedEvent(new NoteOnEvent(), 76),
                    new TimedEvent(new NoteOffEvent(), 80),
                    new TimedEvent(new NoteOffEvent(), 86),
                    new TimedEvent(new TextEvent("B"), 100),
                    new TimedEvent(new TextEvent("C"), 200),
                }
            },
            objectType: ObjectType.Note,
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 15, 20),
                    new Note((SevenBitNumber)70, 15, 35),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 70),
                    new TimedEvent(new NoteOffEvent(), 75),
                    new TimedEvent(new NoteOnEvent(), 75),
                    new TimedEvent(new NoteOnEvent(), 76),
                    new TimedEvent(new NoteOffEvent(), 80),
                    new TimedEvent(new NoteOffEvent(), 81),
                    new TimedEvent(new NoteOnEvent(), 81),
                    new TimedEvent(new NoteOffEvent(), 86),
                    new TimedEvent(new TextEvent("B"), 100),
                    new TimedEvent(new TextEvent("C"), 200),
                }
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                }
            });

        [Test]
        public void CheckSplitObjectsByPartsNumber_TrackChunks_Simple_Settings_2() => CheckSplitObjectsByPartsNumber_TrackChunks(
            inputObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 30, 20),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 70),
                    new TimedEvent(new NoteOnEvent(), 76),
                    new TimedEvent(new NoteOffEvent(), 80),
                    new TimedEvent(new NoteOffEvent(), 86),
                    new TimedEvent(new TextEvent("B"), 100),
                    new TimedEvent(new TextEvent("C"), 200),
                }
            },
            objectType: ObjectType.Note,
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 15, 20),
                    new Note((SevenBitNumber)70, 15, 35),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 70),
                    new TimedEvent(new NoteOnEvent(), 76),
                    new TimedEvent(new NoteOffEvent(), 78),
                    new TimedEvent(new NoteOffEvent(), 78),
                    new TimedEvent(new NoteOnEvent(), 78),
                    new TimedEvent(new NoteOnEvent(), 78),
                    new TimedEvent(new NoteOffEvent(), 80),
                    new TimedEvent(new NoteOffEvent(), 86),
                    new TimedEvent(new TextEvent("B"), 100),
                    new TimedEvent(new TextEvent("C"), 200),
                }
            },
            settings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        #endregion

        #region Private methods

        private void CheckSplitObjectsByPartsNumber(
            ICollection<ITimedObject> inputObjects,
            int partsNumber,
            TimeSpanType lengthType,
            ICollection<ITimedObject> expectedObjects)
        {
            var actualObjects = inputObjects.SplitObjectsByPartsNumber(partsNumber, lengthType, TempoMap.Default).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                true,
                0,
                "Invalid result objects.");
        }

        private void CheckSplitObjectsByPartsNumber_TrackChunk(
            ICollection<ITimedObject> inputObjects,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            ICollection<ITimedObject> expectedObjects,
            ObjectDetectionSettings settings = null)
        {
            var trackChunk = inputObjects.ToTrackChunk();
            trackChunk.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                expectedObjects.ToTrackChunk(),
                trackChunk,
                true,
                "Invalid result track chunk.");

            var trackChunks = new[] { inputObjects.ToTrackChunk() };
            trackChunks.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                new[] { expectedObjects.ToTrackChunk() },
                trackChunks,
                true,
                "Invalid result track chunks.");

            var midiFile = inputObjects.ToFile();
            midiFile.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, settings);
            MidiAsserts.AreEqual(
                expectedObjects.ToFile(),
                midiFile,
                true,
                "Invalid result track file.");
        }

        private void CheckSplitObjectsByPartsNumber_TrackChunks(
            ICollection<ICollection<ITimedObject>> inputObjects,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            ICollection<ICollection<ITimedObject>> expectedObjects,
            ObjectDetectionSettings settings = null)
        {
            var trackChunks = inputObjects.Select(obj => obj.ToTrackChunk()).ToArray();
            trackChunks.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                expectedObjects.Select(e => e.ToTrackChunk()).ToArray(),
                trackChunks,
                true,
                "Invalid result track chunks.");

            var midiFile = new MidiFile(inputObjects.Select(obj => obj.ToTrackChunk()).ToArray());
            midiFile.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, settings);
            MidiAsserts.AreEqual(
                new MidiFile(expectedObjects.Select(e => e.ToTrackChunk()).ToArray()),
                midiFile,
                true,
                "Invalid result track file.");
        }

        #endregion
    }
}
