using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Nested classes

        private sealed class CustomTimedEvent : TimedEvent
        {
            public CustomTimedEvent(MidiEvent midiEvent, long time, int eventsCollectionIndex, int eventIndex)
                : base(midiEvent, time)
            {
                EventsCollectionIndex = eventsCollectionIndex;
                EventIndex = eventIndex;
            }

            public int EventsCollectionIndex { get; }

            public int EventIndex { get; }

            public override ITimedObject Clone() =>
                new CustomTimedEvent(Event, Time, EventsCollectionIndex, EventIndex);

            public override bool Equals(object obj) =>
                base.Equals(obj) &&
                (obj as CustomTimedEvent).EventIndex == EventIndex;
        }

        private sealed class CustomNote : Note
        {
            public CustomNote(TimedEvent noteOnTimedEvent, TimedEvent noteOffTimedEvent, int? eventsCollectionIndex)
                : base(noteOnTimedEvent, noteOffTimedEvent, false)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int? EventsCollectionIndex { get; }

            public override ITimedObject Clone() =>
                new CustomNote(TimedNoteOnEvent, TimedNoteOffEvent, EventsCollectionIndex);

            public override bool Equals(object obj)
            {
                if (!(obj is CustomNote customNote))
                    return false;

                return customNote.EventsCollectionIndex == EventsCollectionIndex;
            }
        }

        private sealed class CustomChord : Chord
        {
            public CustomChord(ICollection<Note> notes, int? eventsCollectionIndex)
                : base(notes)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public int? EventsCollectionIndex { get; }

            public override ITimedObject Clone() =>
                new CustomChord(Notes.Select(n => (Note)n.Clone()).ToArray(), EventsCollectionIndex);

            public override bool Equals(object obj)
            {
                if (!(obj is CustomChord customChord))
                    return false;

                return customChord.EventsCollectionIndex == EventsCollectionIndex;
            }
        }

        #endregion
    }
}
