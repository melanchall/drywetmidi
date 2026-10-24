using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    internal static class ToolsAsyncEnumerable
    {
        private const int BatchSize = 64;

        private sealed class EnumerationProgress : IProgress<AsyncOperationProgress>
        {
            private readonly IProgress<AsyncOperationProgress> _progress;
            private long _processed;
            private long _batchProcessed;
            private long _lastReport;
            private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

            internal EnumerationProgress(IProgress<AsyncOperationProgress> progress)
            {
                _progress = progress;
            }

            public void Report(AsyncOperationProgress value)
            {
                _batchProcessed = value.ProcessedItems;

                if (!value.IsCompleted)
                {
                    _lastReport = _stopwatch.ElapsedMilliseconds;
                    _progress.Report(new AsyncOperationProgress(_processed + _batchProcessed));
                }
            }

            internal void EndBatch()
            {
                _processed += _batchProcessed;
                _batchProcessed = 0;
                var elapsed = _stopwatch.ElapsedMilliseconds;

                if (elapsed - _lastReport >= 100)
                {
                    _lastReport = elapsed;
                    _progress.Report(new AsyncOperationProgress(_processed));
                }
            }

            internal void Complete() => _progress.Report(new AsyncOperationProgress(_processed, true));
        }

        // Advance a persistent iterator in bounded batches without retaining a worker between requests.
        internal static async IAsyncEnumerable<T> Enumerate<T>(
            Func<IEnumerable<T>> source,
            IProgress<AsyncOperationProgress>? progress,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            IEnumerator<T>? enumerator = null;
            ExceptionDispatchInfo? deferredError = null;
            bool completed = false;
            bool finished = false;
            var enumerationProgress = progress == null ? null : new EnumerationProgress(progress);

            try
            {
                while (!completed)
                {
                    var batch = await AsyncOperation.Run(() =>
                    {
                        enumerator ??= source().GetEnumerator();
                        var items = new List<T>(BatchSize);

                        try
                        {
                            while (items.Count < BatchSize)
                            {
                                AsyncOperation.Checkpoint();

                                if (!enumerator.MoveNext())
                                {
                                    completed = true;
                                    break;
                                }

                                items.Add(enumerator.Current);
                            }
                        }
                        catch (Exception exception) when (!(exception is OperationCanceledException))
                        {
                            deferredError = ExceptionDispatchInfo.Capture(exception);
                            completed = true;
                        }

                        return items;
                    },
                    cancellationToken,
                    enumerationProgress).ConfigureAwait(false);
                    enumerationProgress?.EndBatch();

                    foreach (var item in batch)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        yield return item;
                    }

                    deferredError?.Throw();
                }

                finished = true;
            }
            finally
            {
                if (enumerator != null)
                    await AsyncOperation.Run(() => enumerator.Dispose(), CancellationToken.None).ConfigureAwait(false);

                if (finished)
                    enumerationProgress?.Complete();
            }
        }
    }
}
