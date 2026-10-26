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
    public sealed partial class MergerTests
    {
        #region Test methods

        [Test]
        public void MergeObjects_Objects_Empty() => MergeObjects_Objects(
            timedObjects: Array.Empty<ITimedObject>(),
            settings: null,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void MergeObjects_Objects_TimedEvents() => MergeObjects_Objects(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 30),
                new TimedEvent(new ControlChangeEvent(), 30),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 40),
            },
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 30),
                new TimedEvent(new ControlChangeEvent(), 30),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 40),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_NoMerging() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 21),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 21),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_1() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 40, 0),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_2() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 30),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)10
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 50, 0),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_3() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new Note((SevenBitNumber)20, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 60, 0),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_4() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new Note((SevenBitNumber)25, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 40, 0),
                new Note((SevenBitNumber)25, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_5() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new Note((SevenBitNumber)20, 20, 40) { Channel = (FourBitNumber)4 },
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 40, 0),
                new Note((SevenBitNumber)20, 20, 40) { Channel = (FourBitNumber)4 },
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_6() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)30, 20, 20),
                new Note((SevenBitNumber)20, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)20
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 60, 0),
                new Note((SevenBitNumber)30, 20, 20),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndNotes_Merging_7() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new TimedEvent(new TextEvent("B"), 10),
                new Note((SevenBitNumber)30, 20, 20),
                new TimedEvent(new TextEvent("C"), 30),
                new Note((SevenBitNumber)20, 20, 40),
                new TimedEvent(new TextEvent("D"), 50),
                new Note((SevenBitNumber)30, 20, 60),
                new TimedEvent(new TextEvent("E"), 70),
            },
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)20
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 60, 0),
                new TimedEvent(new TextEvent("B"), 10),
                new Note((SevenBitNumber)30, 60, 20),
                new TimedEvent(new TextEvent("C"), 30),
                new TimedEvent(new TextEvent("D"), 50),
                new TimedEvent(new TextEvent("E"), 70),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndChords_NoMerging_1() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 61),
                    new Note((SevenBitNumber)30, 30, 61)),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 61),
                    new Note((SevenBitNumber)30, 30, 61)),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndChords_NoMerging_2() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)40, 30, 60),
                    new Note((SevenBitNumber)20, 60, 60)),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)40, 30, 60),
                    new Note((SevenBitNumber)20, 60, 60)),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndChords_Merging_1() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 61),
                    new Note((SevenBitNumber)30, 30, 61)),
            },
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)1
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 121, 0),
                    new Note((SevenBitNumber)30, 91, 0)),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndChords_Merging_2() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)20, 20, 0),
                    new Note((SevenBitNumber)30, 30, 10)),
                new Chord(
                    new Note((SevenBitNumber)20, 20, 40),
                    new Note((SevenBitNumber)30, 100, 50)),
            },
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)1
            },
            expectedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 140, 10)),
            });

        [Test]
        public void MergeObjects_Objects_TimedEventsAndChords_Merging_3() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("X"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 20, 0),
                    new Note((SevenBitNumber)30, 30, 10)),
                new TimedEvent(new TextEvent("A"), 5),
                new Chord(
                    new Note((SevenBitNumber)20, 20, 40),
                    new Note((SevenBitNumber)30, 100, 50)),
                new TimedEvent(new TextEvent("B"), 200),
            },
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)1
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("X"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 140, 10)),
                new TimedEvent(new TextEvent("A"), 5),
                new TimedEvent(new TextEvent("B"), 200),
            });

        [Test]
        public void MergeObjects_Objects_Rests_NoMerging_1() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new Rest(10, 30, (0, (SevenBitNumber)70)),
                new Rest(40, 30, (0, 0)),
                new Rest(300, 10, (0, (SevenBitNumber)70)),
                new Rest(310, 30, (0, (SevenBitNumber)80)),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new Rest(10, 30, (0, (SevenBitNumber)70)),
                new Rest(40, 30, (0, 0)),
                new Rest(300, 10, (0, (SevenBitNumber)70)),
                new Rest(310, 30, (0, (SevenBitNumber)80)),
            });

        [Test]
        public void MergeObjects_Objects_Rests_NoMerging_2() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new Rest(10, 30, (0, (SevenBitNumber)70)),
                new Rest(40, 30, ((FourBitNumber)2, (SevenBitNumber)70)),
                new Rest(300, 10, ((FourBitNumber)2, 0)),
                new Rest(310, 30, ((FourBitNumber)3, 0)),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new Rest(10, 30, (0, (SevenBitNumber)70)),
                new Rest(40, 30, ((FourBitNumber) 2,(SevenBitNumber) 70)),
                new Rest(300, 10, ((FourBitNumber)2, 0)),
                new Rest(310, 30, ((FourBitNumber)3, 0)),
            });

        [Test]
        public void MergeObjects_Objects_Rests_Merging_1() => MergeObjects_Objects(
            timedObjects: new ITimedObject[]
            {
                new Rest(10, 30, (0, (SevenBitNumber)70)),
                new Rest(40, 30, (0, (SevenBitNumber)70)),
                new Rest(300, 10, ((FourBitNumber)2, 0)),
                new Rest(310, 30, ((FourBitNumber)2, 0)),
            },
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new Rest(10, 60, (0, (SevenBitNumber)70)),
                new Rest(300, 40, ((FourBitNumber)2, 0)),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_Empty() => MergeObjects_TrackChunkOrFile(
            timedObjects: Array.Empty<ITimedObject>(),
            objectType: ObjectType.Note,
            settings: null,
            expectedObjects: Array.Empty<ITimedObject>());

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEvents() => MergeObjects_TrackChunkOrFile(
            timedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 30),
                new TimedEvent(new ControlChangeEvent(), 30),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 40),
            },
            objectType: ObjectType.TimedEvent,
            settings: null,
            expectedObjects: new[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new TimedEvent(new NoteOnEvent(), 20),
                new TimedEvent(new NoteOffEvent(), 30),
                new TimedEvent(new ControlChangeEvent(), 30),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 40),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_NoMerging() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 21),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.Note,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 21),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_1() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 40, 0),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_2() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 30),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)10
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 50, 0),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_3() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new Note((SevenBitNumber)20, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 60, 0),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_4() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new Note((SevenBitNumber)25, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 40, 0),
                new Note((SevenBitNumber)25, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_5() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)20, 20, 20),
                new Note((SevenBitNumber)20, 20, 40) { Channel = (FourBitNumber)4 },
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 40, 0),
                new Note((SevenBitNumber)20, 20, 40) { Channel = (FourBitNumber)4 },
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_6() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new Note((SevenBitNumber)30, 20, 20),
                new Note((SevenBitNumber)20, 20, 40),
                new TimedEvent(new ControlChangeEvent(), 100),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)20
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 60, 0),
                new Note((SevenBitNumber)30, 20, 20),
                new TimedEvent(new ControlChangeEvent(), 100),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndNotes_Merging_7() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 20, 0),
                new TimedEvent(new TextEvent("B"), 10),
                new Note((SevenBitNumber)30, 20, 20),
                new TimedEvent(new TextEvent("C"), 30),
                new Note((SevenBitNumber)20, 20, 40),
                new TimedEvent(new TextEvent("D"), 50),
                new Note((SevenBitNumber)30, 20, 60),
                new TimedEvent(new TextEvent("E"), 70),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)20
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)20, 60, 0),
                new TimedEvent(new TextEvent("B"), 10),
                new Note((SevenBitNumber)30, 60, 20),
                new TimedEvent(new TextEvent("C"), 30),
                new TimedEvent(new TextEvent("D"), 50),
                new TimedEvent(new TextEvent("E"), 70),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndChords_NoMerging_1() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 61),
                    new Note((SevenBitNumber)30, 30, 61)),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 61),
                    new Note((SevenBitNumber)30, 30, 61)),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndChords_NoMerging_2() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)40, 30, 60),
                    new Note((SevenBitNumber)20, 60, 60)),
            },
            objectType: ObjectType.Chord,
            settings: null,
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)40, 30, 60),
                    new Note((SevenBitNumber)20, 60, 60)),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndChords_Merging_1() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 60, 0)),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 61),
                    new Note((SevenBitNumber)30, 30, 61)),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)1
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 121, 0),
                    new Note((SevenBitNumber)30, 91, 0)),
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndChords_Merging_2() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)20, 20, 0),
                    new Note((SevenBitNumber)30, 30, 10)),
                new Chord(
                    new Note((SevenBitNumber)20, 20, 40),
                    new Note((SevenBitNumber)30, 100, 50)),
            },
            objectType: ObjectType.Chord,
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)1
            },
            expectedObjects: new ITimedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 140, 10)),
            },
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10
                }
            });

        [Test]
        public void MergeObjects_TrackChunkOrFile_TimedEventsAndChords_Merging_3() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("X"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 20, 0),
                    new Note((SevenBitNumber)30, 30, 10)),
                new TimedEvent(new TextEvent("A"), 5),
                new Chord(
                    new Note((SevenBitNumber)20, 20, 40),
                    new Note((SevenBitNumber)30, 100, 50)),
                new TimedEvent(new TextEvent("B"), 200),
            },
            objectType: ObjectType.TimedEvent | ObjectType.Chord,
            settings: new ObjectsMergingSettings
            {
                Tolerance = (MidiTimeSpan)1
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("X"), 0),
                new Chord(
                    new Note((SevenBitNumber)20, 60, 0),
                    new Note((SevenBitNumber)30, 140, 10)),
                new TimedEvent(new TextEvent("A"), 5),
                new TimedEvent(new TextEvent("B"), 200),
            },
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesTolerance = 10
                }
            });

        [Test]
        public void MergeObjects_Notes() => MergeObjects_TrackChunkOrFile(
            timedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)63, 96, 0),
                new Note((SevenBitNumber)51, 96, 0),
                new Note((SevenBitNumber)63, 96, 96),
                new Note((SevenBitNumber)51, 96, 96),
                new Note((SevenBitNumber)65, 96, 192),
                new Note((SevenBitNumber)58, 96, 192),
                new Note((SevenBitNumber)65, 96, 288),
                new Note((SevenBitNumber)58, 96, 288),
                new Note((SevenBitNumber)51, 96, 384),
                new Note((SevenBitNumber)66, 96, 384),
                new Note((SevenBitNumber)51, 96, 480),
                new Note((SevenBitNumber)66, 96, 480),
            },
            objectType: ObjectType.Note,
            settings: new ObjectsMergingSettings
            {
                Tolerance = new MetricTimeSpan(0, 0, 0, 10)
            },
            expectedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)63, 192, 0),
                new Note((SevenBitNumber)51, 192, 0),
                new Note((SevenBitNumber)65, 192, 192),
                new Note((SevenBitNumber)58, 192, 192),
                new Note((SevenBitNumber)51, 192, 384),
                new Note((SevenBitNumber)66, 192, 384),
            });

        #endregion

        #region Private methods

        private void MergeObjects_Objects(
            ICollection<ITimedObject> timedObjects,
            ObjectsMergingSettings settings,
            ICollection<ITimedObject> expectedObjects)
        {
            var actualObjects = timedObjects.MergeObjects(TempoMap.Default, settings).ToArray();
            MidiAsserts.AreEqual(expectedObjects, actualObjects, "Invalid objects.");
        }

        private void MergeObjects_TrackChunkOrFile(
            ICollection<ITimedObject> timedObjects,
            ObjectType objectType,
            ObjectsMergingSettings settings,
            ICollection<ITimedObject> expectedObjects,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            var trackChunk = timedObjects.ToTrackChunk();
            trackChunk.MergeObjects(objectType, TempoMap.Default, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(expectedObjects.ToTrackChunk(), trackChunk, false, "Invalid track chunk.");

            //

            var trackChunks = new[] { timedObjects.ToTrackChunk() };
            trackChunks.MergeObjects(objectType, TempoMap.Default, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(new[] { expectedObjects.ToTrackChunk() }, trackChunks, false, "Invalid track chunks.");

            //

            var midiFile = timedObjects.ToFile();
            midiFile.MergeObjects(objectType, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(expectedObjects.ToFile(), midiFile, false, "Invalid file.");
        }

        #endregion
    }
}
