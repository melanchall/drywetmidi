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
    public sealed partial class ChordsManagingUtilitiesTests
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

            public override bool Equals(object obj)
            {
                if (!(obj is CustomTimedEvent customTimedEvent))
                    return false;

                return customTimedEvent.EventsCollectionIndex == EventsCollectionIndex &&
                    customTimedEvent.EventIndex == EventIndex;
            }
        }

        private sealed class CustomNote : Note
        {
            public CustomNote(TimedEvent noteOnTimedEvent, TimedEvent noteOffTimedEvent, int? eventsCollectionIndex)
                : base(noteOnTimedEvent, noteOffTimedEvent, false)
            {
                EventsCollectionIndex = eventsCollectionIndex;
            }

            public CustomNote(SevenBitNumber noteNumber, int? eventsCollectionIndex)
                : base(noteNumber)
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

        #region Constants

        private static readonly Func<TimedEventData, CustomTimedEvent> CustomTimedEventConstructor =
            data => new CustomTimedEvent(
                data.Event,
                data.Time,
                data.EventsCollectionIndex,
                data.EventIndex);

        private static readonly TimedEventDetectionSettings CustomEventSettings = new TimedEventDetectionSettings
        {
            Constructor = CustomTimedEventConstructor
        };

        private static readonly Func<NoteData, Note> CustomNoteConstructor =
            data => new CustomNote(
                data.TimedNoteOnEvent,
                data.TimedNoteOffEvent,
                (data.TimedNoteOnEvent as CustomTimedEvent)?.EventsCollectionIndex);

        private static readonly Func<ChordData, Chord> CustomChordConstructor =
            data => new CustomChord(
                data.Notes,
                (data.Notes.FirstOrDefault() as CustomNote)?.EventsCollectionIndex);

        #endregion
    }
}
