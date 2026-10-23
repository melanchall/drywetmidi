using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Resizer
    {
        /// <summary>
        /// Asynchronously resizes a track using the specified length.
        /// </summary>
        /// <inheritdoc cref="Resize(TrackChunk, ITimeSpan, TempoMap)"/>
        /// <param name="trackChunk">Track to resize.</param>
        /// <param name="length">Desired length.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeAsync(
            this TrackChunk trackChunk,
            ITimeSpan length,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunk, length, tempoMap), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes a track using the specified ratio.
        /// </summary>
        /// <inheritdoc cref="Resize(TrackChunk, double)"/>
        /// <param name="trackChunk">Track to resize.</param>
        /// <param name="ratio">Ratio to multiply times and lengths by.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeAsync(
            this TrackChunk trackChunk,
            double ratio,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunk, ratio), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes tracks using the specified length.
        /// </summary>
        /// <inheritdoc cref="Resize(IEnumerable{TrackChunk}, ITimeSpan, TempoMap)"/>
        /// <param name="trackChunks">Tracks to resize.</param>
        /// <param name="length">Desired length.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ITimeSpan length,
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunks, length, tempoMap), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes tracks using the specified ratio.
        /// </summary>
        /// <inheritdoc cref="Resize(IEnumerable{TrackChunk}, double)"/>
        /// <param name="trackChunks">Tracks to resize.</param>
        /// <param name="ratio">Ratio to multiply times and lengths by.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            double ratio,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(trackChunks, ratio), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes a MIDI file using the specified length.
        /// </summary>
        /// <inheritdoc cref="Resize(MidiFile, ITimeSpan)"/>
        /// <param name="midiFile">File to resize.</param>
        /// <param name="length">Desired length.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeAsync(
            this MidiFile midiFile,
            ITimeSpan length,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(midiFile, length), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes a MIDI file using the specified ratio.
        /// </summary>
        /// <inheritdoc cref="Resize(MidiFile, double)"/>
        /// <param name="midiFile">File to resize.</param>
        /// <param name="ratio">Ratio to multiply times and lengths by.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeAsync(
            this MidiFile midiFile,
            double ratio,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Resize(midiFile, ratio), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes a group of objects using the specified length.
        /// </summary>
        /// <inheritdoc cref="ResizeObjectsGroup(IEnumerable{ITimedObject}, ITimeSpan, TempoMap, ObjectsGroupResizingSettings)"/>
        /// <param name="objects">Objects to resize as a group.</param>
        /// <param name="length">Desired group length.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Group resizing settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeObjectsGroupAsync(
            this IEnumerable<ITimedObject?> objects,
            ITimeSpan length,
            TempoMap tempoMap,
            ObjectsGroupResizingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => ResizeObjectsGroup(objects, length, tempoMap, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously resizes a group of objects using the specified ratio.
        /// </summary>
        /// <inheritdoc cref="ResizeObjectsGroup(IEnumerable{ITimedObject}, double, TempoMap, ObjectsGroupResizingSettings)"/>
        /// <param name="objects">Objects to resize as a group.</param>
        /// <param name="ratio">Ratio to multiply group distances and lengths by.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Group resizing settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the resize.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task ResizeObjectsGroupAsync(
            this IEnumerable<ITimedObject?> objects,
            double ratio,
            TempoMap tempoMap,
            ObjectsGroupResizingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => ResizeObjectsGroup(objects, ratio, tempoMap, settings), cancellationToken, progress);
    }
}
