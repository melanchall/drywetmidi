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
        /// <summary>Asynchronously quantizes objects, preserving custom quantizer hooks.</summary>
        public Task QuantizeAsync(IEnumerable<ITimedObject?> objects, IGrid grid, TempoMap tempoMap, QuantizingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => Quantize(objects, grid, tempoMap, settings), cancellationToken, progress);
    }

    public static partial class QuantizerUtilities
    {
        /// <summary>Asynchronously quantizes objects in a track.</summary>
        public static Task QuantizeObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, IGrid grid, TempoMap tempoMap, QuantizingSettings? quantizerSettings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunk, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously quantizes objects in a track using a custom quantizer.</summary>
        public static Task QuantizeObjectsAsync(this TrackChunk trackChunk, Quantizer quantizer, ObjectType objectType, IGrid grid, TempoMap tempoMap, QuantizingSettings? quantizerSettings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunk, quantizer, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously quantizes objects in tracks.</summary>
        public static Task QuantizeObjectsAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, IGrid grid, TempoMap tempoMap, QuantizingSettings? quantizerSettings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunks, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously quantizes objects in tracks using a custom quantizer.</summary>
        public static Task QuantizeObjectsAsync(this IEnumerable<TrackChunk?> trackChunks, Quantizer quantizer, ObjectType objectType, IGrid grid, TempoMap tempoMap, QuantizingSettings? quantizerSettings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(trackChunks, quantizer, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously quantizes objects in a MIDI file.</summary>
        public static Task QuantizeObjectsAsync(this MidiFile midiFile, ObjectType objectType, IGrid grid, QuantizingSettings? quantizerSettings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(midiFile, objectType, grid, quantizerSettings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously quantizes objects in a MIDI file using a custom quantizer.</summary>
        public static Task QuantizeObjectsAsync(this MidiFile midiFile, Quantizer quantizer, ObjectType objectType, IGrid grid, QuantizingSettings? quantizerSettings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => QuantizeObjects(midiFile, quantizer, objectType, grid, quantizerSettings, objectDetectionSettings), cancellationToken, progress);
    }
}
