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

        #region Private methods

        private void CheckGetChordsAsync(
            ContainerType containerType,
            IEnumerable<IEnumerable<MidiEvent>> midiEvents,
            IEnumerable<Chord> expectedChords,
            ChordDetectionSettings settings = null,
            NoteDetectionSettings noteDetectionSettings = null,
            TimedEventDetectionSettings timedEventDetectionSettings = null,
            Action<ICollection<Chord>> additionalChecks = null)
        {
            TrackChunk[] CreateTracks() =>
                midiEvents.Select(events => new TrackChunk(events.Select(e => e.Clone()))).ToArray();

            var tracks = CreateTracks();
            ICollection<Chord> chords;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    chords = tracks[0].Events.GetChordsAsync(settings, noteDetectionSettings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunk:
                    chords = tracks[0].GetChordsAsync(settings, noteDetectionSettings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunks:
                    chords = tracks.GetChordsAsync(settings, noteDetectionSettings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                default:
                    chords = new MidiFile(tracks).GetChordsAsync(settings, noteDetectionSettings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;
                    break;
            }

            MidiAsserts.AreEqual(expectedChords, chords, "Async chords are invalid.");
            additionalChecks?.Invoke(chords);

            var objectSettings = new ObjectDetectionSettings
            {
                ChordDetectionSettings = settings,
                NoteDetectionSettings = noteDetectionSettings,
                TimedEventDetectionSettings = timedEventDetectionSettings
            };
            tracks = CreateTracks();
            ICollection<ITimedObject> objects;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    objects = tracks[0].Events.GetObjectsAsync(ObjectType.Chord, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunk:
                    objects = tracks[0].GetObjectsAsync(ObjectType.Chord, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                case ContainerType.TrackChunks:
                    objects = tracks.GetObjectsAsync(ObjectType.Chord, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
                default:
                    objects = new MidiFile(tracks).GetObjectsAsync(ObjectType.Chord, objectSettings, cancellationToken: CancellationToken.None).Result;
                    break;
            }

            MidiAsserts.AreEqual(expectedChords, objects, "Async chords from GetObjects are invalid.");
            additionalChecks?.Invoke(objects.Cast<Chord>().ToArray());

            chords = InteractionAsyncTests.AsAsync(CreateTracks())
                .GetChordsAsync(settings, noteDetectionSettings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;

            MidiAsserts.AreEqual(expectedChords, chords, "Chords from an async track source are invalid.");
            additionalChecks?.Invoke(chords);

            if (containerType == ContainerType.EventsCollection || containerType == ContainerType.TrackChunk)
            {
                chords = InteractionAsyncTests.AsAsync(CreateTracks()[0].Events)
                    .GetChordsAsync(settings, noteDetectionSettings, timedEventDetectionSettings, cancellationToken: CancellationToken.None).Result;

                MidiAsserts.AreEqual(expectedChords, chords, "Chords from an async event source are invalid.");
                additionalChecks?.Invoke(chords);
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
