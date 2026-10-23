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
        /// <inheritdoc cref="TempoMapManager(TimeDivision, IEnumerable{EventsCollection})" />
        /// <summary>
        /// Asynchronously creates and initializes a tempo map manager.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public static Task<TempoMapManager> CreateAsync(
            TimeDivision timeDivision,
            IEnumerable<EventsCollection> eventsCollections,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TempoMapManager(timeDivision, eventsCollections), cancellationToken, progress);

        /// <inheritdoc cref="SaveChanges" />
        /// <summary>
        /// Asynchronously saves the managed tempo map back to its events collections.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task SaveChangesAsync(
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(SaveChanges, cancellationToken, progress);

        /// <inheritdoc cref="ReplaceTempoMap" />
        /// <summary>
        /// Asynchronously replaces the managed tempo map.
        /// </summary>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public Task ReplaceTempoMapAsync(
            TempoMap tempoMap,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ReplaceTempoMap(tempoMap), cancellationToken, progress);
    }
}
