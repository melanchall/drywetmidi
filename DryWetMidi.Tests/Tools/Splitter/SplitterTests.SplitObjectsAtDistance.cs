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
        public void SplitObjectsAtDistance_Notes_EmptyCollection([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: Array.Empty<Note>(),
            distance: (MidiTimeSpan)100,
            from: from,
            expectedObjects: Array.Empty<Note>());

        [Test]
        public void SplitObjectsAtDistance_Notes_Nulls([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: new[] { default(ITimedObject), default(ITimedObject) },
            distance: (MidiTimeSpan)100,
            from: from,
            expectedObjects: new[] { default(ITimedObject), default(ITimedObject) });

        [Test]
        public void SplitObjectsAtDistance_Notes_ZeroDistance([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)0,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_BigDistance([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)1000,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_Start()
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)10,
                from: LengthedObjectTarget.Start,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.Time + 10 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_End()
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)10,
                from: LengthedObjectTarget.End,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.EndTime - 10 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_EmptyCollection([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: Array.Empty<Note>(),
            ratio: 0.5,
            lengthType: TimeSpanType.Midi,
            from: from,
            expectedObjects: Array.Empty<Note>());

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_Nulls([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: new[] { default(Note), default(Note) },
            ratio: 0.5,
            lengthType: TimeSpanType.Midi,
            from: from,
            expectedObjects: new[] { default(Note), default(Note) });

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_ZeroRatio([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 0.0,
                lengthType: TimeSpanType.Midi,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_FullLengthRatio([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 1.0,
                lengthType: TimeSpanType.Midi,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_Start()
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 0.1,
                lengthType: TimeSpanType.Midi,
                from: LengthedObjectTarget.Start,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.Time + 100 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_End()
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 0.1,
                lengthType: TimeSpanType.Midi,
                from: LengthedObjectTarget.End,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.EndTime - 100 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_EmptyCollection([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: Array.Empty<Chord>(),
            distance: (MidiTimeSpan)100,
            from: from,
            expectedObjects: Array.Empty<Chord>());

        [Test]
        public void SplitObjectsAtDistance_Chords_Nulls([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: new[] { default(Chord), default(Chord) },
            distance: (MidiTimeSpan)100,
            from: from,
            expectedObjects: new[] { default(Chord), default(Chord) });

        [Test]
        public void SplitObjectsAtDistance_Chords_ZeroDistance([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputChords(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)0,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_BigDistance([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputChords(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)1000,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_Start()
        {
            var inputObjects = CreateInputChords(1000);
            var distance = (MidiTimeSpan)10;
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: distance,
                from: LengthedObjectTarget.Start,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.Time + distance })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_End()
        {
            var inputObjects = CreateInputChords(1000);
            var distance = (MidiTimeSpan)10;
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: distance,
                from: LengthedObjectTarget.End,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.EndTime - distance })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_EmptyCollection([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: Array.Empty<Chord>(),
            ratio: 0.5,
            lengthType: TimeSpanType.Midi,
            from: from,
            expectedObjects: Array.Empty<Chord>());

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_Nulls([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: new[] { default(Chord), default(Chord) },
            ratio: 0.5,
            lengthType: TimeSpanType.Midi,
            from: from,
            expectedObjects: new[] { default(Chord), default(Chord) });

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_ZeroRatio([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputChords(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 0.0,
                lengthType: TimeSpanType.Midi,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_FullLengthRatio([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputChords(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 1.0,
                lengthType: TimeSpanType.Midi,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_Start()
        {
            var inputObjects = CreateInputChords(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 0.1,
                lengthType: TimeSpanType.Midi,
                from: LengthedObjectTarget.Start,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.Time + 100 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_End()
        {
            var inputObjects = CreateInputChords(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                ratio: 0.1,
                lengthType: TimeSpanType.Midi,
                from: LengthedObjectTarget.End,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.EndTime - 100 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_FullCheck_Simple(
            [Values(0, 10)] long firstNoteTime,
            [Values(100, 200)] long firstNoteLength,
            [Values(0, 10)] long secondNoteOffset) => SplitObjectsAtDistance_FullCheck(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, firstNoteLength, firstNoteTime),
                new Note((SevenBitNumber)50, 50, firstNoteTime + firstNoteLength + secondNoteOffset),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            distance: (MidiTimeSpan)10,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 10, firstNoteTime),
                new Note((SevenBitNumber)70, firstNoteLength - 10, firstNoteTime + 10),
                new Note((SevenBitNumber)50, 10, firstNoteTime + firstNoteLength + secondNoteOffset),
                new Note((SevenBitNumber)50, 40, firstNoteTime + firstNoteLength + secondNoteOffset + 10),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), firstNoteTime),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), firstNoteTime + 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), firstNoteTime + 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), firstNoteTime + firstNoteLength),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), firstNoteTime + firstNoteLength + secondNoteOffset),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), firstNoteTime + firstNoteLength + secondNoteOffset + 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), firstNoteTime + firstNoteLength + secondNoteOffset + 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), firstNoteTime + firstNoteLength + secondNoteOffset + 50),
            });

        [Test]
        public void SplitObjectsAtDistance_FullCheck_Overlapping() => SplitObjectsAtDistance_FullCheck(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, 100, 0),
                new Note((SevenBitNumber)50, 100, 20),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            distance: (MidiTimeSpan)10,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 10, 0),
                new Note((SevenBitNumber)70, 90, 10),
                new Note((SevenBitNumber)50, 10, 20),
                new Note((SevenBitNumber)50, 90, 30),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 20),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 30),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 120),
            });

        [Test]
        public void SplitObjectsAtDistance_FullCheck_Filter() => SplitObjectsAtDistance_FullCheck(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, 100, 0),
                new Note((SevenBitNumber)50, 50, 200),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            distance: (MidiTimeSpan)10,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 10, 0),
                new Note((SevenBitNumber)70, 90, 10),
                new Note((SevenBitNumber)50, 50, 200),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 200),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 250),
            },
            filter: o => (o as Note)?.NoteNumber != 50);

        [Test]
        public void SplitObjectsAtDistance_FullCheck_TimedEvents() => SplitObjectsAtDistance_FullCheck(
            inputObjects: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
            },
            objectType: ObjectType.TimedEvent,
            distance: (MidiTimeSpan)10,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
            });

        [Test]
        public void SplitObjectsAtDistance_FullCheck_Mixed() => SplitObjectsAtDistance_FullCheck(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 100, 0),
                new TimedEvent(new TextEvent("A"), 50),
                new Note((SevenBitNumber)50, 50, 200),
                new Chord(
                    new Note((SevenBitNumber)30, 50, 300),
                    new Note((SevenBitNumber)40, 60, 300)),
                new TimedEvent(new TextEvent("B"), 400),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
            distance: (MidiTimeSpan)10,
            from: LengthedObjectTarget.Start,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 10, 0),
                new Note((SevenBitNumber)70, 90, 10),

                new TimedEvent(new TextEvent("A"), 50),

                new Note((SevenBitNumber)50, 10, 200),
                new Note((SevenBitNumber)50, 40, 210),

                new Chord(
                    new Note((SevenBitNumber)30, 10, 300),
                    new Note((SevenBitNumber)40, 10, 300)),
                new Chord(
                    new Note((SevenBitNumber)30, 40, 310),
                    new Note((SevenBitNumber)40, 50, 310)),

                new TimedEvent(new TextEvent("B"), 400),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new TextEvent("A"), 50),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 200),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 210),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 210),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 250),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity), 300),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)40, Note.DefaultVelocity), 300),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity), 310),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)40, Note.DefaultOffVelocity), 310),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity), 310),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)40, Note.DefaultVelocity), 310),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity), 350),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)40, Note.DefaultOffVelocity), 360),

                new TimedEvent(new TextEvent("B"), 400),
            },
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });

        [Test]
        public void SplitObjectsAtDistance_FullCheck_ByRatio_Simple(
            [Values(0, 10)] long firstNoteTime,
            [Values(100, 200)] long firstNoteLength,
            [Values(0, 10)] long secondNoteOffset) => SplitObjectsAtDistance_ByRatio_FullCheck(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, firstNoteLength, firstNoteTime),
                new Note((SevenBitNumber)50, 50, firstNoteTime + firstNoteLength + secondNoteOffset),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            ratio: 0.1,
            lengthType: TimeSpanType.Midi,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, (int)(firstNoteLength * 0.1), firstNoteTime),
                new Note((SevenBitNumber)70, firstNoteLength - (int)(firstNoteLength * 0.1), firstNoteTime + (int)(firstNoteLength * 0.1)),
                new Note((SevenBitNumber)50, 5, firstNoteTime + firstNoteLength + secondNoteOffset),
                new Note((SevenBitNumber)50, 45, firstNoteTime + firstNoteLength + secondNoteOffset + 5),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), firstNoteTime),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), firstNoteTime + (int)(firstNoteLength * 0.1)),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), firstNoteTime + (int)(firstNoteLength * 0.1)),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), firstNoteTime + firstNoteLength),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), firstNoteTime + firstNoteLength + secondNoteOffset),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), firstNoteTime + firstNoteLength + secondNoteOffset + 5),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), firstNoteTime + firstNoteLength + secondNoteOffset + 5),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), firstNoteTime + firstNoteLength + secondNoteOffset + 50),
            });

        [Test]
        public void SplitObjectsAtDistance_FullCheck_ByRatio_Overlapping() => SplitObjectsAtDistance_ByRatio_FullCheck(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, 100, 0),
                new Note((SevenBitNumber)50, 100, 20),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            ratio: 0.1,
            lengthType: TimeSpanType.Midi,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 10, 0),
                new Note((SevenBitNumber)70, 90, 10),
                new Note((SevenBitNumber)50, 10, 20),
                new Note((SevenBitNumber)50, 90, 30),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 20),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 30),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 30),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 120),
            });
        
        [Test]
        public void SplitObjectsAtDistance_FullCheck_ByRatio_Filter() => SplitObjectsAtDistance_ByRatio_FullCheck(
            inputObjects: new[]
            {
                new Note((SevenBitNumber)70, 100, 0),
                new Note((SevenBitNumber)50, 50, 200),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            ratio: 0.1,
            lengthType: TimeSpanType.Midi,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new Note((SevenBitNumber)70, 10, 0),
                new Note((SevenBitNumber)70, 90, 10),
                new Note((SevenBitNumber)50, 50, 200),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 200),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 250),
            },
            filter: o => (o as Note)?.NoteNumber != 50);
        
        [Test]
        public void SplitObjectsAtDistance_FullCheck_ByRatio_TimedEvents() => SplitObjectsAtDistance_ByRatio_FullCheck(
            inputObjects: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
            },
            objectType: ObjectType.TimedEvent,
            ratio: 0.1,
            lengthType: TimeSpanType.Midi,
            from: LengthedObjectTarget.Start,
            expectedObjects: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),
            });
        
        [Test]
        public void SplitObjectsAtDistance_FullCheck_ByRatio_Mixed() => SplitObjectsAtDistance_ByRatio_FullCheck(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 100, 0),
                new TimedEvent(new TextEvent("A"), 50),
                new Note((SevenBitNumber)50, 50, 200),
                new Chord(
                    new Note((SevenBitNumber)30, 50, 300),
                    new Note((SevenBitNumber)40, 60, 300)),
                new TimedEvent(new TextEvent("B"), 400),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
            ratio: 0.1,
            lengthType: TimeSpanType.Midi,
            from: LengthedObjectTarget.Start,
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)70, 10, 0),
                new Note((SevenBitNumber)70, 90, 10),

                new TimedEvent(new TextEvent("A"), 50),

                new Note((SevenBitNumber)50, 5, 200),
                new Note((SevenBitNumber)50, 45, 205),

                new Chord(
                    new Note((SevenBitNumber)30, 6, 300),
                    new Note((SevenBitNumber)40, 6, 300)),
                new Chord(
                    new Note((SevenBitNumber)30, 44, 306),
                    new Note((SevenBitNumber)40, 54, 306)),

                new TimedEvent(new TextEvent("B"), 400),
            },
            expectedTimedEvents: new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 0),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 10),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), 10),
                new TimedEvent(new TextEvent("A"), 50),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), 100),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 200),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 205),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 205),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, Note.DefaultOffVelocity), 250),

                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity), 300),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)40, Note.DefaultVelocity), 300),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity), 306),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)40, Note.DefaultOffVelocity), 306),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity), 306),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)40, Note.DefaultVelocity), 306),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity), 350),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)40, Note.DefaultOffVelocity), 360),

                new TimedEvent(new TextEvent("B"), 400),
            },
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2
                }
            });
        
        #endregion

        #region Private methods

        private void SplitObjectsAtDistance(
            ICollection<ITimedObject> inputObjects,
            ITimeSpan distance,
            LengthedObjectTarget from,
            ICollection<ITimedObject> expectedObjects,
            Predicate<ITimedObject> filter = null)
        {
            var actualObjects = inputObjects.SplitObjectsAtDistance(distance, from, TempoMap.Default, filter).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                "Invalid result objects.");
        }

        private void SplitObjectsAtDistance_FullCheck(
            ICollection<ITimedObject> inputObjects,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            ICollection<ITimedObject> expectedObjects,
            ICollection<TimedEvent> expectedTimedEvents,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            var actualObjects = inputObjects.SplitObjectsAtDistance(distance, from, TempoMap.Default, filter).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                "Invalid result objects.");

            //

            var trackChunk = inputObjects.ToTrackChunk();
            trackChunk.SplitObjectsAtDistance(objectType, distance, from, TempoMap.Default, objectDetectionSettings, filter);
            var actualTimedEvents = trackChunk.GetTimedEvents();
            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Invalid track chunk.");

            //

            var trackChunks = new[] { inputObjects.ToTrackChunk() };
            trackChunks.SplitObjectsAtDistance(objectType, distance, from, TempoMap.Default, objectDetectionSettings, filter);
            actualTimedEvents = trackChunks.Single().GetTimedEvents();
            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Invalid track chunks.");

            //

            var midiFile = inputObjects.ToFile();
            midiFile.SplitObjectsAtDistance(objectType, distance, from, objectDetectionSettings, filter);
            actualTimedEvents = midiFile.GetTimedEvents();
            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Invalid file.");
        }

        private void SplitObjectsAtDistance(
            ICollection<ITimedObject> inputObjects,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            ICollection<ITimedObject> expectedObjects,
            Predicate<ITimedObject> filter = null)
        {
            var actualObjects = inputObjects.SplitObjectsAtDistance(ratio, lengthType, from, TempoMap.Default, filter).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                "Invalid result objects.");
        }

        private void SplitObjectsAtDistance_ByRatio_FullCheck(
            ICollection<ITimedObject> inputObjects,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            ICollection<ITimedObject> expectedObjects,
            ICollection<TimedEvent> expectedTimedEvents,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            var actualObjects = inputObjects.SplitObjectsAtDistance(ratio, lengthType, from, TempoMap.Default, filter).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                "Invalid result objects.");

            //

            var trackChunk = inputObjects.ToTrackChunk();
            trackChunk.SplitObjectsAtDistance(objectType, ratio, lengthType, from, TempoMap.Default, objectDetectionSettings, filter);
            var actualTimedEvents = trackChunk.GetTimedEvents();
            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Invalid track chunk.");

            //

            var trackChunks = new[] { inputObjects.ToTrackChunk() };
            trackChunks.SplitObjectsAtDistance(objectType, ratio, lengthType, from, TempoMap.Default, objectDetectionSettings, filter);
            actualTimedEvents = trackChunks.Single().GetTimedEvents();
            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Invalid track chunks.");

            //

            var midiFile = inputObjects.ToFile();
            midiFile.SplitObjectsAtDistance(objectType, ratio, lengthType, from, objectDetectionSettings, filter);
            actualTimedEvents = midiFile.GetTimedEvents();
            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Invalid file.");
        }

        #endregion
    }
}
