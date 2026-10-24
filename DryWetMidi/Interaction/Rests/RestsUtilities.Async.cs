using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class RestsUtilities
    {
        /// <inheritdoc cref="GetRests(IEnumerable{ITimedObject}, RestDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects rests between timed objects.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Rest>> GetRestsAsync(
            this IEnumerable<ITimedObject> timedObjects,
            RestDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => timedObjects.GetRests(settings), cancellationToken, progress);

        /// <inheritdoc cref="GetRests(IEnumerable{ITimedObject}, RestDetectionSettings)" />
        /// <summary>
        /// Asynchronously detects rests from an asynchronous timed object source.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<ICollection<Rest>> GetRestsAsync(
            this IAsyncEnumerable<ITimedObject> timedObjects,
            RestDetectionSettings? settings = null,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(async () => (await InteractionAsyncUtilities.CollectAsync(timedObjects, cancellationToken).ConfigureAwait(false)).GetRests(settings), cancellationToken, progress);
    }
}
