using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        /// <summary>Asynchronously takes the beginning of a MIDI file.</summary>
        public static Task<MidiFile> TakePartAsync(this MidiFile midiFile, ITimeSpan partLength, SliceMidiFileSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => TakePart(midiFile, partLength, settings), cancellationToken, progress);

        /// <summary>Asynchronously takes a part of a MIDI file.</summary>
        public static Task<MidiFile> TakePartAsync(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => TakePart(midiFile, partStart, partLength, settings), cancellationToken, progress);

        /// <summary>Asynchronously skips the beginning of a MIDI file.</summary>
        public static Task<MidiFile> SkipPartAsync(this MidiFile midiFile, ITimeSpan partLength, SliceMidiFileSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SkipPart(midiFile, partLength, settings), cancellationToken, progress);

        /// <summary>Asynchronously cuts a part from a MIDI file.</summary>
        public static Task<MidiFile> CutPartAsync(this MidiFile midiFile, ITimeSpan partStart, ITimeSpan partLength, SliceMidiFileSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => CutPart(midiFile, partStart, partLength, settings), cancellationToken, progress);

        /// <summary>Lazily splits a MIDI file by channels asynchronously.</summary>
        public static IAsyncEnumerable<MidiFile> SplitByChannelAsync(this MidiFile midiFile, SplitFileByChannelSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByChannel(midiFile, settings), progress, cancellationToken);

        /// <summary>Lazily splits a MIDI file by chunks asynchronously.</summary>
        public static IAsyncEnumerable<MidiFile> SplitByChunksAsync(this MidiFile midiFile, SplitFileByChunksSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByChunks(midiFile, settings), progress, cancellationToken);

        /// <summary>Lazily splits a MIDI file by a grid asynchronously.</summary>
        public static IAsyncEnumerable<MidiFile> SplitByGridAsync(this MidiFile midiFile, IGrid grid, SliceMidiFileSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByGrid(midiFile, grid, settings), progress, cancellationToken);

        /// <summary>Lazily splits a MIDI file by objects asynchronously.</summary>
        public static IAsyncEnumerable<MidiFile> SplitByObjectsAsync(this MidiFile midiFile, ObjectType objectType, SplitByObjectsSettings? settings = null, ObjectDetectionSettings? objectDetectionSettings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByObjects(midiFile, objectType, settings, objectDetectionSettings), progress, cancellationToken);

        /// <summary>Asynchronously splits objects in a track by a step.</summary>
        public static Task SplitObjectsByStepAsync(this TrackChunk trackChunk, ObjectType objectType, ITimeSpan step, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByStep(trackChunk, objectType, step, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in tracks by a step.</summary>
        public static Task SplitObjectsByStepAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, ITimeSpan step, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByStep(trackChunks, objectType, step, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in a MIDI file by a step.</summary>
        public static Task SplitObjectsByStepAsync(this MidiFile midiFile, ObjectType objectType, ITimeSpan step, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByStep(midiFile, objectType, step, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Lazily splits objects by a step asynchronously.</summary>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsByStepAsync(this IEnumerable<ITimedObject?> objects, ITimeSpan step, TempoMap tempoMap, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsByStep(objects, step, tempoMap, filter), progress, cancellationToken);

        /// <summary>Asynchronously splits objects in a track by a grid.</summary>
        public static Task SplitObjectsByGridAsync(this TrackChunk trackChunk, ObjectType objectType, IGrid grid, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByGrid(trackChunk, objectType, grid, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in tracks by a grid.</summary>
        public static Task SplitObjectsByGridAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, IGrid grid, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByGrid(trackChunks, objectType, grid, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in a MIDI file by a grid.</summary>
        public static Task SplitObjectsByGridAsync(this MidiFile midiFile, ObjectType objectType, IGrid grid, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByGrid(midiFile, objectType, grid, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Lazily splits objects by a grid asynchronously.</summary>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsByGridAsync(this IEnumerable<ITimedObject?> objects, IGrid grid, TempoMap tempoMap, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsByGrid(objects, grid, tempoMap, filter), progress, cancellationToken);

        /// <summary>Asynchronously splits objects in a track into a number of parts.</summary>
        public static Task SplitObjectsByPartsNumberAsync(this TrackChunk trackChunk, ObjectType objectType, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByPartsNumber(trackChunk, objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in tracks into a number of parts.</summary>
        public static Task SplitObjectsByPartsNumberAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByPartsNumber(trackChunks, objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in a MIDI file into a number of parts.</summary>
        public static Task SplitObjectsByPartsNumberAsync(this MidiFile midiFile, ObjectType objectType, int partsNumber, TimeSpanType lengthType, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByPartsNumber(midiFile, objectType, partsNumber, lengthType, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Lazily splits objects into a number of parts asynchronously.</summary>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsByPartsNumberAsync(this IEnumerable<ITimedObject?> objects, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsByPartsNumber(objects, partsNumber, lengthType, tempoMap, filter), progress, cancellationToken);

        /// <summary>Asynchronously splits objects in a track at a distance.</summary>
        public static Task SplitObjectsAtDistanceAsync(this TrackChunk trackChunk, ObjectType objectType, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsAtDistance(trackChunk, objectType, distance, from, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in tracks at a distance.</summary>
        public static Task SplitObjectsAtDistanceAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsAtDistance(trackChunks, objectType, distance, from, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in a MIDI file at a distance.</summary>
        public static Task SplitObjectsAtDistanceAsync(this MidiFile midiFile, ObjectType objectType, ITimeSpan distance, LengthedObjectTarget from, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsAtDistance(midiFile, objectType, distance, from, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Lazily splits objects at a distance asynchronously.</summary>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsAtDistanceAsync(this IEnumerable<ITimedObject?> objects, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsAtDistance(objects, distance, from, tempoMap, filter), progress, cancellationToken);

        /// <summary>Asynchronously splits objects in a track at a relative distance.</summary>
        public static Task SplitObjectsAtDistanceAsync(this TrackChunk trackChunk, ObjectType objectType, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsAtDistance(trackChunk, objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in tracks at a relative distance.</summary>
        public static Task SplitObjectsAtDistanceAsync(this IEnumerable<TrackChunk?> trackChunks, ObjectType objectType, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsAtDistance(trackChunks, objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Asynchronously splits objects in a MIDI file at a relative distance.</summary>
        public static Task SplitObjectsAtDistanceAsync(this MidiFile midiFile, ObjectType objectType, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, ObjectDetectionSettings? objectDetectionSettings = null, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsAtDistance(midiFile, objectType, ratio, lengthType, from, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>Lazily splits objects at a relative distance asynchronously.</summary>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsAtDistanceAsync(this IEnumerable<ITimedObject?> objects, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, Predicate<ITimedObject>? filter = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsAtDistance(objects, ratio, lengthType, from, tempoMap, filter), progress, cancellationToken);
    }
}
