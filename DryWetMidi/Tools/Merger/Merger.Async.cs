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
        /// <summary>
        /// Asynchronously merges MIDI files sequentially.
        /// </summary>
        /// <inheritdoc cref="MergeSequentially(IEnumerable{MidiFile}, SequentialMergingSettings)" path="/member/exception"/>
        /// <param name="midiFiles">Files to merge.</param>
        /// <param name="settings">Sequential merging settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the merged file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> MergeSequentiallyAsync(
            this IEnumerable<MidiFile> midiFiles,
            SequentialMergingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeSequentially(midiFiles, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously merges MIDI files simultaneously.
        /// </summary>
        /// <inheritdoc cref="MergeSimultaneously(IEnumerable{MidiFile}, SimultaneousMergingSettings)" path="/member/exception"/>
        /// <param name="midiFiles">Files to merge.</param>
        /// <param name="settings">Simultaneous merging settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the merged file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> MergeSimultaneouslyAsync(
            this IEnumerable<MidiFile> midiFiles,
            SimultaneousMergingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeSimultaneously(midiFiles, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously merges objects in a track.
        /// </summary>
        /// <inheritdoc cref="MergeObjects(TrackChunk, ObjectType, TempoMap, ObjectsMergingSettings, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="trackChunk">Track whose objects should be merged.</param>
        /// <param name="objectType">Types of objects to merge.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Object merging settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the merge.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task MergeObjectsAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            TempoMap tempoMap,
            ObjectsMergingSettings? settings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => MergeObjects(trackChunk, objectType, tempoMap, settings, objectDetectionSettings),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously merges objects in tracks.
        /// </summary>
        /// <inheritdoc cref="MergeObjects(IEnumerable{TrackChunk}, ObjectType, TempoMap, ObjectsMergingSettings, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="trackChunks">Tracks whose objects should be merged.</param>
        /// <param name="objectType">Types of objects to merge.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Object merging settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the merge.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task MergeObjectsAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            TempoMap tempoMap,
            ObjectsMergingSettings? settings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => MergeObjects(trackChunks, objectType, tempoMap, settings, objectDetectionSettings),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously merges objects in a MIDI file.
        /// </summary>
        /// <inheritdoc cref="MergeObjects(MidiFile, ObjectType, ObjectsMergingSettings, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiFile">File whose objects should be merged.</param>
        /// <param name="objectType">Types of objects to merge.</param>
        /// <param name="settings">Object merging settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the merge.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task MergeObjectsAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            ObjectsMergingSettings? settings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => MergeObjects(midiFile, objectType, settings, objectDetectionSettings), cancellationToken, progress);

        /// <summary>
        /// Lazily merges objects asynchronously as the result is enumerated.
        /// </summary>
        /// <inheritdoc cref="MergeObjects(IEnumerable{ITimedObject}, TempoMap, ObjectsMergingSettings)" path="/member/exception"/>
        /// <param name="objects">Objects to merge.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="settings">Object merging settings.</param>
        /// <param name="progress">Optional progress receiver.</param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of merged objects.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject> MergeObjectsAsync(
            this IEnumerable<ITimedObject?> objects,
            TempoMap tempoMap,
            ObjectsMergingSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => MergeObjects(objects, tempoMap, settings), progress, cancellationToken);
    }
}
