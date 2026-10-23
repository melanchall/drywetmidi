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
        /// <inheritdoc cref="GetTempoMap(IEnumerable{TrackChunk}, TimeDivision)" />
        /// <summary>
        /// Asynchronously detects the tempo map of tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMap> GetTempoMapAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="GetTempoMap(MidiFile)" />
        /// <summary>
        /// Asynchronously detects the tempo map of a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMap> GetTempoMapAsync(
            this MidiFile file,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.GetTempoMap(), cancellationToken, progress);

        /// <inheritdoc cref="GetTempoMap(IEnumerable{TrackChunk}, TimeDivision)" />
        /// <summary>
        /// Asynchronously detects the tempo map from an asynchronous track source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMap> GetTempoMapAsync(
            this IAsyncEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="ManageTempoMap(IEnumerable{EventsCollection}, TimeDivision)" />
        /// <summary>
        /// Asynchronously creates a tempo map manager for events collections.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> ManageTempoMapAsync(
            this IEnumerable<EventsCollection> eventsCollections,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollections.ManageTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="ManageTempoMap(IEnumerable{TrackChunk}, TimeDivision)" />
        /// <summary>
        /// Asynchronously creates a tempo map manager for tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> ManageTempoMapAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TimeDivision timeDivision,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ManageTempoMap(timeDivision), cancellationToken, progress);

        /// <inheritdoc cref="ManageTempoMap(MidiFile)" />
        /// <summary>
        /// Asynchronously creates a tempo map manager for a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> ManageTempoMapAsync(
            this MidiFile file,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ManageTempoMap(), cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap(IEnumerable{EventsCollection}, TempoMap)" />
        /// <summary>
        /// Asynchronously replaces the tempo map in events collections.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ReplaceTempoMapAsync(
            this IEnumerable<EventsCollection> eventsCollections,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollections.ReplaceTempoMap(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap(IEnumerable{TrackChunk}, TempoMap)" />
        /// <summary>
        /// Asynchronously replaces the tempo map in tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ReplaceTempoMapAsync(
            this IEnumerable<TrackChunk> trackChunks,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.ReplaceTempoMap(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap(MidiFile, TempoMap)" />
        /// <summary>
        /// Asynchronously replaces the tempo map in a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ReplaceTempoMapAsync(
            this MidiFile file,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => file.ReplaceTempoMap(tempoMap), cancellationToken, progress);
    }
}
