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
        public void BuildTimedEventsAndNotes_Empty()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: Enumerable.Empty<ITimedObject>(),
                outputObjects: Enumerable.Empty<ITimedObject>());
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromTimedEvents_Mixed()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOnEvent(), 20),
                    new TimedEvent(new NoteOffEvent(), 50),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note(SevenBitNumber.MinValue, 30, 20) { Velocity = SevenBitNumber.MinValue }
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromTimedEvents_Mixed_Unordered()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new NoteOffEvent(), 50),
                    new TimedEvent(new NoteOnEvent(), 20),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note(SevenBitNumber.MinValue, 30, 20) { Velocity = SevenBitNumber.MinValue }
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromTimedEvents_OnlyNoteEvents()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(), 20),
                    new TimedEvent(new NoteOffEvent(), 50),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note(SevenBitNumber.MinValue, 30, 20) { Velocity = SevenBitNumber.MinValue }
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromTimedEvents_OnlyNonNoteEvents()
        {
            CheckBuildingTimedEventsAndNotes(
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
        public void BuildTimedEventsAndNotes_FromNotes()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50, 80, 100),
                    new Note((SevenBitNumber)10, 20, 140),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50, 80, 100),
                    new Note((SevenBitNumber)10, 20, 140),
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromNotesAndTimedEvents()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)50, 80, 180),
                    new TimedEvent(new TextEvent("A"), 40),
                    new Note((SevenBitNumber)10, 20, 140),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 40),
                    new Note((SevenBitNumber)10, 20, 140),
                    new Note((SevenBitNumber)50, 80, 180),
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromChords()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)50, 80, 100),
                        new Note((SevenBitNumber)10, 20, 50)),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)10, 20, 50),
                    new Note((SevenBitNumber)50, 80, 100),
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_FromChordsAndNotesAndTimedEvents()
        {
            CheckBuildingTimedEventsAndNotes(
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
                    new Note((SevenBitNumber)10, 20, 50),
                    new Note((SevenBitNumber)50, 80, 100),
                    new Note((SevenBitNumber)90, 23334, 223),
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_AllProcessed()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { Channel = (FourBitNumber)1 }, 20),
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 40),
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 }, 70),
                    new TimedEvent(new CuePointEvent("Point"), 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { Channel = (FourBitNumber)1 }, 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 }, 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)1 }, 100),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new Note((SevenBitNumber)1, 80, 10) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)100 },
                    new Note((SevenBitNumber)2, 80, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new Note((SevenBitNumber)2, 20, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 },
                    new TimedEvent(new CuePointEvent("Point"), 80),
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_NotAllProcessed()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70) { Channel = (FourBitNumber)1 }, 20),
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 40),
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)1) { Channel = (FourBitNumber)10 }, 70),
                    new TimedEvent(new CuePointEvent("Point"), 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)3, (SevenBitNumber)1) { Channel = (FourBitNumber)1 }, 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)10 }, 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { Channel = (FourBitNumber)1 }, 90),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)78, (SevenBitNumber)0) { Channel = (FourBitNumber)11 }, 100),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(1234), 0),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { Channel = (FourBitNumber)1 }, 10),
                    new Note((SevenBitNumber)2, 70, 20) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)70 },
                    new TimedEvent(new PitchBendEvent(123), 30),
                    new TimedEvent(new MarkerEvent("Marker"), 40),
                    new Note((SevenBitNumber)3, 40, 40) { Channel = (FourBitNumber)1, Velocity = (SevenBitNumber)1, OffVelocity = (SevenBitNumber)1 },
                    new TimedEvent(new MarkerEvent("Marker 2"), 50),
                    new TimedEvent(new TextEvent("Text"), 60),
                    new TimedEvent(new TextEvent("Text 2"), 70),
                    new Note((SevenBitNumber)2, 10, 70) { Channel = (FourBitNumber)10, Velocity = (SevenBitNumber)1 },
                    new TimedEvent(new CuePointEvent("Point"), 80),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)78, (SevenBitNumber)0) { Channel = (FourBitNumber)11 }, 100),
                });
        }

        [Test]
        public void BuildTimedEventsAndNotes_SameNotesInTail()
        {
            CheckBuildingTimedEventsAndNotes(
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100), 10),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)70), 20),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)1), 20),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)0), 20),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0), 30),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0), 40),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)1, 30, 10) { Velocity = (SevenBitNumber)100 },
                    new Note((SevenBitNumber)2, 0, 20) { Velocity = (SevenBitNumber)70, OffVelocity = (SevenBitNumber)1 },
                    new Note((SevenBitNumber)2, 10, 20) { Velocity = (SevenBitNumber)0 },
                });
        }

        #endregion

        #region Private methods

        private void CheckBuildingTimedEventsAndNotes(
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects)
        {
            CheckObjectsBuilding(
                inputObjects,
                outputObjects,
                new ObjectsBuildingSettings
                {
                    BuildTimedEvents = true,
                    BuildNotes = true
                });
        }

        #endregion
    }
}
