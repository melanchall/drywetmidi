using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

        #region Private methods

        private void CheckGetNotesAsync(
            ContainerType containerType,
            IEnumerable<IEnumerable<MidiEvent>> midiEvents,
            IEnumerable<Note> expectedNotes,
            NoteDetectionSettings settings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            Action<ICollection<Note>> additionalChecks = null)
        {
            TrackChunk[] CreateTracks() =>
                midiEvents.Select(events => new TrackChunk(events.Select(e => e.Clone()))).ToArray();

            ICollection<Note> notes;
            var tracks = CreateTracks();

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    notes = tracks[0].Events.GetNotesAsync(settings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunk:
                    notes = tracks[0].GetNotesAsync(settings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunks:
                    notes = tracks.GetNotesAsync(settings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                default:
                    notes = new MidiFile(tracks).GetNotesAsync(settings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
            }

            MidiAsserts.AreEqual(expectedNotes, notes, "Async notes are invalid.");
            additionalChecks?.Invoke(notes);

            var objectSettings = new ObjectDetectionSettings
            {
                NoteDetectionSettings = settings,
                TimedEventDetectionSettings = timedEventDetectionSettings
            };
            tracks = CreateTracks();
            ICollection<ITimedObject> objects;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    objects = tracks[0].Events.GetObjectsAsync(ObjectType.Note, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunk:
                    objects = tracks[0].GetObjectsAsync(ObjectType.Note, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunks:
                    objects = tracks.GetObjectsAsync(ObjectType.Note, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                default:
                    objects = new MidiFile(tracks).GetObjectsAsync(ObjectType.Note, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
            }

            MidiAsserts.AreEqual(expectedNotes, objects, "Async notes from GetObjects are invalid.");
            additionalChecks?.Invoke(objects.Cast<Note>().ToArray());

            notes = InteractionAsyncTests.AsAsync(CreateTracks())
                .GetNotesAsync(settings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expectedNotes, notes, "Notes from an async track source are invalid.");
            additionalChecks?.Invoke(notes);

            if (containerType == ContainerType.EventsCollection || containerType == ContainerType.TrackChunk)
            {
                notes = InteractionAsyncTests.AsAsync(CreateTracks()[0].Events)
                    .GetNotesAsync(settings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;

                MidiAsserts.AreEqual(expectedNotes, notes, "Notes from an async event source are invalid.");
                additionalChecks?.Invoke(notes);
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

        #endregion
    }
}
