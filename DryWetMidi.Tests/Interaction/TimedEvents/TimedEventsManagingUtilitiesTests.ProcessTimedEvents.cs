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
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[0],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_OneEvent_NoProcessing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_OneEvent_Processing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_OneEvent_Processing_Time([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_MultipleEvents_NoProcessing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_MultipleEvents_Processing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_MultipleEvents_Processing_Time([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate_MultipleEvents_Processing_Time_Stable([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_Matched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_Matched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_Matched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_OneEvent_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 900 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 3);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 900 },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90 },
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOffEvent { DeltaTime = 900 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 700,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_OneEvent_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_OneEvent_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_OneEvent_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_MultipleEvents_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_MultipleEvents_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_MultipleEvents_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate_MultipleEvents_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_Matched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_Matched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_Matched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_OneEvent_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_AllMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 },
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 3,
                expectedMatchedTotalIndices: new int[] { 0, 1, 2 },
                expectedMatchedIndices: new int[] { 0, 1, 2 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }
                },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90 },
                },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90 }
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 1, 2 },
                expectedMatchedIndices: new int[] { 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_EmptyTrackChunksCollection([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 900 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedProcessedCount: 3);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 900 },
                } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90 },
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOffEvent { DeltaTime = 900 },
                } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = 700,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0] },
                expectedMatchedChunkIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedMatchedChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedMatchedChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[][] { new MidiEvent[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new MidiEvent[][] { new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 } } },
                expectedMatchedChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedMatchedChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedMatchedChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedMatchedChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedMatchedChunkIndices: new[] { 0, 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0] },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 0, 0 },
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 0, 0 },
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 0, 1 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 0, 0 },
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 },
                } },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedProcessedCount: 3,
                expectedMatchedTotalIndices: new int[] { 0, 1, 2 },
                expectedMatchedIndices: new int[] { 0, 1, 2 },
                expectedMatchedChunkIndices: new[] { 0, 0, 0 },
                expectedTotalChunkIndices: new[] { 0, 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }
                } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 1 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90 },
                } },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90 }
                } },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 1, 2 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 0, 0 },
                expectedTotalChunkIndices: new[] { 0, 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0], new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } }, new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 1000 } },
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 10 }
                    }
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)e.Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                        new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 10, NoteNumber = (SevenBitNumber)23 }
                    }
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 10 }
                    }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 10 },
                        new NoteOnEvent { DeltaTime = 90 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 100 }
                    }
                });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 4);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)e.Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10, NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedProcessedCount: 4);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 }, new NoteOnEvent { DeltaTime = 90 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 }, new NoteOnEvent { DeltaTime = 90 } },
                },
                expectedProcessedCount: 4);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)e.Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 910 } },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => e.Time = 700,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedMatchedChunkIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedMatchedChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedMatchedChunkIndices: new[] { 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[][] { new MidiEvent[0], new MidiEvent[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                expectedMidiEvents: new MidiEvent[][] { new MidiEvent[0], new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 } }, new MidiEvent[0] },
                expectedMatchedChunkIndices: new[] { 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900 } },
                },
                action: e => { },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900 } },
                },
                expectedMatchedChunkIndices: new[] { 0, 1, 1, 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent(), new TextEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900 } },
                },
                action: e =>
                {
                    var noteEvent = e.Event as NoteEvent;
                    if (noteEvent != null)
                    {
                        noteEvent.NoteNumber = (SevenBitNumber)23;
                        noteEvent.DeltaTime = 100;
                    }
                },
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                    new MidiEvent[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 }, new TextEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedMatchedChunkIndices: new[] { 0, 1, 1, 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new TextEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900 } },
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 }, new NoteOnEvent { DeltaTime = 90 } },
                    new MidiEvent[] { new TextEvent { DeltaTime = 10 }, new NoteOffEvent() },
                },
                expectedMatchedChunkIndices: new[] { 0, 1, 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 },
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 10 },
                        new NoteOffEvent { DeltaTime = 990 }
                    }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 10 },
                        new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                        new NoteOnEvent { DeltaTime = 90 },
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 10 },
                        new NoteOnEvent { DeltaTime = 90 }
                    }
                },
                expectedMatchedChunkIndices: new[] { 0, 1, 0, 1, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new int[0]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 0 },
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 1 },
                expectedTotalChunkIndices: new[] { 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } } },
                expectedProcessedCount: 1,
                expectedMatchedTotalIndices: new int[] { 0 },
                expectedMatchedIndices: new int[] { 0 },
                expectedMatchedChunkIndices: new[] { 1 },
                expectedTotalChunkIndices: new[] { 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 90 } },
                },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 90 } },
                },
                expectedProcessedCount: 4,
                expectedMatchedTotalIndices: new int[] { 0, 1, 2, 3 },
                expectedMatchedIndices: new int[] { 0, 1, 2, 3 },
                expectedMatchedChunkIndices: new[] { 0, 1, 1, 0 },
                expectedTotalChunkIndices: new[] { 0, 1, 1, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 90 } },
                },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 90 } },
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 2, 3 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 1, 0 },
                expectedTotalChunkIndices: new[] { 0, 1, 1, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0, 1, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[0],
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 }
                    },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)e.Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                        new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                    },
                    new MidiEvent[0],
                    new MidiEvent[]
                    {
                        new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                        new NoteOffEvent { DeltaTime = 100, NoteNumber = (SevenBitNumber)23 }
                    },
                },
                expectedProcessedCount: 4,
                expectedMatchedTotalIndices: new int[] { 0, 1, 2, 3 },
                expectedMatchedIndices: new int[] { 0, 1, 2, 3 },
                expectedMatchedChunkIndices: new[] { 0, 2, 2, 0 },
                expectedTotalChunkIndices: new[] { 0, 2, 2, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[0],
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 }
                    },
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 10 },
                        new NoteOnEvent { DeltaTime = 90 },
                    },
                    new MidiEvent[0],
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 10 },
                        new NoteOnEvent { DeltaTime = 90 },
                    },
                },
                expectedProcessedCount: 4,
                expectedMatchedTotalIndices: new int[] { 0, 1, 2, 3 },
                expectedMatchedIndices: new int[] { 0, 1, 2, 3 },
                expectedMatchedChunkIndices: new[] { 0, 2, 2, 0 },
                expectedTotalChunkIndices: new[] { 0, 2, 2, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 100 }
                    },
                    new MidiEvent[]
                    {
                        new TextEvent(),
                    },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)e.Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 100, NoteNumber = (SevenBitNumber)23 }
                    },
                    new MidiEvent[]
                    {
                        new TextEvent(),
                    },
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 2, 3 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 1, 0 },
                expectedTotalChunkIndices: new[] { 0, 2, 1, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 1000 }
                    },
                    new MidiEvent[]
                    {
                        new TextEvent(),
                    },
                },
                action: e => e.Time = 100,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[]
                    {
                        new NoteOnEvent(),
                        new NoteOffEvent { DeltaTime = 100 }
                    },
                    new MidiEvent[]
                    {
                        new NoteOffEvent { DeltaTime = 100 }
                    },
                    new MidiEvent[]
                    {
                        new TextEvent(),
                    },
                },
                expectedProcessedCount: 2,
                expectedMatchedTotalIndices: new int[] { 2, 3 },
                expectedMatchedIndices: new int[] { 0, 1 },
                expectedMatchedChunkIndices: new[] { 0, 1 },
                expectedTotalChunkIndices: new[] { 0, 2, 0, 1 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0, 1, 0 });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 } },
                },
                expectedProcessedCount: 0,
                expectedMatchedTotalIndices: new int[0],
                expectedMatchedIndices: new int[0],
                expectedMatchedChunkIndices: new int[0],
                expectedTotalChunkIndices: new[] { 0, 1, 0 });
        }

        #endregion

        #region Private methods

        private void ProcessTimedEvents_EventsCollection_WithIndices_WithPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ICollection<int> expectedMatchedTotalIndices,
            ICollection<int> expectedMatchedIndices)
        {
            var matchedTotalIndices = new List<int>();
            var matchedIndices = new List<int>();
            var totalIndices = new List<int>();

            ProcessTimedEventAction processTimedEventAction = (timedEvent, iEventsCollection, iTotal, iMatched) =>
            {
                Assert.AreEqual(0, iEventsCollection, "Events collection index is invalid.");
                matchedTotalIndices.Add(iTotal);
                matchedIndices.Add(iMatched);
                action(timedEvent);
            };

            ProcessTimedEventPredicate processTimedEventPredicate = (timedEvent, iEventsCollection, iTotal) =>
            {
                Assert.AreEqual(0, iEventsCollection, "Events collection index is invalid.");
                totalIndices.Add(iTotal);
                return match(timedEvent);
            };

            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunk.ProcessTimedEvents(processTimedEventAction, processTimedEventPredicate),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    expectedProcessedCount,
                    eventsCollection.ProcessTimedEvents(processTimedEventAction, processTimedEventPredicate),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), totalIndices, "Invalid total indices.");

            CollectionAssert.AreEqual(expectedMatchedTotalIndices, matchedTotalIndices, "Invalid matched total indices.");
            CollectionAssert.AreEqual(expectedMatchedIndices, matchedIndices, "Invalid matched indices.");
        }

        private void ProcessTimedEvents_EventsCollection_WithIndices_WithoutPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var totalIndices = new List<int>();
            var matchedIndices = new List<int>();

            ProcessTimedEventAction processTimedEventAction = (timedEvent, iEventsCollection, iTotal, iMatched) =>
            {
                Assert.AreEqual(0, iEventsCollection, "Events collection index is invalid.");
                totalIndices.Add(iTotal);
                matchedIndices.Add(iMatched);
                action(timedEvent);
            };

            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    trackChunk.ProcessTimedEvents(processTimedEventAction),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    eventsCollection.ProcessTimedEvents(processTimedEventAction),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), totalIndices, "Invalid total indices.");
            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Count), matchedIndices, "Invalid matched indices.");
        }

        private void ProcessTimedEvents_EventsCollection_WithoutIndices_WithPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunk.ProcessTimedEvents(action, match),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    expectedProcessedCount,
                    eventsCollection.ProcessTimedEvents(action, match),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }
        }

        private void ProcessTimedEvents_EventsCollection_WithoutIndices_WithoutPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    trackChunk.ProcessTimedEvents(action),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    eventsCollection.ProcessTimedEvents(action),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
            }
        }

        private void ProcessTimedEvents_TrackChunks_WithIndices_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ICollection<int> expectedMatchedTotalIndices,
            ICollection<int> expectedMatchedIndices,
            ICollection<int> expectedMatchedChunkIndices,
            ICollection<int> expectedTotalChunkIndices)
        {
            var matchedTotalIndices = new List<int>();
            var matchedIndices = new List<int>();
            var matchedChunkIndices = new List<int>();

            var totalIndices = new List<int>();
            var totalChunkIndices = new List<int>();

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            ProcessTimedEventAction processTimedEventAction = (timedEvent, iChunk, iTotal, iMatched) =>
            {
                matchedTotalIndices.Add(iTotal);
                matchedIndices.Add(iMatched);
                matchedChunkIndices.Add(iChunk);
                action(timedEvent);
            };

            ProcessTimedEventPredicate processTimedEventPredicate = (timedEvent, iChunk, iTotal) =>
            {
                totalIndices.Add(iTotal);
                totalChunkIndices.Add(iChunk);
                return match(timedEvent);
            };

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessTimedEvents(processTimedEventAction, processTimedEventPredicate),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessTimedEvents(processTimedEventAction, processTimedEventPredicate),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Sum(e => e.Count)), totalIndices, "Invalid total indices.");
            CollectionAssert.AreEqual(expectedTotalChunkIndices, totalChunkIndices, "Invalid total chunk indices.");
            
            CollectionAssert.AreEqual(expectedMatchedTotalIndices, matchedTotalIndices, "Invalid matched total indices.");
            CollectionAssert.AreEqual(expectedMatchedIndices, matchedIndices, "Invalid matched indices.");
            CollectionAssert.AreEqual(expectedMatchedChunkIndices, matchedChunkIndices, "Invalid matched chunk indices.");
        }

        private void ProcessTimedEvents_TrackChunks_WithIndices_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<TimedEvent> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ICollection<int> expectedMatchedChunkIndices)
        {
            var totalIndices = new List<int>();
            var matchedIndices = new List<int>();
            var matchedChunkIndices = new List<int>();

            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            ProcessTimedEventAction processTimedEventAction = (timedEvent, iChunk, iTotal, iMatched) =>
            {
                totalIndices.Add(iTotal);
                matchedIndices.Add(iMatched);
                matchedChunkIndices.Add(iChunk);
                action(timedEvent);
            };

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    midiFile.ProcessTimedEvents(processTimedEventAction),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    trackChunks.ProcessTimedEvents(processTimedEventAction),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
            }

            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Sum(e => e.Count)), totalIndices, "Invalid total indices.");
            CollectionAssert.AreEqual(Enumerable.Range(0, midiEvents.Sum(e => e.Count)), matchedIndices, "Invalid matched indices.");
            CollectionAssert.AreEqual(expectedMatchedChunkIndices, matchedChunkIndices, "Invalid matched chunk indices.");
        }

        private void ProcessTimedEvents_TrackChunks_WithoutIndices_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessTimedEvents(action, match),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessTimedEvents(action, match),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
            }
        }

        private void ProcessTimedEvents_TrackChunks_WithoutIndices_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<TimedEvent> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    midiFile.ProcessTimedEvents(action),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreFilesEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
            }
            else
            {
                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    trackChunks.ProcessTimedEvents(action),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
            }
        }

        #endregion
    }
}
