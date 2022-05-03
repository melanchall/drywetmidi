using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class NotesManagingUtilitiesTests
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

        #endregion

        #region Constants

        private static readonly NoteMethods NoteMethods = new NoteMethods();

        private static readonly TimedEventDetectionSettings CustomEventSettings = new TimedEventDetectionSettings
        {
            Constructor = data => new CustomTimedEvent(data.Event, data.Time, data.EventsCollectionIndex, data.EventIndex)
        };

        private static readonly Func<NoteData, Note> CustomNoteConstructor =
            data => new CustomNote(
                data.TimedNoteOnEvent,
                data.TimedNoteOffEvent,
                (data.TimedNoteOnEvent as CustomTimedEvent)?.EventsCollectionIndex);

        #endregion
    }
}
