using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Interaction
{
    internal static class InteractionAsyncUtilities
    {
        internal static IEnumerable<EventsCollection> Collections(EventsCollection eventsCollection)
        {
            ThrowIfArgument.IsNull(nameof(eventsCollection), eventsCollection);
            return new[] { eventsCollection };
        }

        internal static IEnumerable<EventsCollection> Collections(TrackChunk trackChunk)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            return Collections(trackChunk.Events);
        }

        internal static IEnumerable<EventsCollection> Collections(IEnumerable<TrackChunk> trackChunks)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            return trackChunks.Select(t => t.Events);
        }

        internal static IEnumerable<EventsCollection> Collections(MidiFile file)
        {
            ThrowIfArgument.IsNull(nameof(file), file);
            return Collections(file.GetTrackChunks());
        }

        internal static async Task<List<T>> CollectAsync<T>(IAsyncEnumerable<T> source, CancellationToken cancellationToken)
        {
            ThrowIfArgument.IsNull(nameof(source), source);
            var result = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
            {
                AsyncOperation.Checkpoint();
                result.Add(item);
            }
            return result;
        }

        internal static async IAsyncEnumerable<T> EnumerateAsync<T>(
            Func<IEnumerable<T>> source,
            IProgress<AsyncOperationProgress>? progress,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var ready = new SemaphoreSlim(0);
            using var consumed = new SemaphoreSlim(0);
            var completed = false;
            T current = default!;
            var producer = AsyncOperation.Run(async () =>
            {
                try
                {
                    foreach (var item in source())
                    {
                        AsyncOperation.Checkpoint();
                        current = item;
                        ready.Release();
                        await consumed.WaitAsync(lifetime.Token).ConfigureAwait(false);
                    }
                    return true;
                }
                finally
                {
                    completed = true;
                    ready.Release();
                }
            }, lifetime.Token, progress);
            try
            {
                while (true)
                {
                    await ready.WaitAsync(cancellationToken).ConfigureAwait(false);
                    if (completed)
                    {
                        await producer.ConfigureAwait(false);
                        yield break;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    yield return current;
                    consumed.Release();
                }
            }
            finally
            {
                lifetime.Cancel();
                try { await producer.ConfigureAwait(false); }
                catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            }
        }
    }
}
