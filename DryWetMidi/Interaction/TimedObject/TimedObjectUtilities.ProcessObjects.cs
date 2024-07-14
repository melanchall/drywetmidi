using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TimedObjectUtilities
    {
        #region Nested classes

        private sealed class ProcessingContext
        {
            public bool TimeOrLengthChanged { get; set; }

            public bool NotesCollectionChanged { get; set; }

            public bool NoteTimeOrLengthChanged { get; set; }

            public bool TimeOrLengthCanBeChanged { get; set; }

            public bool NotesCollectionCanBeChanged { get; set; }

            public bool NoteTimeOrLengthCanBeChanged { get; set; }

            public bool HasTimingChanges =>
                (TimeOrLengthCanBeChanged && TimeOrLengthChanged) ||
                (NotesCollectionCanBeChanged && NotesCollectionChanged) ||
                (NoteTimeOrLengthCanBeChanged && NoteTimeOrLengthChanged);
        }

        #endregion

        #region Methods

        public static int ProcessObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return eventsCollection.ProcessObjects(objectType, action, note => true, settings, hint);
        }

        public static int ProcessObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return new[] { eventsCollection }.ProcessObjectsInternal(objectType, action, match, settings, hint);
        }

        public static int ProcessObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.ProcessObjects(objectType, action, note => true, settings, hint);
        }

        public static int ProcessObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunk.Events.ProcessObjects(objectType, action, match, settings, hint);
        }

        public static int ProcessObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return trackChunks.ProcessObjects(objectType, action, note => true, settings, hint);
        }

        public static int ProcessObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunks
                .Where(c => c != null)
                .Select(c => c.Events)
                .ProcessObjectsInternal(objectType, action, match, settings, hint);
        }

        public static int ProcessObjects(
            this MidiFile file,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return file.ProcessObjects(objectType, action, note => true, settings, hint);
        }

        public static int ProcessObjects(
            this MidiFile file,
            ObjectType objectType,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);
            ThrowIfArgument.IsNull(nameof(match), match);

            return file.GetTrackChunks().ProcessObjects(objectType, action, match, settings, hint);
        }

        internal static int ProcessObjectsInternal(
            this IEnumerable<EventsCollection> eventsCollectionsIn,
            ObjectType objectType,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings objectDetectionSettings,
            ObjectProcessingHint hint)
        {
            var eventsCollections = eventsCollectionsIn.Where(c => c != null).ToArray();
            var eventsCount = eventsCollections.Sum(c => c.Count);

            var iMatched = 0;

            var processingContext = new ProcessingContext
            {
                TimeOrLengthCanBeChanged = hint.HasFlag(ObjectProcessingHint.TimeOrLengthCanBeChanged),
                NotesCollectionCanBeChanged = hint.HasFlag(ObjectProcessingHint.NotesCollectionCanBeChanged),
                NoteTimeOrLengthCanBeChanged = hint.HasFlag(ObjectProcessingHint.NoteTimeOrLengthCanBeChanged)
            };

            var collectedTimedEvents = processingContext.TimeOrLengthCanBeChanged || processingContext.NotesCollectionCanBeChanged || processingContext.NoteTimeOrLengthCanBeChanged
                ? new List<TimedObjectAt<TimedEvent>>(eventsCount)
                : null;

            var getTimedEvents = objectType.HasFlag(ObjectType.TimedEvent);
            var getNotes = objectType.HasFlag(ObjectType.Note);
            var getChords = objectType.HasFlag(ObjectType.Chord);

            var timedEvents = eventsCollections
                .GetTimedEventsLazy(eventsCount, objectDetectionSettings?.TimedEventDetectionSettings, false, collectedTimedEvents);

            IEnumerable<TimedObjectAt<ITimedObject>> timedObjects = null;
            if (getChords)
                timedObjects = timedEvents
                    .GetChordsAndNotesAndTimedEventsLazy(objectDetectionSettings?.ChordDetectionSettings ?? new ChordDetectionSettings(), objectDetectionSettings?.NoteDetectionSettings ?? new NoteDetectionSettings(), objectDetectionSettings?.TimedEventDetectionSettings ?? new TimedEventDetectionSettings());
            else if (getNotes)
                timedObjects = timedEvents
                    .GetNotesAndTimedEventsLazy(objectDetectionSettings?.NoteDetectionSettings ?? new NoteDetectionSettings());
            else if (getTimedEvents)
                timedObjects = timedEvents
                    .Select(e => new TimedObjectAt<ITimedObject>(e.Object, e.AtIndex));
            else
                return 0;

            foreach (var timedObjectAt in timedObjects)
            {
                if (timedObjectAt.Object is Note && !getNotes && getTimedEvents)
                {
                    var note = timedObjectAt.Object as Note;
                    if (TryProcessTimedEvent(new TimedObjectAt<ITimedObject>(note.TimedNoteOnEvent, timedObjectAt.AtIndex), getTimedEvents, action, match, processingContext, collectedTimedEvents))
                        iMatched++;
                    if (TryProcessTimedEvent(new TimedObjectAt<ITimedObject>(note.TimedNoteOffEvent, timedObjectAt.AtIndex), getTimedEvents, action, match, processingContext, collectedTimedEvents))
                        iMatched++;
                }
                else if (TryProcessTimedEvent(timedObjectAt, getTimedEvents, action, match, processingContext, collectedTimedEvents) ||
                    TryProcessNote(timedObjectAt, getNotes, action, match, processingContext, collectedTimedEvents) ||
                    TryProcessChord(timedObjectAt, getChords, action, match, processingContext, collectedTimedEvents))
                    iMatched++;
            }

            if (processingContext.HasTimingChanges)
                eventsCollections.SortAndUpdateEvents(collectedTimedEvents);

            return iMatched;
        }

        private static bool TryProcessTimedEvent(
            TimedObjectAt<ITimedObject> timedObjectAt,
            bool getTimedEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ProcessingContext processingContext,
            List<TimedObjectAt<TimedEvent>> collectedTimedEvents)
        {
            var timedObject = timedObjectAt.Object;
            if (!(timedObject is TimedEvent))
                return false;

            var timedEvent = (TimedEvent)timedObject;

            if (!getTimedEvents || !match(timedObject))
                return false;

            var deltaTime = timedEvent.Event.DeltaTime;
            var time = timedEvent.Time;

            action(timedEvent);
            timedEvent.Event.DeltaTime = deltaTime;

            processingContext.TimeOrLengthChanged |= timedEvent.Time != time;

            return true;
        }

        private static bool TryProcessNote(
            TimedObjectAt<ITimedObject> timedObjectAt,
            bool getNotes,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ProcessingContext processingContext,
            List<TimedObjectAt<TimedEvent>> collectedTimedEvents)
        {
            var timedObject = timedObjectAt.Object;
            if (!(timedObject is Note))
                return false;

            var note = (Note)timedObject;

            if (!getNotes || !match(timedObject))
                return false;

            var startTime = note.TimedNoteOnEvent.Time;
            var endTime = note.TimedNoteOffEvent.Time;

            action(note);

            processingContext.TimeOrLengthChanged |= note.TimedNoteOnEvent.Time != startTime || note.TimedNoteOffEvent.Time != endTime;

            return true;
        }

        private static bool TryProcessChord(
            TimedObjectAt<ITimedObject> timedObjectAt,
            bool getChords,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ProcessingContext processingContext,
            List<TimedObjectAt<TimedEvent>> collectedTimedEvents)
        {
            var timedObject = timedObjectAt.Object;
            if (!(timedObject is Chord))
                return false;

            var chord = (Chord)timedObject;

            if (!getChords || !match(timedObject))
                return false;

            long time;
            long length;
            chord.GetTimeAndLength(out time, out length);

            var notes = processingContext.NotesCollectionCanBeChanged || processingContext.NoteTimeOrLengthCanBeChanged
                ? chord.Notes.ToArray()
                : null;

            var notesTimes = processingContext.NoteTimeOrLengthCanBeChanged
                ? notes.ToDictionary(n => n, n => n.Time)
                : null;
            var notesLengths = processingContext.NoteTimeOrLengthCanBeChanged
                ? notes.ToDictionary(n => n, n => n.Length)
                : null;

            action(chord);

            long newTime;
            long newLength;
            chord.GetTimeAndLength(out newTime, out newLength);
            processingContext.TimeOrLengthChanged |=
                newTime != time ||
                newLength != length;

            var addedNotes = processingContext.NotesCollectionCanBeChanged ? chord.Notes.Except(notes).ToArray() : null;
            var removedNotes = processingContext.NotesCollectionCanBeChanged ? notes.Except(chord.Notes).ToArray() : null;
            processingContext.NotesCollectionChanged |=
                addedNotes?.Length > 0 ||
                removedNotes?.Length > 0;

            var savedNotes = processingContext.NoteTimeOrLengthCanBeChanged
                ? (processingContext.NotesCollectionCanBeChanged ? (IEnumerable<Note>)notes.Intersect(chord.Notes).ToArray() : chord.Notes)
                : null;
            processingContext.NoteTimeOrLengthChanged |=
                savedNotes?.Any(n => n.Time != notesTimes[n]) == true ||
                savedNotes?.Any(n => n.Length != notesLengths[n]) == true;

            if (processingContext.NotesCollectionChanged)
            {
                foreach (var note in addedNotes)
                {
                    collectedTimedEvents?.Add(new TimedObjectAt<TimedEvent>(
                        note.TimedNoteOnEvent,
                        timedObjectAt.AtIndex));
                    collectedTimedEvents?.Add(new TimedObjectAt<TimedEvent>(
                        note.TimedNoteOffEvent,
                        timedObjectAt.AtIndex));
                }

                foreach (var note in removedNotes)
                {
                    note.TimedNoteOnEvent.Event.Flag = note.TimedNoteOffEvent.Event.Flag = true;
                }
            }

            return true;
        }

        #endregion
    }
}
