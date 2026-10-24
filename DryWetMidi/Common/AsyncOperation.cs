using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Common
{
    internal static class AsyncOperation
    {
        private sealed class Context
        {
            internal readonly CancellationToken CancellationToken;
            internal readonly IProgress<AsyncOperationProgress>? Progress;
            internal readonly Stopwatch? Stopwatch;
            internal long Processed;
            internal long LastReport;

            internal Context(CancellationToken cancellationToken, IProgress<AsyncOperationProgress>? progress)
            {
                CancellationToken = cancellationToken;
                Progress = progress;
                Stopwatch = progress != null ? Stopwatch.StartNew() : null;
            }
        }

        private static readonly AsyncLocal<Context?> Current = new AsyncLocal<Context?>();

        internal static bool IsActive => Current.Value != null;

        internal static Task Run(
            Action action,
            CancellationToken cancellationToken,
            IProgress<AsyncOperationProgress>? progress = null)
        {
            return Run(
                () =>
                {
                    action();

                    return true;
                },
                cancellationToken,
                progress);
        }

        internal static Task<T> Run<T>(
            Func<T> action,
            CancellationToken cancellationToken,
            IProgress<AsyncOperationProgress>? progress = null)
        {
            return Task.Run(
                () =>
                {
                    var previous = Current.Value;
                    var context = cancellationToken.CanBeCanceled || progress != null
                        ? new Context(cancellationToken, progress)
                        : null;
                    Current.Value = context;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var result = action();

                        cancellationToken.ThrowIfCancellationRequested();
                        progress?.Report(new AsyncOperationProgress(context!.Processed, true));

                        return result;
                    }
                    finally
                    {
                        Current.Value = previous;
                    }
                },
                cancellationToken);
        }

        internal static Task<T> Run<T>(
            Func<Task<T>> action,
            CancellationToken cancellationToken,
            IProgress<AsyncOperationProgress>? progress = null)
        {
            return Task.Run(
                async () =>
                {
                    var previous = Current.Value;
                    var context = cancellationToken.CanBeCanceled || progress != null
                        ? new Context(cancellationToken, progress)
                        : null;
                    Current.Value = context;
                    try
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var result = await action().ConfigureAwait(false);

                        cancellationToken.ThrowIfCancellationRequested();
                        progress?.Report(new AsyncOperationProgress(context!.Processed, true));

                        return result;
                    }
                    finally
                    {
                        Current.Value = previous;
                    }
                },
                cancellationToken);
        }

        internal static void Checkpoint(long? processed = null)
        {
            var context = Current.Value;
            if (context == null)
                return;

            context.CancellationToken.ThrowIfCancellationRequested();
            if (context.Progress == null)
                return;

            context.Processed = processed ?? context.Processed + 1;
            var elapsed = context.Stopwatch!.ElapsedMilliseconds;
            if (elapsed - context.LastReport >= 100)
            {
                context.LastReport = elapsed;
                context.Progress.Report(new AsyncOperationProgress(context.Processed));
            }
        }
    }
}
