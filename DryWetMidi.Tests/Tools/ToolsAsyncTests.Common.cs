using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class ToolsAsyncTests
    {
        private sealed class AsyncOnlyStream : Stream
        {
            private readonly MemoryStream _storage;
            internal int Reads;
            internal int Writes;
            internal int Flushes;
            internal bool FailRead;
            internal bool FailWrite;
            internal bool BlockRead;
            internal bool BlockWrite;
            internal bool CompleteSynchronously;
            internal Action OnRead;
            internal Action OnWrite;
            internal bool Disposed;

            internal AsyncOnlyStream(byte[] bytes = null)
            {
                _storage = bytes == null ? new MemoryStream() : new MemoryStream(bytes);
            }

            internal byte[] ToArray() => _storage.ToArray();

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush() => throw new InvalidOperationException("Synchronous flush.");

            public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous read.");

            public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous write.");

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override async Task<int> ReadAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken)
            {
                Reads++;
                OnRead?.Invoke();

                if (!CompleteSynchronously)
                    await Task.Yield();

                if (BlockRead)
                    await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (FailRead)
                    throw new IOException("Read failure.");

                return await _storage.ReadAsync(buffer, offset, Math.Min(count, 17), cancellationToken).ConfigureAwait(false);
            }

            public override async Task WriteAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken)
            {
                Writes++;

                if (!CompleteSynchronously)
                    await Task.Yield();

                OnWrite?.Invoke();
                if (BlockWrite)
                    await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (FailWrite)
                    throw new IOException("Write failure.");

                await _storage.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                Flushes++;
                cancellationToken.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;
                if (disposing)
                    _storage.Dispose();

                base.Dispose(disposing);
            }
        }

        private sealed class InlineProgress : IProgress<AsyncOperationProgress>
        {
            internal readonly List<AsyncOperationProgress> Reports = new List<AsyncOperationProgress>();

            public void Report(AsyncOperationProgress value) => Reports.Add(value);
        }

        private static MidiFile CreateFile() => new MidiFile(new TrackChunk(
            new SequenceTrackNameEvent("quoted \"track\"\nsecond line"),
            new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100),
            new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)64) { DeltaTime = 100 },
            new NoteOnEvent((SevenBitNumber)62, (SevenBitNumber)90) { DeltaTime = 20 },
            new NoteOffEvent((SevenBitNumber)62, (SevenBitNumber)64) { DeltaTime = 100 }));

        internal static void AssertAsyncException(Action action, Type exceptionType)
        {
            var aggregate = Assert.Throws<AggregateException>(() => action(), "Async operation did not throw an aggregate exception.");

            ClassicAssert.AreEqual(exceptionType, aggregate.InnerException.GetType(), "Async exception type differs.");
        }
    }
}
