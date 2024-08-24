using Melanchall.DryWetMidi.Core;
using System.Collections.Generic;
using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using System.Text.RegularExpressions;
using System.IO;

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

        /// <summary>
        /// Performs the specified action on each object contained in the <see cref="EventsCollection"/>. Objects
        /// for processing will be selected by the specified object type. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return eventsCollection.ProcessTimedEvents(action, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return eventsCollection.ProcessNotes(action, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return eventsCollection.ProcessChords(action, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return eventsCollection.ProcessObjects(objectType, action, obj => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on objects contained in the <see cref="EventsCollection"/>. Objects
        /// for processing will be selected by the specified object type and matching predicate. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="eventsCollection"/>.</param>
        /// <param name="match">The predicate that defines the conditions of an object to process. Predicate
        /// should return <c>true</c> for an object to process.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return eventsCollection.ProcessTimedEvents(action, match, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return eventsCollection.ProcessNotes(action, match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return eventsCollection.ProcessChords(action, match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return new[] { eventsCollection }.ProcessObjectsInternal(objectType, action, match, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each object contained in the <see cref="TrackChunk"/>. Objects
        /// for processing will be selected by the specified object type. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunk.ProcessTimedEvents(action, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return trackChunk.ProcessNotes(action, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return trackChunk.ProcessChords(action, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return trackChunk.ProcessObjects(objectType, action, obj => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on objects contained in the <see cref="TrackChunk"/>. Objects
        /// for processing will be selected by the specified object type and matching predicate. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="trackChunk"/>.</param>
        /// <param name="match">The predicate that defines the conditions of an object to process. Predicate
        /// should return <c>true</c> for an object to process.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunk.ProcessTimedEvents(action, match, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return trackChunk.ProcessNotes(action, match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return trackChunk.ProcessChords(action, match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return trackChunk.Events.ProcessObjects(objectType, action, match, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each object contained in the collection of <see cref="TrackChunk"/>. Objects
        /// for processing will be selected by the specified object type. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="trackChunks">The collection of <see cref="TrackChunk"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunks.ProcessTimedEvents(action, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return trackChunks.ProcessNotes(action, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return trackChunks.ProcessChords(action, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return trackChunks.ProcessObjects(objectType, action, obj => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on objects contained in the collection of <see cref="TrackChunk"/>. Objects
        /// for processing will be selected by the specified object type and matching predicate. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="trackChunks">The collection of <see cref="TrackChunk"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="trackChunks"/>.</param>
        /// <param name="match">The predicate that defines the conditions of an object to process. Predicate
        /// should return <c>true</c> for an object to process.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunks.ProcessTimedEvents(action, match, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return trackChunks.ProcessNotes(action, match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return trackChunks.ProcessChords(action, match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return trackChunks
                .Where(c => c != null)
                .Select(c => c.Events)
                .ProcessObjectsInternal(objectType, action, match, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on each object contained in <see cref="MidiFile"/>. Objects
        /// for processing will be selected by the specified object type. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int ProcessObjects(
            this MidiFile file,
            ObjectType objectType,
            Action<ITimedObject> action,
            ObjectDetectionSettings settings = null,
            ObjectProcessingHint hint = ObjectProcessingHint.Default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return file.ProcessTimedEvents(action, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return file.ProcessNotes(action, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return file.ProcessChords(action, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

            return file.ProcessObjects(objectType, action, obj => true, settings, hint);
        }

        /// <summary>
        /// Performs the specified action on objects contained in <see cref="MidiFile"/>. Objects
        /// for processing will be selected by the specified object type and matching predicate. More info in the
        /// <see href="xref:a_processing_objects#processobjects">Processing objects: ProcessObjects</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for objects to process.</param>
        /// <param name="objectType">Types of objects to process (for example, <c>ObjectType.Chord | ObjectType.TimedEvent</c>).</param>
        /// <param name="action">The action to perform on each object contained in the
        /// <paramref name="file"/>.</param>
        /// <param name="match">The predicate that defines the conditions of an object to process. Predicate
        /// should return <c>true</c> for an object to process.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <param name="hint">Hint which tells the processing algorithm how it can optimize its performance.
        /// The default value is <see cref="ObjectProcessingHint.Default"/>.</param>
        /// <remarks>
        /// Note that you can always use <see href="xref:a_managers">an object manager</see> to
        /// perform any manipulations with objects but dedicated methods of the <see cref="TimedObjectUtilities"/> will
        /// always be faster and will consume less memory.
        /// </remarks>
        /// <returns>Count of processed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="action"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
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

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return file.ProcessTimedEvents(action, match, settings?.TimedEventDetectionSettings, hint.ToTimedEventProcessingHint());
                case ObjectType.Note:
                    return file.ProcessNotes(action, match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToNoteProcessingHint());
                case ObjectType.Chord:
                    return file.ProcessChords(action, match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings, hint.ToChordProcessingHint());
            }

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

        private static TimedEventProcessingHint ToTimedEventProcessingHint(this ObjectProcessingHint hint)
        {
            var result = TimedEventProcessingHint.None;

            if (hint.HasFlag(ObjectProcessingHint.TimeOrLengthCanBeChanged))
                result |= TimedEventProcessingHint.TimeCanBeChanged;

            return result;
        }

        private static NoteProcessingHint ToNoteProcessingHint(this ObjectProcessingHint hint)
        {
            var result = NoteProcessingHint.None;

            if (hint.HasFlag(ObjectProcessingHint.TimeOrLengthCanBeChanged))
                result |= NoteProcessingHint.TimeOrLengthCanBeChanged;

            return result;
        }

        private static ChordProcessingHint ToChordProcessingHint(this ObjectProcessingHint hint)
        {
            var result = ChordProcessingHint.None;

            if (hint.HasFlag(ObjectProcessingHint.TimeOrLengthCanBeChanged))
                result |= ChordProcessingHint.TimeOrLengthCanBeChanged;
            if (hint.HasFlag(ObjectProcessingHint.NoteTimeOrLengthCanBeChanged))
                result |= ChordProcessingHint.NoteTimeOrLengthCanBeChanged;
            if (hint.HasFlag(ObjectProcessingHint.NotesCollectionCanBeChanged))
                result |= ChordProcessingHint.NotesCollectionCanBeChanged;

            return result;
        }

        #endregion
    }
}
