using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed partial class TempoMapManager
    {
        /// <inheritdoc cref="TempoMapManager(TimeDivision, IEnumerable{EventsCollection})" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously creates and initializes a tempo map manager.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> CreateAsync(
            TimeDivision timeDivision,
            IEnumerable<EventsCollection> eventsCollections,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TempoMapManager(timeDivision, eventsCollections), cancellationToken, progress);

        /// <inheritdoc cref="SaveChanges" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously saves the managed tempo map back to its events collections.
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

        /// <inheritdoc cref="ReplaceTempoMap" path="/*[not(self::summary) and not(self::param[@name='progress' or @name='cancellationToken'])]" />
        /// <summary>
        /// Asynchronously replaces the managed tempo map.
        /// </summary>
        /// <param name="progress">
        /// Optional throttled progress in work checkpoints across phases, not exact note or event counts.
        /// The total is unknown. A final report marks successful completion.
        /// </param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task ReplaceTempoMapAsync(
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ReplaceTempoMap(tempoMap), cancellationToken, progress);
    }
}
