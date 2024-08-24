using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TimedObjectUtilities
    {
        #region Methods

        /// <summary>
        /// Removes all objects of the specified type from <see cref="EventsCollection"/>. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="eventsCollection"/> is <c>null</c>.</exception>
        public static int RemoveObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return eventsCollection.RemoveTimedEvents();
                case ObjectType.Note:
                    return eventsCollection.RemoveNotes(settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return eventsCollection.RemoveChords(settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            return eventsCollection.RemoveObjects(objectType, obj => true, settings);
        }

        /// <summary>
        /// Removes objects from <see cref="EventsCollection"/>. Objects for removing will be selected by the specified
        /// object type and matching predicate. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="eventsCollection"><see cref="EventsCollection"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="match">The predicate that defines the conditions of an object to remove. Predicate
        /// should return <c>true</c> for an object that must be removed.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="eventsCollection"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return eventsCollection.RemoveTimedEvents(match, settings?.TimedEventDetectionSettings);
                case ObjectType.Note:
                    return eventsCollection.RemoveNotes(match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return eventsCollection.RemoveChords(match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            var objectsToRemoveCount = eventsCollection.ProcessObjects(
                objectType,
                SetObjectFlag,
                match,
                settings,
                ObjectProcessingHint.None);

            if (objectsToRemoveCount == 0)
                return 0;

            eventsCollection.RemoveTimedEvents(e => e.Event.Flag);
            return objectsToRemoveCount;
        }

        /// <summary>
        /// Removes all objects of the specified type from <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is <c>null</c>.</exception>
        public static int RemoveObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunk.RemoveTimedEvents();
                case ObjectType.Note:
                    return trackChunk.RemoveNotes(settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return trackChunk.RemoveChords(settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            return trackChunk.RemoveObjects(objectType, obj => true, settings);
        }

        /// <summary>
        /// Removes objects from <see cref="TrackChunk"/>. Objects for removing will be selected by the specified
        /// object type and matching predicate. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="match">The predicate that defines the conditions of an object to remove. Predicate
        /// should return <c>true</c> for an object that must be removed.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunk.RemoveTimedEvents(match, settings?.TimedEventDetectionSettings);
                case ObjectType.Note:
                    return trackChunk.RemoveNotes(match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return trackChunk.RemoveChords(match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            return trackChunk.Events.RemoveObjects(objectType, match, settings);
        }

        /// <summary>
        /// Removes all objects of the specified type from the collection of <see cref="TrackChunk"/>. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="trackChunks">The collection of <see cref="TrackChunk"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is <c>null</c>.</exception>
        public static int RemoveObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunks.RemoveTimedEvents();
                case ObjectType.Note:
                    return trackChunks.RemoveNotes(settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return trackChunks.RemoveChords(settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            return trackChunks.RemoveObjects(objectType, obj => true, settings);
        }

        /// <summary>
        /// Removes objects from the collection of <see cref="TrackChunk"/>. Objects for removing will be selected
        /// by the specified object type and matching predicate. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="trackChunks">The collection of <see cref="TrackChunk"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="match">The predicate that defines the conditions of an object to remove. Predicate
        /// should return <c>true</c> for an object that must be removed.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return trackChunks.RemoveTimedEvents(match, settings?.TimedEventDetectionSettings);
                case ObjectType.Note:
                    return trackChunks.RemoveNotes(match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return trackChunks.RemoveChords(match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            var objectsToRemoveCount = trackChunks.ProcessObjects(
                objectType,
                SetObjectFlag,
                match,
                settings,
                ObjectProcessingHint.None);

            if (objectsToRemoveCount == 0)
                return 0;

            trackChunks.RemoveTimedEvents(e => e.Event.Flag);
            return objectsToRemoveCount;
        }

        /// <summary>
        /// Removes all objects of the specified type from the <see cref="MidiFile"/>. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="file"/> is <c>null</c>.</exception>
        public static int RemoveObjects(
            this MidiFile file,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return file.RemoveTimedEvents();
                case ObjectType.Note:
                    return file.RemoveNotes(settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return file.RemoveChords(settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            return file.RemoveObjects(objectType, obj => true, settings);
        }

        /// <summary>
        /// Removes objects from <see cref="MidiFile"/>. Objects for removing will be selected
        /// by the specified object type and matching predicate. More info in the
        /// <see href="xref:a_removing_objects#removeobjects">Removing objects: RemoveObjects</see> article.
        /// </summary>
        /// <param name="file"><see cref="MidiFile"/> to search for objects to remove.</param>
        /// <param name="objectType">Types of objects to remove (for example, <c>ObjectType.Chord | ObjectType.Note</c>).</param>
        /// <param name="match">The predicate that defines the conditions of an object to remove. Predicate
        /// should return <c>true</c> for an object that must be removed.</param>
        /// <param name="settings">Settings according to which objects should be detected and built.</param>
        /// <returns>Count of removed objects.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="file"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="match"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static int RemoveObjects(
            this MidiFile file,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

            switch (objectType)
            {
                case ObjectType.TimedEvent:
                    return file.RemoveTimedEvents(match, settings?.TimedEventDetectionSettings);
                case ObjectType.Note:
                    return file.RemoveNotes(match, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
                case ObjectType.Chord:
                    return file.RemoveChords(match, settings?.ChordDetectionSettings, settings?.NoteDetectionSettings, settings?.TimedEventDetectionSettings);
            }

            return file.GetTrackChunks().RemoveObjects(objectType, match, settings);
        }

        private static void SetObjectFlag(ITimedObject obj)
        {
            var timedEvent = obj as TimedEvent;
            if (timedEvent != null)
            {
                timedEvent.Event.Flag = true;
                return;
            }

            var note = obj as Note;
            if (note != null)
            {
                note.TimedNoteOnEvent.Event.Flag = note.TimedNoteOffEvent.Event.Flag = true;
                return;
            }

            var chord = obj as Chord;
            if (chord != null)
            {
                foreach (var n in chord.Notes)
                {
                    n.TimedNoteOnEvent.Event.Flag = n.TimedNoteOffEvent.Event.Flag = true;
                }
                return;
            }
        }

        #endregion
    }
}
