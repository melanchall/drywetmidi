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
        /// <inheritdoc cref="GetDuration{TTimeSpan}(TrackChunk, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates a track duration.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this TrackChunk trackChunk,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(() => trackChunk.GetDuration<TTimeSpan>(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(TrackChunk, TimeSpanType, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates a track duration in the specified representation.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this TrackChunk trackChunk,
            TimeSpanType durationType,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetDuration(durationType, tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration{TTimeSpan}(IEnumerable{TrackChunk}, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates the duration of tracks.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this IEnumerable<TrackChunk?> trackChunks,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(() => trackChunks.GetDuration<TTimeSpan>(tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(IEnumerable{TrackChunk}, TimeSpanType, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates the duration of tracks in the specified representation.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            TimeSpanType durationType,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetDuration(durationType, tempoMap), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration{TTimeSpan}(MidiFile)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates a MIDI file duration.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this MidiFile midiFile,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan =>
            AsyncOperation.Run(() => midiFile.GetDuration<TTimeSpan>(), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration(MidiFile, TimeSpanType)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates a MIDI file duration in the specified representation.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this MidiFile midiFile,
            TimeSpanType durationType,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiFile.GetDuration(durationType), cancellationToken, progress);

        /// <inheritdoc cref="GetDuration{TTimeSpan}(IEnumerable{TrackChunk}, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates duration from an asynchronous track source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TTimeSpan> GetDurationAsync<TTimeSpan>(
            this IAsyncEnumerable<TrackChunk?> trackChunks,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            where TTimeSpan : class, ITimeSpan
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetDuration<TTimeSpan>(tempoMap), cancellationToken, progress);
        }

        /// <inheritdoc cref="GetDuration(IEnumerable{TrackChunk}, TimeSpanType, TempoMap)" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously calculates duration from an asynchronous track source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ITimeSpan> GetDurationAsync(
            this IAsyncEnumerable<TrackChunk?> trackChunks,
            TimeSpanType durationType,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);

            return AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetDuration(durationType, tempoMap), cancellationToken, progress);
        }
    }
}
