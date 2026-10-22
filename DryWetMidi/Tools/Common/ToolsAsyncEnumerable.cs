using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class ToolsAsyncEnumerable
    {
        // A single worker advances the synchronous iterator only when the consumer asks for an item.
        internal static async IAsyncEnumerable<T> Enumerate<T>(
            Func<IEnumerable<T>> source,
            IProgress<AsyncOperationProgress>? progress,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var requested = new SemaphoreSlim(0);
            using var available = new SemaphoreSlim(0);
            T current = default!;
            bool hasValue = false;
            ExceptionDispatchInfo? error = null;
            var worker = AsyncOperation.Run(() =>
            {
                try
                {
                    using var enumerator = source().GetEnumerator();
                    while (true)
                    {
                        requested.Wait(lifetime.Token);
                        AsyncOperation.Checkpoint();
                        hasValue = enumerator.MoveNext();
                        if (hasValue)
                            current = enumerator.Current;
                        available.Release();
                        if (!hasValue)
                            break;
                    }
                }
                catch (Exception exception)
                {
                    error = ExceptionDispatchInfo.Capture(exception);
                    available.Release();
                }
            }, lifetime.Token, progress);

            try
            {
                while (true)
                {
                    requested.Release();
                    await available.WaitAsync(cancellationToken).ConfigureAwait(false);
                    error?.Throw();
                    if (!hasValue)
                        yield break;
                    yield return current;
                }
            }
            finally
            {
                lifetime.Cancel();
                try { await worker.ConfigureAwait(false); }
                catch (OperationCanceledException) when (lifetime.IsCancellationRequested) { }
            }
        }
    }
}
