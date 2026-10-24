using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public partial class Repeater
    {
        /// <summary>
        /// Asynchronously repeats a MIDI file, preserving custom processing hooks.
        /// </summary>
        /// <inheritdoc cref="Repeat(MidiFile, int, RepeatingSettings)" path="/member/exception"/>
        /// <param name="midiFile">File to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task<MidiFile> RepeatAsync(
            MidiFile midiFile,
            int repeatsNumber,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(midiFile, repeatsNumber, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously repeats tracks, preserving custom processing hooks.
        /// </summary>
        /// <inheritdoc cref="Repeat(IEnumerable{TrackChunk}, int, TempoMap, RepeatingSettings)" path="/member/exception"/>
        /// <param name="trackChunks">Tracks to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated tracks.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task<ICollection<TrackChunk>> RepeatAsync(
            IEnumerable<TrackChunk?> trackChunks,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(trackChunks, repeatsNumber, tempoMap, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously repeats a track, preserving custom processing hooks.
        /// </summary>
        /// <inheritdoc cref="Repeat(TrackChunk, int, TempoMap, RepeatingSettings)" path="/member/exception"/>
        /// <param name="trackChunk">Track to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated track.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task<TrackChunk> RepeatAsync(
            TrackChunk trackChunk,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(trackChunk, repeatsNumber, tempoMap, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously repeats objects, preserving custom processing hooks.
        /// </summary>
        /// <inheritdoc cref="Repeat(IEnumerable{ITimedObject}, int, TempoMap, RepeatingSettings)" path="/member/exception"/>
        /// <param name="timedObjects">Objects to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated objects.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task<ICollection<ITimedObject>> RepeatAsync(
            IEnumerable<ITimedObject?> timedObjects,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Repeat(timedObjects, repeatsNumber, tempoMap, settings), cancellationToken, progress);
    }

    public static partial class RepeaterUtilities
    {
        /// <summary>
        /// Asynchronously repeats a MIDI file.
        /// </summary>
        /// <inheritdoc cref="Repeat(MidiFile, int, RepeatingSettings)" path="/member/exception"/>
        /// <param name="midiFile">File to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> RepeatAsync(
            this MidiFile midiFile,
            int repeatsNumber,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(midiFile, repeatsNumber, settings, progress, cancellationToken);

        /// <summary>
        /// Asynchronously repeats a track.
        /// </summary>
        /// <inheritdoc cref="Repeat(TrackChunk, int, TempoMap, RepeatingSettings)" path="/member/exception"/>
        /// <param name="trackChunk">Track to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated track.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TrackChunk> RepeatAsync(
            this TrackChunk trackChunk,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(trackChunk, repeatsNumber, tempoMap, settings, progress, cancellationToken);

        /// <summary>
        /// Asynchronously repeats tracks.
        /// </summary>
        /// <inheritdoc cref="Repeat(IEnumerable{TrackChunk}, int, TempoMap, RepeatingSettings)" path="/member/exception"/>
        /// <param name="trackChunks">Tracks to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated tracks.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<TrackChunk>> RepeatAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(trackChunks, repeatsNumber, tempoMap, settings, progress, cancellationToken);

        /// <summary>
        /// Asynchronously repeats timed objects.
        /// </summary>
        /// <inheritdoc cref="Repeat(IEnumerable{ITimedObject}, int, TempoMap, RepeatingSettings)" path="/member/exception"/>
        /// <param name="timedObjects">Objects to repeat.</param>
        /// <param name="repeatsNumber">Number of repeats to append.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Repeating settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the repeated objects.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> RepeatAsync(
            this IEnumerable<ITimedObject?> timedObjects,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => new Repeater().RepeatAsync(timedObjects, repeatsNumber, tempoMap, settings, progress, cancellationToken);
    }
}
