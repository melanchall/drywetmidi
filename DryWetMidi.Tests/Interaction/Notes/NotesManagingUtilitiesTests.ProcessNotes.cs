using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[0],
                action: n => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_ProcessNoteNumber([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => n.NoteNumber = (SevenBitNumber)40,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MinValue),
                    new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue),
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_ProcessVelocity([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => n.Velocity = (SevenBitNumber)40,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)40 },
                    new NoteOffEvent(),
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_ProcessOffVelocity([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => n.OffVelocity = (SevenBitNumber)40,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { Velocity = (SevenBitNumber)40 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                },
                action: n => n.Time = 10,
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_ProcessChannel([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => n.Channel = (FourBitNumber)4,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneEvent_NoteOn_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new NoteOnEvent() },
                action: n => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneEvent_NoteOff_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new NoteOffEvent() },
                action: n => { },
                expectedMidiEvents: new[] { new NoteOffEvent() });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneEvent_NonNote_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new TextEvent() },
                action: n => { },
                expectedMidiEvents: new[] { new TextEvent() });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                action: n => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => n.NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new TextEvent("A"),
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_Processing_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => n.Time = 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent()
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_Processing_Time_HintNone([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => n.Time = 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                hint: NoteProcessingHint.None);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => n.Length = 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent { DeltaTime = 100 }
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_Processing_Length_HintNone([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => n.Length = 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                hint: NoteProcessingHint.None);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_OneNote_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B") { DeltaTime = 1000 },
                },
                action: n =>
                {
                    n.Time = 50;
                    n.Length = 100;
                },
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("B") { DeltaTime = 850 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleEvents_NoNotes_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A")
                },
                action: n => { },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A")
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleEvents_OneNote_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => { },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleNotes_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent()
                },
                action: n => { },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent()
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleNotes_NoEvents_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 1000 },
                },
                action: n => n.Channel = (FourBitNumber)3,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)3 },
                    new NoteOffEvent { DeltaTime = 1000, Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50, Channel = (FourBitNumber)3 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 1000, Channel = (FourBitNumber)3 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleNotes_WithEvents_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new ControlChangeEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 1000 },
                },
                action: n => n.Channel = (FourBitNumber)3,
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { Channel = (FourBitNumber)3 },
                    new NoteOffEvent { DeltaTime = 1000, Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50, Channel = (FourBitNumber)3 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 1000, Channel = (FourBitNumber)3 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleNotes_Processing_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n => n.Time = n.NoteNumber == 0 ? 100 : 10,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent { DeltaTime = 90 },
                    new NoteOffEvent(),
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleNotes_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n => n.Length = n.NoteNumber == 0 ? 100 : 10,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithoutPredicate_MultipleNotes_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n =>
                {
                    n.Time = n.NoteNumber == 0 ? 100 : 50;
                    n.Length = n.NoteNumber == 0 ? 1000 : 500;
                },
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 450 },
                    new NoteOffEvent { DeltaTime = 550 },
                });
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[0],
                action: n => { },
                match: n => n.NoteNumber == 70,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_Matched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                action: n => { },
                match: n => true,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_NotMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                action: n => { },
                match: n => false,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_Matched_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => n.NoteNumber = (SevenBitNumber)23,
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_Matched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => n.Time = 100,
                match: n => true,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent()
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_Matched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => n.Length = 100,
                match: n => true,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_Matched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n =>
                {
                    n.Time = 50;
                    n.Length = 100;
                },
                match: n => true,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 100 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => n.NoteNumber = (SevenBitNumber)23,
                match: n => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => n.Time = 100,
                match: n => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => n.Length = 100,
                match: n => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n =>
                {
                    n.Time = 50;
                    n.Length = 100;
                },
                match: n => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n => { },
                match: n => true,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n => { },
                match: n => n.NoteNumber == 70,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: e => { },
                match: e => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.NoteNumber = (SevenBitNumber)23,
                match: n => n.NoteNumber < 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)23, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)23, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Time = 100,
                match: n => n.NoteNumber < 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("A") { DeltaTime = 900 },
                    new ControlChangeEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Length = 100,
                match: n => n.NoteNumber < 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("A") { DeltaTime = 900 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new ControlChangeEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n =>
                {
                    n.Time = 50;
                    n.Length = 100;
                },
                match: n => true,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("A") { DeltaTime = 850 },
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Velocity = (SevenBitNumber)23,
                match: n => n.NoteNumber > 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)23),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_Time_1([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Time = 100,
                match: n => n.NoteNumber > 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOffEvent { DeltaTime = 900 },
                    new TextEvent("A"),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_Time_2([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Time = 10,
                match: n => n.Time == 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 90 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Length = 10000,
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A") { DeltaTime = 1000 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent(),
                    new NoteOffEvent { DeltaTime = 9000 },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n =>
                {
                    n.Time = 100;
                    n.Length = 10000;
                },
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new TextEvent("A") { DeltaTime = 900 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent(),
                    new NoteOffEvent { DeltaTime = 9100 },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Velocity = (SevenBitNumber)23,
                match: n => n.NoteNumber > 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Time = 10000,
                match: n => n.NoteNumber > 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n => n.Length = 10000,
                match: n => n.NoteNumber > 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessNotes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                action: n =>
                {
                    n.Time = 600;
                    n.Length = 10000;
                },
                match: n => n.NoteNumber > 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new ControlChangeEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: n => { },
                expectedMidiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_NoProcessing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                action: n => { },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)70,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)70 },
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)70 },
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)70 },
                        new TextEvent("B"),
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)70 },
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing_Time([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                },
                action: n => n.Time = 100,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new NoteOnEvent { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing_Length([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("B") { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                },
                action: n => n.Length = 50,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 50 },
                        new TextEvent("B") { DeltaTime = 50 },
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing_TimeAndLength([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A") { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("B") { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                },
                action: n =>
                {
                    n.Time = 150;
                    n.Length = 200;
                },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A") { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B") { DeltaTime = 100 },
                        new NoteOnEvent { DeltaTime = 50 },
                        new NoteOffEvent { DeltaTime = 200 },
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing_Channel([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                },
                action: n => n.Channel = (FourBitNumber)5,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Channel = (FourBitNumber)5 },
                        new TextEvent("A"),
                        new NoteOffEvent { Channel = (FourBitNumber)5 },
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing_Velocity([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.Velocity = (SevenBitNumber)50,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Velocity = (SevenBitNumber)50 },
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOnEvent { Velocity = (SevenBitNumber)50 },
                        new NoteOffEvent(),
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_OneNote_Processing_OffVelocity([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.OffVelocity = (SevenBitNumber)50,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { Velocity = (SevenBitNumber)50 },
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_NoProcessing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                action: n => { },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_NoteNumber([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)80,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)80 },
                        new TextEvent("A"),
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)80 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Velocity([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Velocity = (SevenBitNumber)80,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Velocity = (SevenBitNumber)80 },
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)80 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_OffVelocity([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.OffVelocity = (SevenBitNumber)80,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent { Velocity = (SevenBitNumber)80 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Velocity = (SevenBitNumber)80 },
                        new ControlChangeEvent()
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Channel([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Channel = (FourBitNumber)8,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Channel = (FourBitNumber)8 },
                        new TextEvent("A"),
                        new NoteOffEvent { Channel = (FourBitNumber)8 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)8 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)8 },
                        new ControlChangeEvent()
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Time([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Time = 100,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Length([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Length = 100,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new ControlChangeEvent(),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_TimeAndLength([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                        new TextEvent("A") { DeltaTime = 10 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n =>
                {
                    n.Time = 50;
                    n.Length = 100;
                },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A") { DeltaTime = 10 },
                        new NoteOnEvent { DeltaTime = 40 },
                        new NoteOffEvent { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    },
                });
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: n => { },
                match: n => true,
                expectedMidiEvents: new MidiEvent[0][],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                },
                action: n => { },
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                action: n => { },
                match: n => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)70,
                match: n => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("B"),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)70,
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)70 },
                        new TextEvent("B"),
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)70 },
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.Time = 100,
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                        new NoteOnEvent { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Length([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new TextEvent("B") { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.Length = 50,
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent { DeltaTime = 50 },
                        new TextEvent("B") { DeltaTime = 50 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 50 },
                    },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing_TimeAndLength([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A") { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B") { DeltaTime = 100 },
                    },
                },
                action: n =>
                {
                    n.Time = 150;
                    n.Length = 200;
                },
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A") { DeltaTime = 100 },
                        new NoteOnEvent { DeltaTime = 50 },
                        new NoteOffEvent { DeltaTime = 200 },
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B") { DeltaTime = 100 },
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Channel([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.Channel = (FourBitNumber)5,
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Channel = (FourBitNumber)5 },
                        new NoteOffEvent { Channel = (FourBitNumber)5 },
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Velocity([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                },
                action: n => n.Velocity = (SevenBitNumber)50,
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new TextEvent("A"),
                        new TextEvent("B"),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { Velocity = (SevenBitNumber)50 },
                        new NoteOffEvent(),
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_OneNote_Matched_Processing_OffVelocity([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                },
                action: n => n.OffVelocity = (SevenBitNumber)50,
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent { Velocity = (SevenBitNumber)50 },
                    },
                    new MidiEvent[]
                    {
                        new TextEvent("B"),
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                action: n => { },
                match: n => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                action: n => { },
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)70,
                match: n => false,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_Processing_NoteNumber([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)90, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue),
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)80,
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)80 },
                        new TextEvent("A"),
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)80 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MinValue),
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 3);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_Processing_NoteNumber([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)90, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)90, SevenBitNumber.MinValue),
                    },
                },
                action: n => n.NoteNumber = (SevenBitNumber)80,
                match: n => n.NoteNumber == 90,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new TextEvent("A"),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Time = 100,
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 100 },
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Time = 100,
                match: n => n.NoteNumber == 70,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new ControlChangeEvent(),
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_Processing_Length([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Length = 100,
                match: n => true,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new ControlChangeEvent(),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_Processing_Length([Values] bool wrapToFile)
        {
            ProcessNotes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent(),
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                action: n => n.Length = 100,
                match: n => n.NoteNumber == 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                        new ControlChangeEvent()
                    },
                },
                expectedProcessedCount: 1);
        }

        #endregion

        #region Private methods

        private void ProcessNotes_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<Note> action,
            Predicate<Note> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedProcessedCount,
                            eventsCollection.ProcessNotes(action, match, hint: hint),
                            "Invalid count of processed notes.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            expectedProcessedCount,
                            trackChunk.ProcessNotes(action, match, hint: hint),
                            "Invalid count of processed notes.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessNotes_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            action,
                            match,
                            new[] { expectedMidiEvents },
                            expectedProcessedCount,
                            hint);
                    }
                    break;
            }
        }

        private void ProcessNotes_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<Note> action,
            ICollection<MidiEvent> expectedMidiEvents,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            var notesCount = midiEvents.GetNotes().Count();

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);

                        Assert.AreEqual(
                            notesCount,
                            eventsCollection.ProcessNotes(action, hint: hint),
                            "Invalid count of processed notes.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        Assert.AreEqual(
                            notesCount,
                            trackChunk.ProcessNotes(action, hint: hint),
                            "Invalid count of processed notes.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                    {
                        ProcessNotes_TrackChunks_WithoutPredicate(
                            false,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents },
                            hint);
                    }
                    break;
                case ContainerType.File:
                    {
                        ProcessNotes_TrackChunks_WithoutPredicate(
                            true,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents },
                            hint);
                    }
                    break;
            }
        }

        private void ProcessNotes_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Note> action,
            Predicate<Note> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessNotes(action, match, hint: hint),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessNotes(action, match, hint: hint),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessNotes_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Note> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            NoteProcessingHint hint = NoteProcessingHint.Default)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var notesCount = trackChunks.GetNotes().Count();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    notesCount,
                    midiFile.ProcessNotes(action, hint: hint),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    notesCount,
                    trackChunks.ProcessNotes(action, hint: hint),
                    "Invalid count of processed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
