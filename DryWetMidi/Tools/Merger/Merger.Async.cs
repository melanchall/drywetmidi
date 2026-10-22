using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Merger
    {
        /// <summary>Asynchronously merges MIDI files sequentially.</summary>
        public static Task<MidiFile> MergeSequentiallyAsync(this IEnumerable<MidiFile> midiFiles, SequentialMergingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeSequentially(midiFiles, settings), cancellationToken, progress);

        /// <summary>Asynchronously merges MIDI files simultaneously.</summary>
        public static Task<MidiFile> MergeSimultaneouslyAsync(this IEnumerable<MidiFile> midiFiles, SimultaneousMergingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeSimultaneously(midiFiles, settings), cancellationToken, progress);

        /// <summary>Asynchronously merges objects in a track.</summary>
        public static Task MergeObjectsAsync(this TrackChunk trackChunk, ObjectType objectType, TempoMap tempoMap, ObjectsMergingSettings? settings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeObjects(trackChunk, objectType, tempoMap, settings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously merges objects in tracks.</summary>
        public static Task MergeObjectsAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, TempoMap tempoMap, ObjectsMergingSettings? settings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeObjects(trackChunks, objectType, tempoMap, settings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Asynchronously merges objects in a MIDI file.</summary>
        public static Task MergeObjectsAsync(this MidiFile midiFile, ObjectType objectType, ObjectsMergingSettings? settings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeObjects(midiFile, objectType, settings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>Lazily merges objects asynchronously as the result is enumerated.</summary>
        public static IAsyncEnumerable<ITimedObject> MergeObjectsAsync(this IEnumerable<ITimedObject?> objects, TempoMap tempoMap, ObjectsMergingSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => MergeObjects(objects, tempoMap, settings), progress, cancellationToken);
    }
}
