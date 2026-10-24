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
        /// <summary>
        /// Asynchronously takes the beginning of a MIDI file.
        /// </summary>
        /// <inheritdoc cref="TakePart(MidiFile, ITimeSpan, SliceMidiFileSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="partLength">Length of the part to take.</param>
        /// <param name="settings">Slicing settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the selected part.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> TakePartAsync(
            this MidiFile midiFile,
            ITimeSpan partLength,
            SliceMidiFileSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => TakePart(midiFile, partLength, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously takes a part of a MIDI file.
        /// </summary>
        /// <inheritdoc cref="TakePart(MidiFile, ITimeSpan, ITimeSpan, SliceMidiFileSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="partStart">Start of the part to take.</param>
        /// <param name="partLength">Length of the part to take.</param>
        /// <param name="settings">Slicing settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the selected part.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> TakePartAsync(
            this MidiFile midiFile,
            ITimeSpan partStart,
            ITimeSpan partLength,
            SliceMidiFileSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => TakePart(midiFile, partStart, partLength, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously skips the beginning of a MIDI file.
        /// </summary>
        /// <inheritdoc cref="SkipPart(MidiFile, ITimeSpan, SliceMidiFileSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="partLength">Length of the beginning to skip.</param>
        /// <param name="settings">Slicing settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the remainder of the file.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> SkipPartAsync(
            this MidiFile midiFile,
            ITimeSpan partLength,
            SliceMidiFileSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SkipPart(midiFile, partLength, settings), cancellationToken, progress);

        /// <summary>
        /// Asynchronously cuts a part from a MIDI file.
        /// </summary>
        /// <inheritdoc cref="CutPart(MidiFile, ITimeSpan, ITimeSpan, SliceMidiFileSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="partStart">Start of the part to remove.</param>
        /// <param name="partLength">Length of the part to remove.</param>
        /// <param name="settings">Slicing settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task returning the file with the selected part removed.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<MidiFile> CutPartAsync(
            this MidiFile midiFile,
            ITimeSpan partStart,
            ITimeSpan partLength,
            SliceMidiFileSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => CutPart(midiFile, partStart, partLength, settings), cancellationToken, progress);

        /// <summary>
        /// Lazily splits a MIDI file by channels asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitByChannel(MidiFile, SplitFileByChannelSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="settings">Channel splitting settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of files split by channel.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<MidiFile> SplitByChannelAsync(
            this MidiFile midiFile,
            SplitFileByChannelSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByChannel(midiFile, settings), progress, cancellationToken);

        /// <summary>
        /// Lazily splits a MIDI file by chunks asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitByChunks(MidiFile, SplitFileByChunksSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="settings">Chunk splitting settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of files split by chunk.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<MidiFile> SplitByChunksAsync(
            this MidiFile midiFile,
            SplitFileByChunksSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByChunks(midiFile, settings), progress, cancellationToken);

        /// <summary>
        /// Lazily splits a MIDI file by a grid asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitByGrid(MidiFile, IGrid, SliceMidiFileSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="grid">Grid defining the split positions.</param>
        /// <param name="settings">Slicing settings.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of files split by the grid.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<MidiFile> SplitByGridAsync(
            this MidiFile midiFile,
            IGrid grid,
            SliceMidiFileSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitByGrid(midiFile, grid, settings), progress, cancellationToken);

        /// <summary>
        /// Lazily splits a MIDI file by objects asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitByObjects(MidiFile, ObjectType, SplitByObjectsSettings, ObjectDetectionSettings)" path="/member/exception"/>
        /// <param name="midiFile">Source MIDI file.</param>
        /// <param name="objectType">Types of objects used to split the file.</param>
        /// <param name="settings">Object splitting settings.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of files split by objects.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<MidiFile> SplitByObjectsAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            SplitByObjectsSettings? settings = null,
            ObjectDetectionSettings? objectDetectionSettings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(
                () => SplitByObjects(midiFile, objectType, settings, objectDetectionSettings),
                progress,
                cancellationToken);

        /// <summary>
        /// Asynchronously splits objects in a track by a step.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByStep(TrackChunk, ObjectType, ITimeSpan, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunk">Track whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="step">Length of each split part.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="InvalidOperationException">The step converts to zero MIDI ticks at a split position.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByStepAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ITimeSpan step,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByStep(trackChunk, objectType, step, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in tracks by a step.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByStep(IEnumerable{TrackChunk}, ObjectType, ITimeSpan, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunks">Tracks whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="step">Length of each split part.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="InvalidOperationException">The step converts to zero MIDI ticks at a split position.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByStepAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            ITimeSpan step,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByStep(trackChunks, objectType, step, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in a MIDI file by a step.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByStep(MidiFile, ObjectType, ITimeSpan, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="midiFile">File whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="step">Length of each split part.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="InvalidOperationException">The step converts to zero MIDI ticks at a split position.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByStepAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            ITimeSpan step,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByStep(midiFile, objectType, step, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>
        /// Lazily splits objects by a step asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByStep(IEnumerable{ITimedObject}, ITimeSpan, TempoMap, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="objects">Objects to split.</param>
        /// <param name="step">Length of each split part.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of split objects.</returns>
        /// <exception cref="InvalidOperationException">The step converts to zero MIDI ticks at a split position.</exception>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsByStepAsync(
            this IEnumerable<ITimedObject?> objects,
            ITimeSpan step,
            TempoMap tempoMap,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsByStep(objects, step, tempoMap, filter), progress, cancellationToken);

        /// <summary>
        /// Asynchronously splits objects in a track by a grid.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByGrid(TrackChunk, ObjectType, IGrid, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunk">Track whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="grid">Grid defining the split positions.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByGridAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByGrid(trackChunk, objectType, grid, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in tracks by a grid.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByGrid(IEnumerable{TrackChunk}, ObjectType, IGrid, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunks">Tracks whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="grid">Grid defining the split positions.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByGridAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByGrid(trackChunks, objectType, grid, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in a MIDI file by a grid.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByGrid(MidiFile, ObjectType, IGrid, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="midiFile">File whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="grid">Grid defining the split positions.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByGridAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            IGrid grid,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(() => SplitObjectsByGrid(midiFile, objectType, grid, objectDetectionSettings, filter), cancellationToken, progress);

        /// <summary>
        /// Lazily splits objects by a grid asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByGrid(IEnumerable{ITimedObject}, IGrid, TempoMap, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="objects">Objects to split.</param>
        /// <param name="grid">Grid defining the split positions.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of split objects.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsByGridAsync(
            this IEnumerable<ITimedObject?> objects,
            IGrid grid,
            TempoMap tempoMap,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsByGrid(objects, grid, tempoMap, filter), progress, cancellationToken);

        /// <summary>
        /// Asynchronously splits objects in a track into a number of parts.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByPartsNumber(TrackChunk, ObjectType, int, TimeSpanType, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunk">Track whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="partsNumber">Number of parts per object.</param>
        /// <param name="lengthType">Time span type used to divide object lengths.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByPartsNumberAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByPartsNumber(trackChunk, objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in tracks into a number of parts.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByPartsNumber(IEnumerable{TrackChunk}, ObjectType, int, TimeSpanType, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunks">Tracks whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="partsNumber">Number of parts per object.</param>
        /// <param name="lengthType">Time span type used to divide object lengths.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByPartsNumberAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByPartsNumber(trackChunks, objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in a MIDI file into a number of parts.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByPartsNumber(MidiFile, ObjectType, int, TimeSpanType, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="midiFile">File whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="partsNumber">Number of parts per object.</param>
        /// <param name="lengthType">Time span type used to divide object lengths.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsByPartsNumberAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsByPartsNumber(midiFile, objectType, partsNumber, lengthType, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Lazily splits objects into a number of parts asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsByPartsNumber(IEnumerable{ITimedObject}, int, TimeSpanType, TempoMap, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="objects">Objects to split.</param>
        /// <param name="partsNumber">Number of parts per object.</param>
        /// <param name="lengthType">Time span type used to divide object lengths.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of split objects.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsByPartsNumberAsync(
            this IEnumerable<ITimedObject?> objects,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(
                () => SplitObjectsByPartsNumber(objects, partsNumber, lengthType, tempoMap, filter),
                progress,
                cancellationToken);

        /// <summary>
        /// Asynchronously splits objects in a track at a distance.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(TrackChunk, ObjectType, ITimeSpan, LengthedObjectTarget, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunk">Track whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="distance">Distance from the selected object end to the split position.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsAtDistanceAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsAtDistance(trackChunk, objectType, distance, from, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in tracks at a distance.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(IEnumerable{TrackChunk}, ObjectType, ITimeSpan, LengthedObjectTarget, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunks">Tracks whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="distance">Distance from the selected object end to the split position.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsAtDistanceAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsAtDistance(trackChunks, objectType, distance, from, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in a MIDI file at a distance.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(MidiFile, ObjectType, ITimeSpan, LengthedObjectTarget, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="midiFile">File whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="distance">Distance from the selected object end to the split position.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsAtDistanceAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsAtDistance(midiFile, objectType, distance, from, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Lazily splits objects at a distance asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(IEnumerable{ITimedObject}, ITimeSpan, LengthedObjectTarget, TempoMap, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="objects">Objects to split.</param>
        /// <param name="distance">Distance from the selected object end to the split position.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of split objects.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsAtDistanceAsync(
            this IEnumerable<ITimedObject?> objects,
            ITimeSpan distance,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(() => SplitObjectsAtDistance(objects, distance, from, tempoMap, filter), progress, cancellationToken);

        /// <summary>
        /// Asynchronously splits objects in a track at a relative distance.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(TrackChunk, ObjectType, double, TimeSpanType, LengthedObjectTarget, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunk">Track whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="ratio">Fraction of the object length to measure from the selected end.</param>
        /// <param name="lengthType">Time span type used to measure object lengths.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsAtDistanceAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsAtDistance(trackChunk, objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in tracks at a relative distance.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(IEnumerable{TrackChunk}, ObjectType, double, TimeSpanType, LengthedObjectTarget, TempoMap, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="trackChunks">Tracks whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="ratio">Fraction of the object length to measure from the selected end.</param>
        /// <param name="lengthType">Time span type used to measure object lengths.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsAtDistanceAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsAtDistance(trackChunks, objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Asynchronously splits objects in a MIDI file at a relative distance.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(MidiFile, ObjectType, double, TimeSpanType, LengthedObjectTarget, ObjectDetectionSettings, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="midiFile">File whose objects should be split.</param>
        /// <param name="objectType">Types of objects to split.</param>
        /// <param name="ratio">Fraction of the object length to measure from the selected end.</param>
        /// <param name="lengthType">Time span type used to measure object lengths.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="objectDetectionSettings">Settings used to detect objects.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>A task representing the split.</returns>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task SplitObjectsAtDistanceAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            ObjectDetectionSettings? objectDetectionSettings = null,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => AsyncOperation.Run(
                () => SplitObjectsAtDistance(midiFile, objectType, ratio, lengthType, from, objectDetectionSettings, filter),
                cancellationToken,
                progress);

        /// <summary>
        /// Lazily splits objects at a relative distance asynchronously.
        /// </summary>
        /// <inheritdoc cref="SplitObjectsAtDistance(IEnumerable{ITimedObject}, double, TimeSpanType, LengthedObjectTarget, TempoMap, Predicate{ITimedObject})" path="/member/exception"/>
        /// <param name="objects">Objects to split.</param>
        /// <param name="ratio">Fraction of the object length to measure from the selected end.</param>
        /// <param name="lengthType">Time span type used to measure object lengths.</param>
        /// <param name="from">Object end from which the distance is measured.</param>
        /// <param name="tempoMap">Tempo map used for time and length calculations.</param>
        /// <param name="filter">Optional predicate selecting objects to split.</param>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful enumeration.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel enumeration.</param>
        /// <returns>A lazy asynchronous sequence of split objects.</returns>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject?> SplitObjectsAtDistanceAsync(
            this IEnumerable<ITimedObject?> objects,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            Predicate<ITimedObject>? filter = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
            => ToolsAsyncEnumerable.Enumerate(
                () => SplitObjectsAtDistance(objects, ratio, lengthType, from, tempoMap, filter),
                progress,
                cancellationToken);
    }
}
