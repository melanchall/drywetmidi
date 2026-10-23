using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    public sealed partial class TimedObjectsWriter
    {
        private int _asyncOperation;

        private sealed class WritingProgress
        {
            private readonly IProgress<AsyncOperationProgress>? _progress;
            private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
            private long _lastReport;
            private long _count;

            internal WritingProgress(IProgress<AsyncOperationProgress>? progress)
            {
                _progress = progress;
            }

            internal void Advance()
            {
                _count++;

                var elapsed = _stopwatch.ElapsedMilliseconds;
                if (_progress != null && elapsed - _lastReport >= 100)
                {
                    _lastReport = elapsed;
                    _progress.Report(new AsyncOperationProgress(_count, phase: "Writing objects"));
                }
            }

            internal void Complete()
            {
                _progress?.Report(new AsyncOperationProgress(_count, true, "Writing objects"));
            }
        }

        /// <inheritdoc cref="WriteObject" />
        /// <summary>
        /// Writes an object using asynchronous output. Objects must be supplied in
        /// non-descending time order. Cancellation can leave partially written output.
        /// </summary>
        /// <param name="timedObject">Object to write.</param>
        /// <param name="cancellationToken">Token used to cancel output.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timedObject"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">This writer or its underlying token writer is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another asynchronous operation is in progress.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async ValueTask WriteObjectAsync(
            ITimedObject timedObject,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(timedObject), timedObject);

            EnterAsyncOperation();

            try
            {
                await WriteObjectCoreAsync(timedObject, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                Volatile.Write(ref _asyncOperation, 0);
            }
        }

        /// <inheritdoc cref="WriteObjects" />
        /// <summary>
        /// Writes a sequence in non-descending time order using asynchronous output.
        /// The supplied token writer remains owned by the caller.
        /// </summary>
        /// <param name="timedObjects">Objects to write; null elements are ignored.</param>
        /// <param name="progress">Optional throttled progress of accepted objects, excluding final token-writer completion.</param>
        /// <param name="cancellationToken">Token used to cancel enumeration and output.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timedObjects"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">This writer or its underlying token writer is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another asynchronous operation is in progress.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task WriteObjectsAsync(
            IEnumerable<ITimedObject?> timedObjects,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            EnterAsyncOperation();

            try
            {
                var reporter = new WritingProgress(progress);
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var timedObject in timedObjects)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (timedObject != null)
                    {
                        await WriteObjectCoreAsync(timedObject, cancellationToken).ConfigureAwait(false);
                        reporter.Advance();
                    }

                }
                cancellationToken.ThrowIfCancellationRequested();
                reporter.Complete();
            }
            finally
            {
                Volatile.Write(ref _asyncOperation, 0);
            }
        }

        /// <inheritdoc cref="WriteObjects" />
        /// <summary>
        /// Writes asynchronously produced objects in non-descending time order.
        /// Cancellation or producer failure can leave partially written output.
        /// </summary>
        /// <param name="timedObjects">Objects to write; null elements are ignored.</param>
        /// <param name="progress">Optional throttled progress of accepted objects, excluding final token-writer completion.</param>
        /// <param name="cancellationToken">Token used to cancel enumeration and output.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timedObjects"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">This writer or its underlying token writer is disposed.</exception>
        /// <exception cref="InvalidOperationException">Another asynchronous operation is in progress.</exception>
        /// <exception cref="OperationCanceledException">The operation was canceled.</exception>
        public async Task WriteObjectsAsync(
            IAsyncEnumerable<ITimedObject?> timedObjects,
            IProgress<AsyncOperationProgress>? progress = null,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            EnterAsyncOperation();

            try
            {
                var reporter = new WritingProgress(progress);
                cancellationToken.ThrowIfCancellationRequested();

                await foreach (var timedObject in timedObjects.WithCancellation(cancellationToken).ConfigureAwait(false))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (timedObject != null)
                    {
                        await WriteObjectCoreAsync(timedObject, cancellationToken).ConfigureAwait(false);
                        reporter.Advance();
                    }

                }
                cancellationToken.ThrowIfCancellationRequested();
                reporter.Complete();
            }
            finally
            {
                Volatile.Write(ref _asyncOperation, 0);
            }
        }

        /// <summary>
        /// Emits deferred events asynchronously without disposing the supplied token writer.
        /// The token writer must subsequently be completed or disposed by its owner.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The underlying token writer is disposed while deferred events remain.</exception>
        /// <exception cref="InvalidOperationException">
        /// Another asynchronous operation is in progress, or the underlying token writer has no open track.
        /// </exception>
        /// <exception cref="System.IO.IOException">An I/O error occurred while writing deferred events.</exception>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            EnterAsyncOperation();

            try
            {
                await FlushCoreAsync(CancellationToken.None).ConfigureAwait(false);
                _disposed = true;
            }
            finally
            {
                Volatile.Write(ref _asyncOperation, 0);
            }
        }

        private async ValueTask WriteObjectCoreAsync(
            ITimedObject timedObject,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var time = timedObject.Time;
            if (time < _tokensWriter.CurrentTime)
                throw new InvalidOperationException("Object's time is less than the current time of the underlying writer.");

            while (_timedEvents.First != null && _timedEvents.First.Value.Time <= time)
            {
                await WriteTimedEventAsync(_timedEvents.First.Value, cancellationToken).ConfigureAwait(false);
                _timedEvents.RemoveFirst();
            }

            var events = GetObjectTimedEvents(timedObject);
            var index = 0;

            while (index < events.Length && events[index].Time <= time)
            {
                await WriteTimedEventAsync(events[index], cancellationToken).ConfigureAwait(false);
                index++;
            }

            if (index < events.Length)
                SaveTimedEvents(events, index);
        }

        private async ValueTask WriteTimedEventAsync(
            TimedEvent timedEvent,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var midiEvent = timedEvent.Event;
            midiEvent.DeltaTime = timedEvent.Time - _tokensWriter.CurrentTime;
            await _tokensWriter.WriteEventAsync(midiEvent, cancellationToken).ConfigureAwait(false);
        }

        private async ValueTask FlushPendingAsync(CancellationToken cancellationToken)
        {
            if (_disposed)
                return;

            EnterAsyncOperation();

            try
            {
                await FlushCoreAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                Volatile.Write(ref _asyncOperation, 0);
            }
        }

        private async ValueTask FlushCoreAsync(CancellationToken cancellationToken)
        {
            while (_timedEvents.First != null)
            {
                await WriteTimedEventAsync(_timedEvents.First.Value, cancellationToken).ConfigureAwait(false);
                _timedEvents.RemoveFirst();
            }
        }

        private void EnterAsyncOperation()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(TimedObjectsWriter));

            if (Interlocked.CompareExchange(ref _asyncOperation, 1, 0) != 0)
                throw new InvalidOperationException("Another asynchronous operation is already in progress.");
        }

        private void EnsureNoAsyncOperation()
        {
            if (Volatile.Read(ref _asyncOperation) != 0)
                throw new InvalidOperationException("An asynchronous operation is already in progress.");
        }
    }
}
