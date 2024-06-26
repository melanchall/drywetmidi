﻿using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[0],
                action: n => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_ProcessNoteNumber([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)40,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MinValue),
                    new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue),
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_ProcessVelocity([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => ((Note)n).Velocity = (SevenBitNumber)40,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)40 },
                    new NoteOffEvent(),
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_ProcessOffVelocity([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => ((Note)n).OffVelocity = (SevenBitNumber)40,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { Velocity = (SevenBitNumber)40 },
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_ProcessChannel([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                action: n => ((Note)n).Channel = (FourBitNumber)4,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneEvent_NoteOn_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new NoteOnEvent() },
                action: n => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneEvent_NoteOff_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new NoteOffEvent() },
                action: n => { },
                expectedMidiEvents: new[] { new NoteOffEvent() });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneEvent_NonNote_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new[] { new TextEvent() },
                action: n => { },
                expectedMidiEvents: new[] { new TextEvent() });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                action: n => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new TextEvent("A"),
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_Processing_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_Processing_Time_HintNone([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
                hint: ObjectProcessingHint.None);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).Length = 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent { DeltaTime = 100 }
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_Processing_Length_HintNone([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).Length = 100,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent()
                },
                hint: ObjectProcessingHint.None);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_OneNote_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
                    ((Note)n).Length = 100;
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleEvents_NoNotes_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleEvents_OneNote_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleNotes_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleNotes_NoEvents_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 1000 },
                },
                action: n => ((Note)n).Channel = (FourBitNumber)3,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)3 },
                    new NoteOffEvent { DeltaTime = 1000, Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50, Channel = (FourBitNumber)3 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 1000, Channel = (FourBitNumber)3 },
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleNotes_WithEvents_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
                action: n => ((Note)n).Channel = (FourBitNumber)3,
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
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleNotes_Processing_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n => n.Time = ((Note)n).NoteNumber == 0 ? 100 : 10,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent { DeltaTime = 90 },
                    new NoteOffEvent(),
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleNotes_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                action: n => ((Note)n).Length = ((Note)n).NoteNumber == 0 ? 100 : 10,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithoutPredicate_MultipleNotes_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithoutPredicate(
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
                    n.Time = ((Note)n).NoteNumber == 0 ? 100 : 50;
                    ((Note)n).Length = ((Note)n).NoteNumber == 0 ? 1000 : 500;
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[0],
                action: n => { },
                match: n => ((Note)n).NoteNumber == 70,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_Matched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                action: n => { },
                match: n => true,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_NotMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                action: n => { },
                match: n => false,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_Matched_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)23,
                match: n => ((Note)n).NoteNumber == 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_Matched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_Matched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).Length = 100,
                match: n => true,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_Matched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n =>
                {
                    n.Time = 50;
                    ((Note)n).Length = 100;
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)23,
                match: n => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n => ((Note)n).Length = 100,
                match: n => false,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_OneNote_NotMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
                containerType,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent()
                },
                action: n =>
                {
                    n.Time = 50;
                    ((Note)n).Length = 100;
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                match: n => ((Note)n).NoteNumber == 70,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_NoProcessing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)23,
                match: n => ((Note)n).NoteNumber < 100,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                match: n => ((Note)n).NoteNumber < 100,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                action: n => ((Note)n).Length = 100,
                match: n => ((Note)n).NoteNumber < 100,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                    ((Note)n).Length = 100;
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                action: n => ((Note)n).Velocity = (SevenBitNumber)23,
                match: n => ((Note)n).NoteNumber > 0,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_Time_1([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                match: n => ((Note)n).NoteNumber > 0,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_Time_2([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                action: n => ((Note)n).Length = 10000,
                match: n => ((Note)n).NoteNumber == 0,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                    ((Note)n).Length = 10000;
                },
                match: n => ((Note)n).NoteNumber == 0,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                action: n => ((Note)n).Velocity = (SevenBitNumber)23,
                match: n => ((Note)n).NoteNumber > 100,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing_Time([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                match: n => ((Note)n).NoteNumber > 100,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing_Length([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                action: n => ((Note)n).Length = 10000,
                match: n => ((Note)n).NoteNumber > 100,
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
        public void ProcessObjects_Notes_EventsCollection_WithPredicate_MultipleNotes_NotMatched_Processing_TimeAndLength([Values] ContainerType containerType)
        {
            ProcessObjects_Notes_EventsCollection_WithPredicate(
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
                    ((Note)n).Length = 10000;
                },
                match: n => ((Note)n).NoteNumber > 100,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: n => { },
                expectedMidiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing_Length([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).Length = 50,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing_TimeAndLength([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                    ((Note)n).Length = 200;
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing_Channel([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).Channel = (FourBitNumber)5,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing_Velocity([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).Velocity = (SevenBitNumber)50,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_OneNote_Processing_OffVelocity([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).OffVelocity = (SevenBitNumber)50,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_NoteNumber([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)80,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Velocity([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).Velocity = (SevenBitNumber)80,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_OffVelocity([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).OffVelocity = (SevenBitNumber)80,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Channel([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).Channel = (FourBitNumber)8,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_Length([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                action: n => ((Note)n).Length = 100,
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
        public void ProcessObjects_Notes_TrackChunks_WithoutPredicate_MultipleNotes_Processing_TimeAndLength([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithoutPredicate(
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
                    ((Note)n).Length = 100;
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: n => { },
                match: n => true,
                expectedMidiEvents: new MidiEvent[0][],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)70,
                match: n => ((Note)n).NoteNumber == 0,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                match: n => ((Note)n).NoteNumber == 0,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Length([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).Length = 50,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing_TimeAndLength([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                    ((Note)n).Length = 200;
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Channel([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).Channel = (FourBitNumber)5,
                match: n => ((Note)n).NoteNumber == 0,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing_Velocity([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).Velocity = (SevenBitNumber)50,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_OneNote_Matched_Processing_OffVelocity([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).OffVelocity = (SevenBitNumber)50,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)70,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_Processing_NoteNumber([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)80,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_Processing_NoteNumber([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).NoteNumber = (SevenBitNumber)80,
                match: n => ((Note)n).NoteNumber == 90,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                match: n => ((Note)n).NoteNumber == 70,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_Processing_Length([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).Length = 100,
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
        public void ProcessObjects_Notes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_Processing_Length([Values] bool wrapToFile)
        {
            ProcessObjects_Notes_TrackChunks_WithPredicate(
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
                action: n => ((Note)n).Length = 100,
                match: n => ((Note)n).NoteNumber == 0,
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

        private void ProcessObjects_Notes_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectProcessingHint hint = ObjectProcessingHint.Default) => ProcessObjects_EventsCollection_WithPredicate(
                containerType,
                ObjectType.Note,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount,
                hint);

        private void ProcessObjects_Notes_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectProcessingHint hint = ObjectProcessingHint.Default) => ProcessObjects_EventsCollection_WithoutPredicate(
                containerType,
                ObjectType.Note,
                midiEvents,
                action,
                expectedMidiEvents,
                hint);

        private void ProcessObjects_Notes_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectProcessingHint hint = ObjectProcessingHint.Default) => ProcessObjects_TrackChunks_WithPredicate(
                ObjectType.Note,
                wrapToFile,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount,
                hint);

        private void ProcessObjects_Notes_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectProcessingHint hint = ObjectProcessingHint.Default) => ProcessObjects_TrackChunks_WithoutPredicate(
                ObjectType.Note,
                wrapToFile,
                midiEvents,
                action,
                expectedMidiEvents,
                hint);

        #endregion
    }
}
