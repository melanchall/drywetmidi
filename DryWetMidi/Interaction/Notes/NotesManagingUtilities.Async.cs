using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class NotesManagingUtilities
    {
        /// <summary>Asynchronously detects notes in MIDI events.</summary>
        public static Task<ICollection<Note>> GetNotesAsync(this IEnumerable<MidiEvent> midiEvents, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiEvents.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously detects notes in a track.</summary>
        public static Task<ICollection<Note>> GetNotesAsync(this TrackChunk trackChunk, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously detects notes in tracks.</summary>
        public static Task<ICollection<Note>> GetNotesAsync(this IEnumerable<TrackChunk> trackChunks, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously detects notes in a MIDI file.</summary>
        public static Task<ICollection<Note>> GetNotesAsync(this MidiFile file, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously detects notes from an asynchronous MIDI event source.</summary>
        public static Task<ICollection<Note>> GetNotesAsync(this IAsyncEnumerable<MidiEvent> midiEvents, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(midiEvents, cancellationToken).ConfigureAwait(false)).GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously creates a notes manager.</summary>
        public static Task<TimedObjectsManager<Note>> ManageNotesAsync(this EventsCollection eventsCollection, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, TimedObjectsComparer? comparer = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ManageNotes(settings, timedEventDetectionSettings, comparer), cancellationToken, progress);

        /// <summary>Asynchronously creates a notes manager for a track.</summary>
        public static Task<TimedObjectsManager<Note>> ManageNotesAsync(this TrackChunk trackChunk, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, TimedObjectsComparer? comparer = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ManageNotes(settings, timedEventDetectionSettings, comparer), cancellationToken, progress);

        /// <summary>Asynchronously processes notes using the synchronous processing settings and hint.</summary>
        public static Task<int> ProcessNotesAsync(this EventsCollection eventsCollection, Action<Note> action, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching notes.</summary>
        public static Task<int> ProcessNotesAsync(this EventsCollection eventsCollection, Action<Note> action, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes notes in a track.</summary>
        public static Task<int> ProcessNotesAsync(this TrackChunk trackChunk, Action<Note> action, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching notes in a track.</summary>
        public static Task<int> ProcessNotesAsync(this TrackChunk trackChunk, Action<Note> action, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes notes in tracks.</summary>
        public static Task<int> ProcessNotesAsync(this IEnumerable<TrackChunk> trackChunks, Action<Note> action, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching notes in tracks.</summary>
        public static Task<int> ProcessNotesAsync(this IEnumerable<TrackChunk> trackChunks, Action<Note> action, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes notes in a MIDI file.</summary>
        public static Task<int> ProcessNotesAsync(this MidiFile file, Action<Note> action, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Asynchronously processes matching notes in a MIDI file.</summary>
        public static Task<int> ProcessNotesAsync(this MidiFile file, Action<Note> action, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching notes.</summary>
        public static Task<int> ProcessNotesAsync(this EventsCollection eventsCollection, Func<Note, CancellationToken, ValueTask> action, Predicate<Note>? match = null, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(eventsCollection), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching notes in a track.</summary>
        public static Task<int> ProcessNotesAsync(this TrackChunk trackChunk, Func<Note, CancellationToken, ValueTask> action, Predicate<Note>? match = null, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunk), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching notes in tracks.</summary>
        public static Task<int> ProcessNotesAsync(this IEnumerable<TrackChunk> trackChunks, Func<Note, CancellationToken, ValueTask> action, Predicate<Note>? match = null, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunks), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Sequentially awaits an asynchronous action for matching notes in a MIDI file.</summary>
        public static Task<int> ProcessNotesAsync(this MidiFile file, Func<Note, CancellationToken, ValueTask> action, Predicate<Note>? match = null, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, NoteProcessingHint hint = NoteProcessingHint.Default, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(file), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);

        /// <summary>Asynchronously removes notes.</summary>
        public static Task<int> RemoveNotesAsync(this EventsCollection eventsCollection, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching notes.</summary>
        public static Task<int> RemoveNotesAsync(this EventsCollection eventsCollection, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes notes from a track.</summary>
        public static Task<int> RemoveNotesAsync(this TrackChunk trackChunk, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching notes from a track.</summary>
        public static Task<int> RemoveNotesAsync(this TrackChunk trackChunk, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes notes from tracks.</summary>
        public static Task<int> RemoveNotesAsync(this IEnumerable<TrackChunk> trackChunks, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching notes from tracks.</summary>
        public static Task<int> RemoveNotesAsync(this IEnumerable<TrackChunk> trackChunks, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes notes from a MIDI file.</summary>
        public static Task<int> RemoveNotesAsync(this MidiFile file, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously removes matching notes from a MIDI file.</summary>
        public static Task<int> RemoveNotesAsync(this MidiFile file, Predicate<Note> match, NoteDetectionSettings? settings = null, TimedEventDetectionSettings? timedEventDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        private static async Task<int> ProcessNotesAwaitedAsync(IEnumerable<EventsCollection> eventsCollections, Func<Note, CancellationToken, ValueTask> action, Predicate<Note>? match, NoteDetectionSettings? settings, TimedEventDetectionSettings? timedEventDetectionSettings, NoteProcessingHint hint, CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(action), action);
            var count = 0;
            var index = 0;
            foreach (var eventsCollection in eventsCollections)
            {
                AsyncOperation.Checkpoint();
                var timedEvents = eventsCollection.GetTimedEventsLazy(timedEventDetectionSettings, index++, false).ToArray();
                var changed = false;
                foreach (var note in timedEvents.GetNotesAndTimedEventsLazy(settings).OfType<Note>())
                {
                    AsyncOperation.Checkpoint();
                    if (match != null && !match(note))
                        continue;
                    var start = note.TimedNoteOnEvent.Time;
                    var end = note.TimedNoteOffEvent.Time;
                    await action(note, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    changed |= start != note.TimedNoteOnEvent.Time || end != note.TimedNoteOffEvent.Time;
                    count++;
                }
                if (changed && hint.HasFlag(NoteProcessingHint.TimeOrLengthCanBeChanged))
                    eventsCollection.SortAndUpdateEvents(timedEvents);
            }
            return count;
        }
    }
}
