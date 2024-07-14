using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides utilities to extract objects of different types at once. More info in the
    /// <see href="xref:a_getting_objects#getobjects">Getting objects: GetObjects</see> article.
    /// </summary>
    public static class GetObjectsUtilities
    {
        #region Methods

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="midiEvents">Collection of <see cref="MidiEvent"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="midiEvents"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this IEnumerable<MidiEvent> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return midiEvents
                .GetTimedEventsLazy(settings?.TimedEventDetectionSettings)
                .GetObjectsFromSortedTimedObjects(0, objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="MidiEvent"/>
        /// returning them as a lazy collection.
        /// </summary>
        /// <param name="midiEvents">Collection of <see cref="MidiEvent"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>A lazy collection of objects built on top of <paramref name="midiEvents"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public static IEnumerable<ITimedObject> EnumerateObjects(
            this IEnumerable<MidiEvent> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return midiEvents
                .GetTimedEventsLazy(settings?.TimedEventDetectionSettings)
                .EnumerateObjectsFromSortedTimedObjects(objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a <see cref="EventsCollection"/>.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="eventsCollection"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection
                .GetTimedEventsLazy(settings?.TimedEventDetectionSettings)
                .GetObjectsFromSortedTimedObjects(eventsCollection.Count / 2, objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="trackChunk"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.Events.GetObjects(objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="trackChunks"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            var eventsCollections = trackChunks.Where(c => c != null).Select(c => c.Events).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var timedEvents = eventsCollections.GetTimedEventsLazy(
                eventsCount,
                settings?.TimedEventDetectionSettings);
            var timedObjects = (IEnumerable<ITimedObject>)timedEvents.Select(o => o.Object);

            if (objectType.HasFlag(ObjectType.Chord) || objectType.HasFlag(ObjectType.Note))
            {
                timedObjects = !objectType.HasFlag(ObjectType.Chord)
                    ? timedEvents.GetNotesAndTimedEventsLazy(settings?.NoteDetectionSettings ?? new NoteDetectionSettings()).Select(o => o.Object)
                    : timedEvents.GetChordsAndNotesAndTimedEventsLazy(settings?.ChordDetectionSettings ?? new ChordDetectionSettings(), settings?.NoteDetectionSettings ?? new NoteDetectionSettings(), settings?.TimedEventDetectionSettings ?? new TimedEventDetectionSettings()).Select(o => o.Object);
            }

            return timedObjects.GetObjectsFromSortedTimedObjects(eventsCount / 2, objectType, settings, false);
        }

        /// <summary>
        /// Extracts objects of the specified types from a <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="midiFile"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            return midiFile.GetTrackChunks().GetObjects(objectType, settings);
        }

        /// <summary>
        /// Extracts objects of the specified types from a collection of <see cref="ITimedObject"/>.
        /// </summary>
        /// <param name="timedObjects">Collection of <see cref="ITimedObject"/> to extract objects from.</param>
        /// <param name="objectType">Combination of desired objects types.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Collection of objects of the specified types extracted from <paramref name="timedObjects"/>.
        /// Objects are ordered by time.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timedObjects"/> is <c>null</c>.</exception>
        public static ICollection<ITimedObject> GetObjects(
            this IEnumerable<ITimedObject> timedObjects,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            var getChords = objectType.HasFlag(ObjectType.Chord);
            var getNotes = objectType.HasFlag(ObjectType.Note);

            var resultCollectionSize = 0;
            var processedTimedObjects = new List<ITimedObject>();

            foreach (var timedObject in timedObjects)
            {
                var processed =
                    TryProcessTimedEvent(timedObject as TimedEvent, processedTimedObjects) ||
                    TryProcessNote(timedObject as Note, processedTimedObjects, getNotes, getChords) ||
                    TryProcessChord(timedObject as Chord, processedTimedObjects, getNotes, getChords);

                if (processed)
                    resultCollectionSize++;
            }

            return GetObjectsFromSortedTimedObjects(
                processedTimedObjects.OrderBy(o => o.Time),
                resultCollectionSize,
                objectType,
                settings);
        }

        private static bool TryProcessTimedEvent(TimedEvent timedEvent, List<ITimedObject> processedTimedObjects)
        {
            if (timedEvent == null)
                return false;

            processedTimedObjects.Add(timedEvent);
            return true;
        }

        private static bool TryProcessNote(Note note, List<ITimedObject> processedTimedObjects, bool getNotes, bool getChords)
        {
            if (note == null)
                return false;

            if (getNotes || getChords)
                processedTimedObjects.Add(note);
            else
            {
                processedTimedObjects.Add(note.TimedNoteOnEvent);
                processedTimedObjects.Add(note.TimedNoteOffEvent);
            }

            return true;
        }

        private static bool TryProcessChord(Chord chord, List<ITimedObject> processedTimedObjects, bool getNotes, bool getChords)
        {
            if (chord == null)
                return false;

            if (getChords)
                processedTimedObjects.Add(chord);
            else if (getNotes)
                processedTimedObjects.AddRange(chord.Notes);
            else
            {
                foreach (var note in chord.Notes)
                {
                    processedTimedObjects.Add(note.TimedNoteOnEvent);
                    processedTimedObjects.Add(note.TimedNoteOffEvent);
                }
            }

            return true;
        }

        private static ICollection<ITimedObject> GetObjectsFromSortedTimedObjects(
            this IEnumerable<ITimedObject> processedTimedObjects,
            int resultCollectionSize,
            ObjectType objectType,
            ObjectDetectionSettings settings,
            bool createNotes = true)
        {
            var getChords = objectType.HasFlag(ObjectType.Chord);
            var getNotes = objectType.HasFlag(ObjectType.Note);
            var getTimedEvents = objectType.HasFlag(ObjectType.TimedEvent);

            settings = settings ?? new ObjectDetectionSettings();
            var noteDetectionSettings = settings.NoteDetectionSettings ?? new NoteDetectionSettings();
            var chordDetectionSettings = settings.ChordDetectionSettings ?? new ChordDetectionSettings();

            var timedObjects = processedTimedObjects;

            if (createNotes && (getChords || getNotes))
            {
                var notesAndTimedEvents = processedTimedObjects.GetNotesAndTimedEventsLazy(noteDetectionSettings, true);

                timedObjects = getChords
                    ? notesAndTimedEvents.GetChordsAndNotesAndTimedEventsLazy(chordDetectionSettings, true)
                    : notesAndTimedEvents;
            }

            //

            var result = resultCollectionSize > 0
                ? new List<ITimedObject>(resultCollectionSize)
                : new List<ITimedObject>();

            foreach (var timedObject in timedObjects)
            {
                var processed = false;

                if (getChords)
                {
                    var chord = timedObject as Chord;
                    if (processed = (chord != null))
                        result.Add(chord);
                }

                if (!processed && getNotes)
                {
                    var note = timedObject as Note;
                    if (processed = (note != null))
                        result.Add(note);
                }

                if (!processed && getTimedEvents)
                {
                    var timedEvent = timedObject as TimedEvent;
                    if (timedEvent != null)
                        result.Add(timedEvent);
                    else
                    {
                        var note = timedObject as Note;
                        if (note != null)
                        {
                            result.Add(note.GetTimedNoteOnEvent());
                            result.Add(note.GetTimedNoteOffEvent());
                        }
                    }
                }
            }

            result.TrimExcess();
            return new SortedTimedObjectsImmutableCollection<ITimedObject>(result);
        }

        private static IEnumerable<ITimedObject> EnumerateObjectsFromSortedTimedObjects(
            this IEnumerable<ITimedObject> processedTimedObjects,
            ObjectType objectType,
            ObjectDetectionSettings settings,
            bool createNotes = true)
        {
            var getChords = objectType.HasFlag(ObjectType.Chord);
            var getNotes = objectType.HasFlag(ObjectType.Note);
            var getTimedEvents = objectType.HasFlag(ObjectType.TimedEvent);

            settings = settings ?? new ObjectDetectionSettings();
            var noteDetectionSettings = settings.NoteDetectionSettings ?? new NoteDetectionSettings();
            var chordDetectionSettings = settings.ChordDetectionSettings ?? new ChordDetectionSettings();

            var timedObjects = processedTimedObjects;

            if (createNotes && (getChords || getNotes))
            {
                var notesAndTimedEvents = processedTimedObjects.GetNotesAndTimedEventsLazy(noteDetectionSettings, true);

                timedObjects = getChords
                    ? notesAndTimedEvents.GetChordsAndNotesAndTimedEventsLazy(chordDetectionSettings, true)
                    : notesAndTimedEvents;
            }

            //

            foreach (var timedObject in timedObjects)
            {
                var processed = false;

                if (getChords)
                {
                    var chord = timedObject as Chord;
                    if (processed = (chord != null))
                        yield return chord;
                }

                if (!processed && getNotes)
                {
                    var note = timedObject as Note;
                    if (processed = (note != null))
                        yield return note;
                }

                if (!processed && getTimedEvents)
                {
                    var timedEvent = timedObject as TimedEvent;
                    if (timedEvent != null)
                        yield return timedEvent;
                    else
                    {
                        var note = timedObject as Note;
                        if (note != null)
                        {
                            yield return note.GetTimedNoteOnEvent();
                            yield return note.GetTimedNoteOffEvent();
                        }
                    }
                }
            }
        }

        #endregion
    }
}
