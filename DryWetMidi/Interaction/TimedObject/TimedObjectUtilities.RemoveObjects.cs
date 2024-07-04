using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TimedObjectUtilities
    {
        #region Methods

        public static int RemoveObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);

            return eventsCollection.RemoveObjects(objectType, note => true, settings);
        }

        public static int RemoveObjects(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(match), match);

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

        public static int RemoveObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);

            return trackChunk.RemoveObjects(objectType, note => true, settings);
        }

        public static int RemoveObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(match), match);

            return trackChunk.Events.RemoveObjects(objectType, match, settings);
        }

        public static int RemoveObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return trackChunks.RemoveObjects(objectType, note => true, settings);
        }

        public static int RemoveObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(match), match);

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

        public static int RemoveObjects(
            this MidiFile file,
            ObjectType objectType,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);

            return file.RemoveObjects(objectType, note => true, settings);
        }

        public static int RemoveObjects(
            this MidiFile file,
            ObjectType objectType,
            Predicate<ITimedObject> match,
            ObjectDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(match), match);

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
