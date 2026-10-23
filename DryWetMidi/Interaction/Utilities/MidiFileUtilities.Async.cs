using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class MidiFileUtilities
    {
        /// <inheritdoc cref="GetDuration{TTimeSpan}(TrackChunk, TempoMap)" />
        /// <summary>
        /// Asynchronously calculates a track duration.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this TrackChunk trackChunk,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(() => trackChunk.GetDuration<TTimeSpan>(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(TrackChunk, TimeSpanType, TempoMap)" />
        /// <summary>
        /// Asynchronously calculates a track duration in the specified representation.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this TrackChunk trackChunk,
            TimeSpanType durationType,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetDuration(durationType, tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration{TTimeSpan}(IEnumerable{TrackChunk}, TempoMap)" />
        /// <summary>
        /// Asynchronously calculates the duration of tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this IEnumerable<TrackChunk?> trackChunks,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(() => trackChunks.GetDuration<TTimeSpan>(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(IEnumerable{TrackChunk}, TimeSpanType, TempoMap)" />
        /// <summary>
        /// Asynchronously calculates the duration of tracks in the specified representation.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            TimeSpanType durationType,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetDuration(durationType, tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration{TTimeSpan}(MidiFile)" />
        /// <summary>
        /// Asynchronously calculates a MIDI file duration.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this MidiFile midiFile,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(() => midiFile.GetDuration<TTimeSpan>(), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(MidiFile, TimeSpanType)" />
        /// <summary>
        /// Asynchronously calculates a MIDI file duration in the specified representation.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this MidiFile midiFile,
            TimeSpanType durationType,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiFile.GetDuration(durationType), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration{TTimeSpan}(IEnumerable{TrackChunk}, TempoMap)" />
        /// <summary>
        /// Asynchronously calculates duration from an asynchronous track source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this IAsyncEnumerable<TrackChunk?> trackChunks,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetDuration<TTimeSpan>(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(IEnumerable{TrackChunk}, TimeSpanType, TempoMap)" />
        /// <summary>
        /// Asynchronously calculates duration from an asynchronous track source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this IAsyncEnumerable<TrackChunk?> trackChunks,
            TimeSpanType durationType,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetDuration(durationType, tempoMap), cancellationToken, progress);
    }
}
