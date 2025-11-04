using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedEventsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[0],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_OneEvent_NoProcessing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing_Custom([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)((CustomTimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing_Time([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing_Time_Custom([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((CustomTimedEvent)e).Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_NoProcessing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Custom([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)((CustomTimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_1([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_2([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 10 : 1000),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 990 },
                });
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_HintNone([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (e.Event.EventType == MidiEventType.NoteOn ? 10 : 1000),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                hint: TimedEventProcessingHint.None);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_Stable([Values] bool wrapToTrackChunks)
        {
            ProcessTimedEvents_EventsCollection_WithoutPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_Processing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                match: e => ((CustomTimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_Processing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((CustomTimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_NoProcessing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => ((CustomTimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                match: e => ((CustomTimedEvent)e).EventsCollectionIndex == 0,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)e.Event).NoteNumber = (SevenBitNumber)23,
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_EmptyTrackChunksCollection([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((CustomTimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)e.Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((CustomTimedEvent)e).EventsCollectionIndex == 0,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedProcessedCount: 1,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
                match: e => ((CustomTimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 2,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0], new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } } });
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
                    var eventsCollectionIndex = ((CustomTimedEvent)e).EventsCollectionIndex;

                    var noteEvent = (NoteEvent)e.Event;
                    noteEvent.NoteNumber = eventsCollectionIndex == 0 ? (SevenBitNumber)23 : (SevenBitNumber)33;
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
                        new NoteOnEvent { DeltaTime = 10, NoteNumber = (SevenBitNumber)33 }
                    }
                },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => e.Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
                match: e => ((CustomTimedEvent)e).EventsCollectionIndex >= 0,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10, NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedProcessedCount: 4,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
                match: e => ((CustomTimedEvent)e).EventsCollectionIndex == 1,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10, NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedProcessedCount: 2,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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
        public void ProcessTimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessTimedEvents_TrackChunks_WithPredicate(
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

        // TODO: describe in docs
        [Test]
        public void ProcessTimedEvents_Custom_Null_1()
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk: false,
                midiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                },
                expectedProcessedCount: 0,
                settings: new TimedEventDetectionSettings
                {
                    Constructor = timedEventData => null,
                });
        }

        // TODO: describe in docs
        [Test]
        public void ProcessTimedEvents_Custom_Null_2()
        {
            ProcessTimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk: false,
                midiEvents: new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 10 },
                },
                action: e => e.Time = 100,
                match: e => e.Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new TextEvent("B") { DeltaTime = 100 },
                },
                expectedProcessedCount: 1,
                settings: new TimedEventDetectionSettings
                {
                    Constructor = timedEventData => (timedEventData.Event as TextEvent).Text == "A"
                        ? null
                        : new TimedEvent(timedEventData.Event, timedEventData.Time),
                });
        }

        #endregion

        #region Private methods

        private void ProcessTimedEvents_EventsCollection_WithPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            TimedEventDetectionSettings settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                ClassicAssert.AreEqual(
                    expectedProcessedCount,
                    trackChunk.ProcessTimedEvents(action, match, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                ClassicAssert.AreEqual(
                    expectedProcessedCount,
                    eventsCollection.ProcessTimedEvents(action, match, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessTimedEvents_EventsCollection_WithoutPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<TimedEvent> action,
            ICollection<MidiEvent> expectedMidiEvents,
            TimedEventDetectionSettings settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                ClassicAssert.AreEqual(
                    midiEvents.Count,
                    trackChunk.ProcessTimedEvents(action, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                ClassicAssert.AreEqual(
                    midiEvents.Count,
                    eventsCollection.ProcessTimedEvents(action, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessTimedEvents_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<TimedEvent> action,
            Predicate<TimedEvent> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            TimedEventDetectionSettings settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                ClassicAssert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessTimedEvents(action, match, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                ClassicAssert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                ClassicAssert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessTimedEvents(action, match, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessTimedEvents_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<TimedEvent> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            TimedEventDetectionSettings settings = null,
            TimedEventProcessingHint hint = TimedEventProcessingHint.Default)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                ClassicAssert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    midiFile.ProcessTimedEvents(action, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                ClassicAssert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                ClassicAssert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    trackChunks.ProcessTimedEvents(action, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                ClassicAssert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
