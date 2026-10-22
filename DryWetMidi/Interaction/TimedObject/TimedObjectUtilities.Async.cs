using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TimedObjectUtilities
    {
        /// <summary>Asynchronously processes objects.</summary>
        public static Task<int> ProcessObjectsAsync(this EventsCollection eventsCollection, ObjectType objectType, Action<ITimedObject> action, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessObjects(objectType, action, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching objects.</summary>
        public static Task<int> ProcessObjectsAsync(this EventsCollection eventsCollection, ObjectType objectType, Action<ITimedObject> action, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessObjects(objectType, action, match, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes objects in a track.</summary>
        public static Task<int> ProcessObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, Action<ITimedObject> action, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessObjects(objectType, action, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching objects in a track.</summary>
        public static Task<int> ProcessObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, Action<ITimedObject> action, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessObjects(objectType, action, match, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes objects in tracks.</summary>
        public static Task<int> ProcessObjectsAsync(this IEnumerable<TrackChunk> trackChunks, ObjectType objectType, Action<ITimedObject> action, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessObjects(objectType, action, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching objects in tracks.</summary>
        public static Task<int> ProcessObjectsAsync(this IEnumerable<TrackChunk> trackChunks, ObjectType objectType, Action<ITimedObject> action, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessObjects(objectType, action, match, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes objects in a MIDI file.</summary>
        public static Task<int> ProcessObjectsAsync(this MidiFile file, ObjectType objectType, Action<ITimedObject> action, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessObjects(objectType, action, settings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching objects in a MIDI file.</summary>
        public static Task<int> ProcessObjectsAsync(this MidiFile file, ObjectType objectType, Action<ITimedObject> action, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessObjects(objectType, action, match, settings, hint), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching objects.</summary>
        public static Task<int> ProcessObjectsAsync(this EventsCollection eventsCollection, ObjectType objectType, Func<ITimedObject, CancellationToken, ValueTask> action, Predicate<ITimedObject>? match = null, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessObjectsAwaitedAsync(InteractionAsyncUtilities.Collections(eventsCollection), objectType, action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching objects in a track.</summary>
        public static Task<int> ProcessObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, Func<ITimedObject, CancellationToken, ValueTask> action, Predicate<ITimedObject>? match = null, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessObjectsAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunk), objectType, action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching objects in tracks.</summary>
        public static Task<int> ProcessObjectsAsync(this IEnumerable<TrackChunk> trackChunks, ObjectType objectType, Func<ITimedObject, CancellationToken, ValueTask> action, Predicate<ITimedObject>? match = null, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessObjectsAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunks), objectType, action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching objects in a MIDI file.</summary>
        public static Task<int> ProcessObjectsAsync(this MidiFile file, ObjectType objectType, Func<ITimedObject, CancellationToken, ValueTask> action, Predicate<ITimedObject>? match = null, ObjectDetectionSettings? settings = null, ObjectProcessingHint hint = ObjectProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessObjectsAwaitedAsync(InteractionAsyncUtilities.Collections(file), objectType, action, match, settings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Asynchronously removes objects.</summary>
        public static Task<int> RemoveObjectsAsync(this EventsCollection eventsCollection, ObjectType objectType, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveObjects(objectType, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching objects.</summary>
        public static Task<int> RemoveObjectsAsync(this EventsCollection eventsCollection, ObjectType objectType, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveObjects(objectType, match, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes objects from a track.</summary>
        public static Task<int> RemoveObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveObjects(objectType, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching objects from a track.</summary>
        public static Task<int> RemoveObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveObjects(objectType, match, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes objects from tracks.</summary>
        public static Task<int> RemoveObjectsAsync(this IEnumerable<TrackChunk> trackChunks, ObjectType objectType, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveObjects(objectType, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching objects from tracks.</summary>
        public static Task<int> RemoveObjectsAsync(this IEnumerable<TrackChunk> trackChunks, ObjectType objectType, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveObjects(objectType, match, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes objects from a MIDI file.</summary>
        public static Task<int> RemoveObjectsAsync(this MidiFile file, ObjectType objectType, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveObjects(objectType, settings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching objects from a MIDI file.</summary>
        public static Task<int> RemoveObjectsAsync(this MidiFile file, ObjectType objectType, Predicate<ITimedObject> match, ObjectDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveObjects(objectType, match, settings), cancellationToken, progress);

        internal sealed class AsyncProcessingContext
        {
            internal AsyncProcessingContext(bool timing, bool notes, bool noteTiming)
            {
                Timing = timing;
                Notes = notes;
                NoteTiming = noteTiming;
            }
            internal bool Timing { get; }
            internal bool Notes { get; }
            internal bool NoteTiming { get; }
            internal bool HasChanges { get; set; }
        }

        private static async Task<int> ProcessObjectsAwaitedAsync(IEnumerable<EventsCollection> collections, ObjectType objectType, Func<ITimedObject, CancellationToken, ValueTask> action, Predicate<ITimedObject>? match, ObjectDetectionSettings? settings, ObjectProcessingHint hint, CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(action), action);
            var index = 0;
            var count = 0;
            foreach (var collection in collections)
            {
                AsyncOperation.Checkpoint();
                var events = collection.GetTimedEventsLazy(settings?.TimedEventDetectionSettings, index++, false).ToArray();
                var getChords = objectType.HasFlag(ObjectType.Chord);
                var getNotes = objectType.HasFlag(ObjectType.Note);
                var getEvents = objectType.HasFlag(ObjectType.TimedEvent);
                IEnumerable<ITimedObject> objects = getChords
                    ? events.GetChordsAndNotesAndTimedEventsLazy(settings?.ChordDetectionSettings ?? new ChordDetectionSettings(), settings?.NoteDetectionSettings ?? new NoteDetectionSettings())
                    : getNotes ? events.GetNotesAndTimedEventsLazy(settings?.NoteDetectionSettings) : events;
                var context = new AsyncProcessingContext(
                    hint.HasFlag(ObjectProcessingHint.TimeOrLengthCanBeChanged),
                    hint.HasFlag(ObjectProcessingHint.NotesCollectionCanBeChanged),
                    hint.HasFlag(ObjectProcessingHint.NoteTimeOrLengthCanBeChanged));
                var collected = events.ToList();
                foreach (var obj in objects)
                {
                    AsyncOperation.Checkpoint();
                    if (obj is Note note && !getNotes && getEvents)
                    {
                        if (await ProcessObjectAwaitedAsync(note.TimedNoteOnEvent, action, match, context, collected, cancellationToken).ConfigureAwait(false)) count++;
                        if (await ProcessObjectAwaitedAsync(note.TimedNoteOffEvent, action, match, context, collected, cancellationToken).ConfigureAwait(false)) count++;
                    }
                    else if ((obj is TimedEvent && getEvents) || (obj is Note && getNotes) || (obj is Chord && getChords))
                    {
                        if (await ProcessObjectAwaitedAsync(obj, action, match, context, collected, cancellationToken).ConfigureAwait(false)) count++;
                    }
                }
                if (context.HasChanges)
                    collection.SortAndUpdateEvents(collected);
            }
            return count;
        }

        private static async Task<bool> ProcessObjectAwaitedAsync(ITimedObject obj, Func<ITimedObject, CancellationToken, ValueTask> action, Predicate<ITimedObject>? match, AsyncProcessingContext context, List<TimedEvent> collected, CancellationToken cancellationToken)
        {
            AsyncOperation.Checkpoint();
            if (match != null && !match(obj))
                return false;
            if (obj is Chord chord)
                await ProcessChordAwaitedAsync(chord, action, context, collected, cancellationToken).ConfigureAwait(false);
            else
            {
                var time = obj.Time;
                var length = (obj as ILengthedObject)?.Length;
                var timedEvent = obj as TimedEvent;
                var delta = timedEvent?.Event.DeltaTime;
                await action(obj, cancellationToken).ConfigureAwait(false);
                if (timedEvent != null && delta.HasValue)
                    timedEvent.Event.DeltaTime = delta.Value;
                cancellationToken.ThrowIfCancellationRequested();
                context.HasChanges |= context.Timing && (time != obj.Time || length != (obj as ILengthedObject)?.Length);
            }
            return true;
        }

        internal static async Task ProcessChordAwaitedAsync(Chord chord, Func<ITimedObject, CancellationToken, ValueTask> action, AsyncProcessingContext context, List<TimedEvent> collected, CancellationToken cancellationToken)
        {
            chord.GetTimeAndLength(out var time, out var length);
            var notes = context.Notes || context.NoteTiming ? chord.Notes.ToArray() : Array.Empty<Note>();
            var times = context.NoteTiming ? notes.ToDictionary(n => n, n => n.Time) : null;
            var lengths = context.NoteTiming ? notes.ToDictionary(n => n, n => n.Length) : null;
            await action(chord, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            chord.GetTimeAndLength(out var newTime, out var newLength);
            context.HasChanges |= context.Timing && (time != newTime || length != newLength);
            if (context.Notes)
            {
                foreach (var note in chord.Notes.Except(notes))
                {
                    AsyncOperation.Checkpoint();
                    collected.Add(note.TimedNoteOnEvent);
                    collected.Add(note.TimedNoteOffEvent);
                    context.HasChanges = true;
                }
                foreach (var note in notes.Except(chord.Notes))
                {
                    AsyncOperation.Checkpoint();
                    note.TimedNoteOnEvent.Event.MustBeRemoved = note.TimedNoteOffEvent.Event.MustBeRemoved = true;
                    context.HasChanges = true;
                }
            }
            if (context.NoteTiming)
                context.HasChanges |= notes.Intersect(chord.Notes).Any(n => times![n] != n.Time || lengths![n] != n.Length);
        }
    }
}
