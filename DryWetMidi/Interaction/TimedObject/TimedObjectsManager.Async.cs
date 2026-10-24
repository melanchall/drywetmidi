using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed partial class TimedObjectsManager
    {
        /// <inheritdoc cref="TimedObjectsManager(EventsCollection, ObjectType, ObjectDetectionSettings, TimedObjectsComparer)" />
        /// <summary>
        /// Asynchronously creates a manager for the specified object types.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager> CreateAsync(
            EventsCollection eventsCollection,
            ObjectType objectType,
            ObjectDetectionSettings? objectDetectionSettings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TimedObjectsManager(eventsCollection, objectType, objectDetectionSettings, comparer), cancellationToken, progress);
    }

    public partial class TimedObjectsManager<TObject> where TObject : ITimedObject
    {
        /// <inheritdoc cref="TimedObjectsManager{TObject}(EventsCollection, ObjectDetectionSettings, TimedObjectsComparer)" />
        /// <summary>
        /// Asynchronously creates and initializes an objects manager.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// <typeparamref name="TObject"/> is not <see cref="Note"/>, <see cref="Chord"/>,
        /// or <see cref="TimedEvent"/>.
        /// </exception>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TimedObjectsManager<TObject>> CreateAsync(
            EventsCollection eventsCollection,
            ObjectDetectionSettings? objectDetectionSettings = null,
            TimedObjectsComparer? comparer = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TimedObjectsManager<TObject>(eventsCollection, objectDetectionSettings, comparer), cancellationToken, progress);

        /// <inheritdoc cref="SaveChanges" />
        /// <summary>
        /// Asynchronously saves managed objects back to the underlying events collection.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task SaveChangesAsync(
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(SaveChanges, cancellationToken, progress);
    }
}
