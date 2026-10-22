using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed partial class TimedObjectsManager
    {
        /// <summary>Asynchronously creates a manager for the specified object types.</summary>
        public static Task<TimedObjectsManager> CreateAsync(EventsCollection eventsCollection, ObjectType objectType, ObjectDetectionSettings? objectDetectionSettings = null, TimedObjectsComparer? comparer = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TimedObjectsManager(eventsCollection, objectType, objectDetectionSettings, comparer), cancellationToken, progress);
    }

    public partial class TimedObjectsManager<TObject> where TObject : ITimedObject
    {
        /// <summary>Asynchronously creates and initializes an objects manager.</summary>
        public static Task<TimedObjectsManager<TObject>> CreateAsync(EventsCollection eventsCollection, ObjectDetectionSettings? objectDetectionSettings = null, TimedObjectsComparer? comparer = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TimedObjectsManager<TObject>(eventsCollection, objectDetectionSettings, comparer), cancellationToken, progress);

        /// <summary>Asynchronously saves managed objects back to the underlying events collection.</summary>
        public Task SaveChangesAsync(IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(SaveChanges, cancellationToken, progress);
    }
}
