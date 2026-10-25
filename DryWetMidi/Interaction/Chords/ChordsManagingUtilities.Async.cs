using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class ChordsManagingUtilities
    {
        /// <inheritdoc cref="GetChords(IEnumerable{MidiEvent}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects chords in MIDI events.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this IEnumerable<MidiEvent> midiEvents,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiEvents.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetChords(TrackChunk, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects chords in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this TrackChunk trackChunk,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetChords(IEnumerable{TrackChunk}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects chords in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetChords(MidiFile, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects chords in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this MidiFile file,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="GetChords(IEnumerable{Note}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously enumerates chords grouped from notes. Detection starts on enumeration;
        /// a bounded handoff prevents the worker from advancing ahead of the consumer.
        /// Disposing the asynchronous enumerator stops the worker and disposes its enumerator.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<Chord> GetChordsAsync(
            this IEnumerable<Note> notes,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            InteractionAsyncUtilities.EnumerateAsync(() => notes.GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), progress, cancellationToken);

        /// <inheritdoc cref="GetChords(IEnumerable{MidiEvent}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects chords from an asynchronous MIDI event source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this IAsyncEnumerable<MidiEvent> midiEvents,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(midiEvents, cancellationToken).ConfigureAwait(false)).GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);
        }

        /// <inheritdoc cref="GetChords(IEnumerable{TrackChunk}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects chords from an asynchronous track source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this IAsyncEnumerable<TrackChunk> trackChunks,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);
        }

        /// <inheritdoc cref="GetChords(IEnumerable{Note}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously groups notes from an asynchronous source into chords.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Chord>> GetChordsAsync(
            this IAsyncEnumerable<Note> notes,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(notes), notes);

            return AsyncOperation.Run(async () => (ICollection<Chord>)(await InteractionAsyncUtilities.CollectAsync(notes, cancellationToken).ConfigureAwait(false)).GetChords(settings, noteDetectionSettings, timedEventDetectionSettings).ToArray(), cancellationToken, progress);
        }

        /// <inheritdoc cref="ManageChords(EventsCollection, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, TimedObjectsComparer)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a chords manager.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<Chord>> ManageChordsAsync(
            this EventsCollection eventsCollection,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ManageChords(settings, noteDetectionSettings, timedEventDetectionSettings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="ManageChords(TrackChunk, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, TimedObjectsComparer)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a chords manager for a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<Chord>> ManageChordsAsync(
            this TrackChunk trackChunk,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ManageChords(settings, noteDetectionSettings, timedEventDetectionSettings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(EventsCollection, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes chords.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this EventsCollection eventsCollection,
            Action<Chord> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessChords(action, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(EventsCollection, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching chords.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this EventsCollection eventsCollection,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.ProcessChords(action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(TrackChunk, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes chords in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this TrackChunk trackChunk,
            Action<Chord> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessChords(action, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(TrackChunk, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching chords in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this TrackChunk trackChunk,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.ProcessChords(action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(IEnumerable{TrackChunk}, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes chords in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Chord> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessChords(action, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(IEnumerable{TrackChunk}, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching chords in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ProcessChords(action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(MidiFile, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes chords in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this MidiFile file,
            Action<Chord> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessChords(action, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(MidiFile, Action{Chord}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously processes matching chords in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this MidiFile file,
            Action<Chord> action,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ProcessChords(action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint), cancellationToken, progress);

        /// <inheritdoc cref="ProcessChords(EventsCollection, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each chord.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this EventsCollection eventsCollection,
            Func<Chord, CancellationToken, ValueTask> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            eventsCollection.ProcessChordsAsync(action, null, settings, noteDetectionSettings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessChords(TrackChunk, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each chord in a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this TrackChunk trackChunk,
            Func<Chord, CancellationToken, ValueTask> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            trackChunk.ProcessChordsAsync(action, null, settings, noteDetectionSettings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessChords(IEnumerable{TrackChunk}, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each chord in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Func<Chord, CancellationToken, ValueTask> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            trackChunks.ProcessChordsAsync(action, null, settings, noteDetectionSettings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessChords(MidiFile, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for each chord in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this MidiFile file,
            Func<Chord, CancellationToken, ValueTask> action,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            file.ProcessChordsAsync(action, null, settings, noteDetectionSettings, timedEventDetectionSettings, hint, progress, cancellationToken);

        /// <inheritdoc cref="ProcessChords(EventsCollection, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching chords.
        /// A null match processes all chords.
        /// </summary>
        /// <param name="match">Predicate selecting chords to process, or null to process all chords.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this EventsCollection eventsCollection,
            Func<Chord, CancellationToken, ValueTask> action,
            Predicate<Chord>? match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessChordsAwaitedAsync(InteractionAsyncUtilities.Collections(eventsCollection), action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="ProcessChords(TrackChunk, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching chords in a track.
        /// A null match processes all chords.
        /// </summary>
        /// <param name="match">Predicate selecting chords to process, or null to process all chords.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this TrackChunk trackChunk,
            Func<Chord, CancellationToken, ValueTask> action,
            Predicate<Chord>? match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessChordsAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunk), action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="ProcessChords(IEnumerable{TrackChunk}, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching chords in tracks.
        /// A null match processes all chords.
        /// </summary>
        /// <param name="match">Predicate selecting chords to process, or null to process all chords.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Func<Chord, CancellationToken, ValueTask> action,
            Predicate<Chord>? match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessChordsAwaitedAsync(InteractionAsyncUtilities.Collections(trackChunks), action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="ProcessChords(MidiFile, Action{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings, ChordProcessingHint)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Sequentially awaits an asynchronous action for matching chords in a MIDI file.
        /// A null match processes all chords.
        /// </summary>
        /// <param name="match">Predicate selecting chords to process, or null to process all chords.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> ProcessChordsAsync(
            this MidiFile file,
            Func<Chord, CancellationToken, ValueTask> action,
            Predicate<Chord>? match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            ChordProcessingHint hint = ChordProcessingHint.Default,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            ThrowIfArgument.IsNull(nameof(action), action);

            return AsyncOperation.Run(() => ProcessChordsAwaitedAsync(InteractionAsyncUtilities.Collections(file), action, match, settings, noteDetectionSettings, timedEventDetectionSettings, hint, cancellationToken), cancellationToken, progress);
        }

        /// <inheritdoc cref="RemoveChords(EventsCollection, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes chords.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this EventsCollection eventsCollection,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(EventsCollection, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching chords.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this EventsCollection eventsCollection,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.RemoveChords(match, settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(TrackChunk, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes chords from a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this TrackChunk trackChunk,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(TrackChunk, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching chords from a track.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this TrackChunk trackChunk,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.RemoveChords(match, settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(IEnumerable{TrackChunk}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes chords from tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(IEnumerable{TrackChunk}, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching chords from tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this IEnumerable<TrackChunk> trackChunks,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.RemoveChords(match, settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(MidiFile, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes chords from a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this MidiFile file,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveChords(settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        /// <inheritdoc cref="RemoveChords(MidiFile, Predicate{Chord}, ChordDetectionSettings, NoteDetectionSettings, TimedEventDetectionSettings)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously removes matching chords from a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<int> RemoveChordsAsync(
            this MidiFile file,
            Predicate<Chord> match,
            ChordDetectionSettings? settings = null,
            NoteDetectionSettings? noteDetectionSettings = null,
            TimedEventDetectionSettings? timedEventDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.RemoveChords(match, settings, noteDetectionSettings, timedEventDetectionSettings), cancellationToken, progress);

        private static async Task<int> ProcessChordsAwaitedAsync(
            IEnumerable<EventsCollection> eventsCollections,
            Func<Chord, CancellationToken, ValueTask> action,
            Predicate<Chord>? match,
            ChordDetectionSettings? settings,
            NoteDetectionSettings? noteDetectionSettings,
            TimedEventDetectionSettings? timedEventDetectionSettings,
            ChordProcessingHint hint,
            CancellationToken cancellationToken)
        {
            var count = 0;
            var index = 0;

            foreach (var eventsCollection in eventsCollections)
            {
                AsyncOperation.Checkpoint();
                var timedEvents = eventsCollection.GetTimedEventsLazy(timedEventDetectionSettings, index++, false).ToArray();
                var collected = timedEvents.ToList();
                var context = new TimedObjectUtilities.AsyncProcessingContext(
                    hint.HasFlag(ChordProcessingHint.TimeOrLengthCanBeChanged),
                    hint.HasFlag(ChordProcessingHint.NotesCollectionCanBeChanged),
                    hint.HasFlag(ChordProcessingHint.NoteTimeOrLengthCanBeChanged));

                foreach (var chord in timedEvents.GetChordsAndNotesAndTimedEventsLazy(settings ?? new ChordDetectionSettings(), noteDetectionSettings ?? new NoteDetectionSettings()).OfType<Chord>())
                {
                    AsyncOperation.Checkpoint();
                    if (match != null && !match(chord))
                        continue;

                    await TimedObjectUtilities.ProcessChordAwaitedAsync(
                        chord,
                        (obj, token) => action((Chord)obj, token),
                        context,
                        collected,
                        cancellationToken).ConfigureAwait(false);

                    count++;
                }

                if (context.HasChanges)
                    eventsCollection.SortAndUpdateEvents(collected);
            }

            return count;
        }
    }
}
