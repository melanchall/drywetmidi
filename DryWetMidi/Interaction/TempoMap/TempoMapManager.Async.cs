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
        /// <summary>Asynchronously creates and initializes a tempo map manager.</summary>
        public static Task<TempoMapManager> CreateAsync(TimeDivision timeDivision, IEnumerable<EventsCollection> eventsCollections, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => new TempoMapManager(timeDivision, eventsCollections), cancellationToken, progress);

        /// <summary>Asynchronously saves the managed tempo map back to its events collections.</summary>
        public Task SaveChangesAsync(IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(SaveChanges, cancellationToken, progress);

        /// <summary>Asynchronously replaces the managed tempo map.</summary>
        public Task ReplaceTempoMapAsync(TempoMap tempoMap, IProgress<AsyncOperationProgress>? progress = null, CancellationToken cancellationToken = default) =>
            AsyncOperation.Run(() => ReplaceTempoMap(tempoMap), cancellationToken, progress);
    }
}
