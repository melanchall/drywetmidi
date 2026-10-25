using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TempoMapManagingUtilities
    {
        /// <inheritdoc cref="GetTempoMap(IEnumerable{TrackChunk}, TimeDivision)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects the tempo map of tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMap> GetTempoMapAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="GetTempoMap(MidiFile)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects the tempo map of a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMap> GetTempoMapAsync(
            this MidiFile file,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.GetTempoMap(), cancellationToken, progress);

        /// <inheritdoc cref="GetTempoMap(IEnumerable{TrackChunk}, TimeDivision)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously detects the tempo map from an asynchronous track source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMap> GetTempoMapAsync(
            this IAsyncEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetTempoMap(timeDivision), cancellationToken, progress);
        }

        /// <inheritdoc cref="ManageTempoMap(IEnumerable{EventsCollection}, TimeDivision)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a tempo map manager for events collections.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> ManageTempoMapAsync(
            this IEnumerable<EventsCollection> eventsCollections,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollections.ManageTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="ManageTempoMap(IEnumerable{TrackChunk}, TimeDivision)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a tempo map manager for tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> ManageTempoMapAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ManageTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="ManageTempoMap(MidiFile)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates a tempo map manager for a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> ManageTempoMapAsync(
            this MidiFile file,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ManageTempoMap(), cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap(IEnumerable{EventsCollection}, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously replaces the tempo map in events collections.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ReplaceTempoMapAsync(
            this IEnumerable<EventsCollection> eventsCollections,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollections.ReplaceTempoMap(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap(IEnumerable{TrackChunk}, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously replaces the tempo map in tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ReplaceTempoMapAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ReplaceTempoMap(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap(MidiFile, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously replaces the tempo map in a MIDI file.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ReplaceTempoMapAsync(
            this MidiFile file,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ReplaceTempoMap(tempoMap), cancellationToken, progress);
    }
}
