using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Nested classes

        private class CustomTimedEvent : TimedEvent
        {
            public CustomTimedEvent(MidiEvent midiEvent, long time, int eventsCollectionIndex)
                : base(midiEvent, time)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int EventsCollectionIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent(Event, Time, EventsCollectionIndex);

            public override bool Equals(object obj) =>
                (obj as CustomTimedEvent).EventsCollectionIndex == EventsCollectionIndex;
        }

        private sealed class CustomTimedEvent2 : CustomTimedEvent
        {
            public CustomTimedEvent2(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
                : base(midiEvent, time, eventsCollectionIndex)
            {
                EventIndex = eventIndex;
            }

            public int EventIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent2(Event, Time, EventsCollectionIndex, EventIndex);

            public override bool Equals(object obj) =>
                base.Equals(obj) &&
                (obj as CustomTimedEvent2).EventIndex == EventIndex;
        }

        #endregion

        #region Constants

        private static readonly ObjectDetectionSettings CustomEventSettings = new ObjectDetectionSettings
        {
            TimedEventDetectionSettings = new TimedEventDetectionSettings
            {
                Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex)
            }
        };

        private static readonly ObjectDetectionSettings CustomEventSettings2 = new ObjectDetectionSettings
        {
            TimedEventDetectionSettings = new TimedEventDetectionSettings
            {
                Constructor = data => new CustomTimedEvent2(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
            }
        };

        #endregion

        #region Test methods

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[0],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0]);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_OneEvent_NoProcessing([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                expectedMidiEvents: new[] { new NoteOnEvent() });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing_Custom([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)((CustomTimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing_Time([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_OneEvent_Processing_Time_Custom([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((CustomTimedEvent)e).Time = 100,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_NoProcessing([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Custom([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
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
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_1([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_2([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 10 : 1000),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 990 },
                });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_HintNone([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 10 : 1000),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                hint: ObjectProcessingHint.None);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate_MultipleEvents_Processing_Time_Stable([Values] bool wrapToTrackChunks)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
                wrapToTrackChunks,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                });
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_EmptyTrackChunk([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[0],
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[0],
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => { },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_Processing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => ((NoteOnEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                match: e => ((CustomTimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } },
                expectedProcessedCount: 1,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_Matched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new NoteOnEvent { DeltaTime = 100 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_Processing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((CustomTimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_OneEvent_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new[] { new NoteOnEvent() },
                action: e => e.Time = 100,
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new NoteOnEvent() },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_NoProcessing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => ((CustomTimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 2,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing_Custom([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
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
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 900 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                },
                expectedProcessedCount: 3);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => ((NoteEvent)((TimedEvent)e).Event).NoteNumber = (SevenBitNumber)23,
                match: e => ((TimedEvent)e).Event is NoteOffEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 900 },
                },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90 },
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteOnEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOffEvent { DeltaTime = 900 },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched_Processing([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_EventsCollection_WithPredicate_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToTrackChunk)
        {
            ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
                wrapToTrackChunk,
                midiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                action: e => e.Time = 700,
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_EmptyTrackChunksCollection([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new MidiEvent[0][],
                action: e => { },
                expectedMidiEvents: new MidiEvent[0][]);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0] });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_OneTrackChunk_MultipleEvents_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 80 }
                } },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_EmptyTrackChunk([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0] },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((CustomTimedEvent)e).EventsCollectionIndex == 0,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } } },
                expectedProcessedCount: 1,
                settings: CustomEventSettings);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent { DeltaTime = 100 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new[] { new NoteOnEvent() } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                action: e => { },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)23 },
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_AllMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 900 }
                } },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 10 },
                    new NoteOffEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOnEvent { DeltaTime = 90 },
                } },
                expectedProcessedCount: 3);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is NoteOffEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 }
                } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteOnEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent { DeltaTime = 900 },
                } },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_SomeMatched_Processing_Time_Stable([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90 },
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteOnEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOnEvent { NoteNumber = (SevenBitNumber)90, DeltaTime = 0 },
                    new NoteOffEvent { DeltaTime = 900 },
                } },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_OneTrackChunk_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                action: e => e.Time = 700,
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }
                } },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0], new MidiEvent[0] });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_OneEvent_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } }, new MidiEvent[0] });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_OneEvent_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() } },
                action: e => e.Time = 100,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } } });
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
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
                    var noteEvent = (NoteEvent)((TimedEvent)e).Event;
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
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

                    var noteEvent = (NoteEvent)((TimedEvent)e).Event;
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate_MultipleTrackChunks_MultipleEvents_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
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
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_EmptyTrackChunks([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_Matched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => { },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 } }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_Matched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent { DeltaTime = 100 } }, new MidiEvent[0] },
                expectedProcessedCount: 1);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e =>
                {
                    var noteOnEvent = (NoteOnEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_OneEvent_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                action: e => e.Time = 100,
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[] { new MidiEvent[0], new[] { new NoteOnEvent() }, new MidiEvent[0] },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 4);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                match: e => ((TimedEvent)e).Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_NoProcessing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => { },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent { NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10, NoteNumber = (SevenBitNumber)23 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedProcessedCount: 4);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)((TimedEvent)e).Event;
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_AllMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 }, new NoteOnEvent { DeltaTime = 90 } },
                    new MidiEvent[] { new NoteOffEvent { DeltaTime = 10 }, new NoteOnEvent { DeltaTime = 90 } },
                },
                expectedProcessedCount: 4);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteEvent.NoteNumber = (SevenBitNumber)23;
                    noteEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is NoteOffEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000, NoteNumber = (SevenBitNumber)23 } },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing_Custom([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteEvent = (NoteEvent)((TimedEvent)e).Event;
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
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_SomeMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => e.Time = (((TimedEvent)e).Event.EventType == MidiEventType.NoteOn ? 100 : 10),
                match: e => ((TimedEvent)e).Event is NoteOnEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 900 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 910 } },
                },
                expectedProcessedCount: 2);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e =>
                {
                    var noteOnEvent = (NoteEvent)((TimedEvent)e).Event;
                    noteOnEvent.NoteNumber = (SevenBitNumber)23;
                    noteOnEvent.DeltaTime = 100;
                },
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 0);
        }

        [Test]
        public void ProcessObjects_TimedEvents_TrackChunks_WithPredicate_MultipleTrackChunks_MultipleEvents_NotMatched_Processing_Time([Values] bool wrapToFile)
        {
            ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
                wrapToFile,
                midiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                action: e => e.Time = 700,
                match: e => ((TimedEvent)e).Event is TextEvent,
                expectedMidiEvents: new[]
                {
                    new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 1000 } },
                    new MidiEvent[] { new NoteOnEvent { DeltaTime = 10 }, new NoteOffEvent { DeltaTime = 1000 } },
                },
                expectedProcessedCount: 0);
        }

        #endregion

        #region Private methods

        private void ProcessObjects_TimedEvents_EventsCollection_WithPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunk.ProcessObjects(ObjectType.TimedEvent, action, match, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    expectedProcessedCount,
                    eventsCollection.ProcessObjects(ObjectType.TimedEvent, action, match, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                Assert.IsTrue(
                    eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessObjects_TimedEvents_EventsCollection_WithoutPredicate(
            bool wrapToTrackChunk,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            if (wrapToTrackChunk)
            {
                var trackChunk = new TrackChunk(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    trackChunk.ProcessObjects(ObjectType.TimedEvent, action, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                var eventsCollection = new EventsCollection();
                eventsCollection.AddRange(midiEvents);

                Assert.AreEqual(
                    midiEvents.Count,
                    eventsCollection.ProcessObjects(ObjectType.TimedEvent, action, settings, hint),
                    "Invalid count of processed timed events.");

                var expectedEventsCollection = new EventsCollection();
                expectedEventsCollection.AddRange(expectedMidiEvents);
                MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                Assert.IsTrue(
                    eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessObjects_TimedEvents_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessObjects(ObjectType.TimedEvent, action, match, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessObjects(ObjectType.TimedEvent, action, match, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessObjects_TimedEvents_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    midiFile.ProcessObjects(ObjectType.TimedEvent, action, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    midiEvents.Sum(e => e.Count),
                    trackChunks.ProcessObjects(ObjectType.TimedEvent, action, settings, hint),
                    "Invalid count of processed timed events.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
