using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class RestsUtilities
    {
        /// <summary>Asynchronously detects rests between timed objects.</summary>
        public static Task<ICollection<Rest>> GetRestsAsync(this IEnumerable<ITimedObject> timedObjects, RestDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => timedObjects.GetRests(settings), cancellationToken, progress);

        /// <summary>Asynchronously detects rests from an asynchronous timed object source.</summary>
        public static Task<ICollection<Rest>> GetRestsAsync(this IAsyncEnumerable<ITimedObject> timedObjects, RestDetectionSettings? settings = null, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(timedObjects, cancellationToken).ConfigureAwait(false)).GetRests(settings), cancellationToken, progress);
    }
}
