using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public partial class Quantizer
    {
        /// <summary>
        /// Asynchronously quantizes objects, preserving custom quantizer hooks.
        /// </summary>
        /// <inheritdoc cref="Quantize(IEnumerable{ITimedObject}, IGrid, TempoMap, QuantizingSettings)"/>
        /// <param name="objects">Objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Quantizing settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task QuantizeAsync(
            IEnumerable<ITimedObject?> objects,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Quantize(objects, grid, tempoMap, settings), cancellationToken, progress);
    }

    public static partial class QuantizerUtilities
    {
        /// <summary>
        /// Asynchronously quantizes objects in a track.
        /// </summary>
        /// <inheritdoc cref="QuantizeObjects(TrackChunk, ObjectType, IGrid, TempoMap, QuantizingSettings, ObjectDetectionSettings)"/>
        /// <param name="trackChunk">Track whose objects should be quantized.</param>
        /// <param name="objectType">Types of objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="quantizerSettings">Quantizing settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task QuantizeObjectsAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings? quantizerSettings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunk, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously quantizes objects in a track using a custom quantizer.
        /// </summary>
        /// <inheritdoc cref="QuantizeObjects(TrackChunk, Quantizer, ObjectType, IGrid, TempoMap, QuantizingSettings, ObjectDetectionSettings)"/>
        /// <param name="trackChunk">Track whose objects should be quantized.</param>
        /// <param name="quantizer">Quantizer to use.</param>
        /// <param name="objectType">Types of objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="quantizerSettings">Quantizing settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task QuantizeObjectsAsync(
            this TrackChunk trackChunk,
            Quantizer quantizer,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings? quantizerSettings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunk, quantizer, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously quantizes objects in tracks.
        /// </summary>
        /// <inheritdoc cref="QuantizeObjects(IEnumerable{TrackChunk}, ObjectType, IGrid, TempoMap, QuantizingSettings, ObjectDetectionSettings)"/>
        /// <param name="trackChunks">Tracks whose objects should be quantized.</param>
        /// <param name="objectType">Types of objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="quantizerSettings">Quantizing settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task QuantizeObjectsAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings? quantizerSettings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunks, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously quantizes objects in tracks using a custom quantizer.
        /// </summary>
        /// <inheritdoc cref="QuantizeObjects(IEnumerable{TrackChunk}, Quantizer, ObjectType, IGrid, TempoMap, QuantizingSettings, ObjectDetectionSettings)"/>
        /// <param name="trackChunks">Tracks whose objects should be quantized.</param>
        /// <param name="quantizer">Quantizer to use.</param>
        /// <param name="objectType">Types of objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="quantizerSettings">Quantizing settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task QuantizeObjectsAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            Quantizer quantizer,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings? quantizerSettings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunks, quantizer, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously quantizes objects in a MIDI file.
        /// </summary>
        /// <inheritdoc cref="QuantizeObjects(MidiFile, ObjectType, IGrid, QuantizingSettings, ObjectDetectionSettings)"/>
        /// <param name="midiFile">File whose objects should be quantized.</param>
        /// <param name="objectType">Types of objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="quantizerSettings">Quantizing settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task QuantizeObjectsAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            IGrid grid,
            QuantizingSettings? quantizerSettings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(midiFile, objectType, grid, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously quantizes objects in a MIDI file using a custom quantizer.
        /// </summary>
        /// <inheritdoc cref="QuantizeObjects(MidiFile, Quantizer, ObjectType, IGrid, QuantizingSettings, ObjectDetectionSettings)"/>
        /// <param name="midiFile">File whose objects should be quantized.</param>
        /// <param name="quantizer">Quantizer to use.</param>
        /// <param name="objectType">Types of objects to quantize.</param>
        /// <param name="grid">Grid to quantize to.</param>
        /// <param name="quantizerSettings">Quantizing settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the quantization.</returns>
        /// <exception cref="InvalidOperationException">A quantizing policy aborts a move beyond zero or a fixed opposite end.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task QuantizeObjectsAsync(
            this MidiFile midiFile,
            Quantizer quantizer,
            ObjectType objectType,
            IGrid grid,
            QuantizingSettings? quantizerSettings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(midiFile, quantizer, objectType, grid, quantizerSettings, objectDetectionSettings), cancellationToken, progress);
    }
}
