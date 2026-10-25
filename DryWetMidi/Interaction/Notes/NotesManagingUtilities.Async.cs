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
        /// <inheritdoc cref="GetNotes(IEnumerable{MidiEvent}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects notes in MIDI events.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Note>> GetNotesAsync(
            this IEnumerable<MidiEvent> midiEvents,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiEvents.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetNotes(TrackChunk, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects notes in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Note>> GetNotesAsync(
            this TrackChunk trackChunk,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetNotes(IEnumerable{TrackChunk}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects notes in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Note>> GetNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetNotes(MidiFile, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects notes in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Note>> GetNotesAsync(
            this MidiFile file,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetNotes(IEnumerable{MidiEvent}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects notes from an asynchronous MIDI event source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Note>> GetNotesAsync(
            this IAsyncEnumerable<MidiEvent> midiEvents,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(midiEvents, cancellationToken).ConfigureAwait(false)).GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);
        }

        /// <inheritdoc cref="GetNotes(IEnumerable{TrackChunk}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects notes from an asynchronous track source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Note>> GetNotesAsync(
            this IAsyncEnumerable<TrackChunk> trackChunks,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetNotes(settings, timedEventDetectionSettings), cancellationToken, progress);
        }

        /// <inheritdoc cref="ManageNotes(EventsCollection, NoteDetectionSettings, TimedEventDetectionSettings, TimedObjectsComparer)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a notes manager.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<Note>> ManageNotesAsync(
            this EventsCollection eventsCollection,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ManageNotes(settings, timedEventDetectionSettings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="ManageNotes(TrackChunk, NoteDetectionSettings, TimedEventDetectionSettings, TimedObjectsComparer)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a notes manager for a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<Note>> ManageNotesAsync(
            this TrackChunk trackChunk,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ManageNotes(settings, timedEventDetectionSettings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(EventsCollection, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes notes using the synchronous processing settings and hint.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this EventsCollection eventsCollection,
            Action<Note> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(EventsCollection, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching notes.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this EventsCollection eventsCollection,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(TrackChunk, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes notes in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this TrackChunk trackChunk,
            Action<Note> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(TrackChunk, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching notes in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this TrackChunk trackChunk,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(IEnumerable{TrackChunk}, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes notes in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Note> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(IEnumerable{TrackChunk}, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching notes in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(MidiFile, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes notes in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this MidiFile file,
            Action<Note> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessNotes(action, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(MidiFile, Action{Note}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching notes in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this MidiFile file,
            Action<Note> action,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessNotes(action, match, settings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessNotes(EventsCollection, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each note.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this EventsCollection eventsCollection,
            Func<Note, CancellationToken, ValueTask> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            eventsCollection.ProcessNotesAsync(action, null, settings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessNotes(TrackChunk, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each note in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this TrackChunk trackChunk,
            Func<Note, CancellationToken, ValueTask> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            trackChunk.ProcessNotesAsync(action, null, settings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessNotes(IEnumerable{TrackChunk}, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each note in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Func<Note, CancellationToken, ValueTask> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            trackChunks.ProcessNotesAsync(action, null, settings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessNotes(MidiFile, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each note in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this MidiFile file,
            Func<Note, CancellationToken, ValueTask> action,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            file.ProcessNotesAsync(action, null, settings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessNotes(EventsCollection, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching notes.
        /// A null match processes all notes.
        /// </summary>
        /// <param name="match">Predicate selecting notes to process, or null to process all notes.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this EventsCollection eventsCollection,
            Func<Note, CancellationToken, ValueTask> action,
            Predicate<Note>? match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(eventsCollection), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="ProcessNotes(TrackChunk, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching notes in a track.
        /// A null match processes all notes.
        /// </summary>
        /// <param name="match">Predicate selecting notes to process, or null to process all notes.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this TrackChunk trackChunk,
            Func<Note, CancellationToken, ValueTask> action,
            Predicate<Note>? match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunk), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="ProcessNotes(IEnumerable{TrackChunk}, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching notes in tracks.
        /// A null match processes all notes.
        /// </summary>
        /// <param name="match">Predicate selecting notes to process, or null to process all notes.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Func<Note, CancellationToken, ValueTask> action,
            Predicate<Note>? match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunks), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="ProcessNotes(MidiFile, Action{Note}, NoteDetectionSettings, TimedEventDetectionSettings, NoteProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching notes in a MIDI file.
        /// A null match processes all notes.
        /// </summary>
        /// <param name="match">Predicate selecting notes to process, or null to process all notes.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessNotesAsync(
            this MidiFile file,
            Func<Note, CancellationToken, ValueTask> action,
            Predicate<Note>? match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            NoteProcessingHint hint = NoteProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessNotesAwaitedAsync(InteractionAsyncUtilities.Collections(file), action, match, settings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="RemoveNotes(EventsCollection, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes notes.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this EventsCollection eventsCollection,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(EventsCollection, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching notes.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this EventsCollection eventsCollection,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(TrackChunk, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes notes from a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this TrackChunk trackChunk,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(TrackChunk, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching notes from a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this TrackChunk trackChunk,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(IEnumerable{TrackChunk}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes notes from tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(IEnumerable{TrackChunk}, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching notes from tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(MidiFile, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes notes from a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this MidiFile file,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveNotes(settings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveNotes(MidiFile, Predicate{Note}, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching notes from a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveNotesAsync(
            this MidiFile file,
            Predicate<Note> match,
            NoteDetectionSettings? settings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveNotes(match, settings, timedEventDetectionSettings), cancellationToken, progress);

        private static async Task<int> ProcessNotesAwaitedAsync(
            IEnumerable<EventsCollection> eventsCollections,
            Func<Note, CancellationToken, ValueTask> action,
            Predicate<Note>? match,
            NoteDetectionSettings? settings,
            TimedEventDetectionSettings? timedEventDetectionSettings,
            NoteProcessingHint hint,
            CancellationToken cancellationToken)
        {
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
