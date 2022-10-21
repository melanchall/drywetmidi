using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class GetObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetObjects_TimedEvents_Empty() => GetObjects_TimedEvents(
            inputObjects: Enumerable.Empty<ITimedObject>(),
            outputObjects: Enumerable.Empty<ITimedObject>());

        [Test]
        public void GetObjects_TimedEvents_FromTimedEvents_Mixed() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            });

        [Test]
        public void GetObjects_TimedEvents_FromTimedEvents_Mixed_Unordered() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOffEvent(), 50),
                new TimedEvent(new NoteOnEvent(), 20),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            });

        [Test]
        public void GetObjects_TimedEvents_FromTimedEvents_OnlyNoteEvents() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            });

        [Test]
        public void GetObjects_TimedEvents_FromTimedEvents_OnlyNonNoteEvents() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
            });

        [Test]
        public void GetObjects_TimedEvents_FromNotes() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)50, 80, 100),
                new Note((SevenBitNumber)10, 20, 140),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 100),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity), 140),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)10, SevenBitNumber.MinValue), 160),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 180),
            });

        [Test]
        public void GetObjects_TimedEvents_FromNotesAndTimedEvents() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)50, 80, 180),
                new TimedEvent(new TextEvent("A"), 40),
                new Note((SevenBitNumber)10, 20, 140),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 40),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity), 140),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)10, SevenBitNumber.MinValue), 160),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 180),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 260),
            });

        [Test]
        public void GetObjects_TimedEvents_FromChords() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)50, 80, 100),
                    new Note((SevenBitNumber)10, 20, 50)),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity), 50),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)10, SevenBitNumber.MinValue), 70),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 180),
            });

        [Test]
        public void GetObjects_TimedEvents_FromChordsAndNotesAndTimedEvents() => GetObjects_TimedEvents(
            inputObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)50, 80, 100),
                    new Note((SevenBitNumber)10, 20, 50)),
                new TimedEvent(new TextEvent("A"), 30),
                new Note((SevenBitNumber)90, 23334, 223),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 30),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity), 50),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)10, SevenBitNumber.MinValue), 70),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, Note.DefaultVelocity), 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue), 180),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity), 223),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue), 223 + 23334),
            });

        [Test]
        public void EnumerateObjects_TimedEvents_Empty() => EnumerateObjects_TimedEvents(
            inputEvents: Enumerable.Empty<MidiEvent>(),
            outputObjects: Enumerable.Empty<ITimedObject>());

        [Test]
        public void EnumerateObjects_TimedEvents_FromTimedEvents_Mixed() => EnumerateObjects_TimedEvents(
            inputEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent() { DeltaTime = 20 },
                new NoteOffEvent() { DeltaTime = 30 },
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            });

        [Test]
        public void EnumerateObjects_TimedEvents_FromTimedEvents_OnlyNoteEvents() => EnumerateObjects_TimedEvents(
            inputEvents: new MidiEvent[]
            {
                new NoteOnEvent() { DeltaTime = 20 },
                new NoteOffEvent() { DeltaTime = 30 },
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 50),
            });

        [Test]
        public void EnumerateObjects_TimedEvents_FromTimedEvents_OnlyNonNoteEvents() => EnumerateObjects_TimedEvents(
            inputEvents: new MidiEvent[]
            {
                new TextEvent("A"),
            },
            outputObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
            });

        #endregion

        #region Private methods

        private void GetObjects_TimedEvents(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects) =>
            GetObjects(
                inputObjects,
                outputObjects,
                ObjectType.TimedEvent,
                new ObjectDetectionSettings());

        private void EnumerateObjects_TimedEvents(
            IEnumerable<MidiEvent> inputEvents,
            IEnumerable<ITimedObject> outputObjects) =>
            EnumerateObjects(
                inputEvents,
                outputObjects,
                ObjectType.TimedEvent,
                new ObjectDetectionSettings());

        #endregion
    }
}
