using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class GetObjectsUtilities
    {
        /// <inheritdoc cref="GetObjects(IEnumerable{MidiEvent}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects in MIDI events.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this IEnumerable<MidiEvent?> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiEvents.GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(EventsCollection, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects in an events collection.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => eventsCollection.GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(TrackChunk, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects in a track.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunk.GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(IEnumerable{TrackChunk}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects in tracks.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this IEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => trackChunks.GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(MidiFile, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects in a MIDI file.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this MidiFile midiFile,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => midiFile.GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(IEnumerable{ITimedObject}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects in timed objects.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this IEnumerable<ITimedObject?> timedObjects,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => timedObjects.GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(IEnumerable{MidiEvent}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects from an asynchronous MIDI event source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this IAsyncEnumerable<MidiEvent?> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(midiEvents, cancellationToken).ConfigureAwait(false)).GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(IEnumerable{ITimedObject}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects from an asynchronous timed object source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this IAsyncEnumerable<ITimedObject?> timedObjects,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(timedObjects, cancellationToken).ConfigureAwait(false)).GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="GetObjects(IEnumerable{TrackChunk}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects objects from an asynchronous track source.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<ITimedObject>> GetObjectsAsync(
            this IAsyncEnumerable<TrackChunk?> trackChunks,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(trackChunks, cancellationToken).ConfigureAwait(false)).GetObjects(objectType, settings), cancellationToken, progress);

        /// <inheritdoc cref="EnumerateObjects(IEnumerable{MidiEvent}, ObjectType, ObjectDetectionSettings)" />
        /// <summary>
        /// Lazily detects objects on a worker and yields them asynchronously, without materializing
        /// the result. Disposing the enumerator cancels detection and disposes the source enumerator.
        /// </summary>
        /// <exception cref="OperationCanceledException">Enumeration was canceled.</exception>
        public static IAsyncEnumerable<ITimedObject> EnumerateObjectsAsync(
            this IEnumerable<MidiEvent?> midiEvents,
            ObjectType objectType,
            ObjectDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            InteractionAsyncUtilities.EnumerateAsync(() => midiEvents.EnumerateObjects(objectType, settings), progress, cancellationToken);
    }
}
