using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class BuildObjectsUtilitiesTests
    {
        #region Test methods

        [Test]
        public void BuildTimedEvents_Empty()
        {
            CheckBuildingTimedEvents(
                inputObjects: Enumerable.Empty<ITimedObject>(),
                outputObjects: Enumerable.Empty<ITimedObject>());
        }

        [Test]
        public void BuildTimedEvents_FromTimedEvents_Mixed()
        {
            CheckBuildingTimedEvents(
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
        }

        [Test]
        public void BuildTimedEvents_FromTimedEvents_Mixed_Unordered()
        {
            CheckBuildingTimedEvents(
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
        }

        [Test]
        public void BuildTimedEvents_FromTimedEvents_OnlyNoteEvents()
        {
            CheckBuildingTimedEvents(
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
        }

        [Test]
        public void BuildTimedEvents_FromTimedEvents_OnlyNonNoteEvents()
        {
            CheckBuildingTimedEvents(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                });
        }

        [Test]
        public void BuildTimedEvents_FromNotes()
        {
            CheckBuildingTimedEvents(
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
        }

        [Test]
        public void BuildTimedEvents_FromNotesAndTimedEvents()
        {
            CheckBuildingTimedEvents(
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
        }

        [Test]
        public void BuildTimedEvents_FromChords()
        {
            CheckBuildingTimedEvents(
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
        }

        [Test]
        public void BuildTimedEvents_FromChordsAndNotesAndTimedEvents()
        {
            CheckBuildingTimedEvents(
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
        }

        #endregion

        #region Private methods

        private void CheckBuildingTimedEvents(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects)
        {
            CheckObjectsBuilding(
                inputObjects,
                outputObjects,
                ObjectType.TimedEvent,
                new ObjectsBuildingSettings
                {
                });
        }

        #endregion
    }
}
