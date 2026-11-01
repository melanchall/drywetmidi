using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Core
{
    internal sealed class AsyncOnlyStream : Stream
    {
        private readonly MemoryStream _innerStream;
        private readonly bool _seekable;
        private readonly int _maximumReadSize;

        internal AsyncOnlyStream(
            byte[] bytes = null,
            bool seekable = true,
            int maximumReadSize = int.MaxValue)
        {
            _innerStream = bytes == null ? new MemoryStream() : new MemoryStream(bytes);
            _seekable = seekable;
            _maximumReadSize = maximumReadSize;
        }

        public override bool CanRead => true;

        public override bool CanSeek => _seekable;

        public override bool CanWrite => true;

        public override long Length => _seekable ? _innerStream.Length : throw new NotSupportedException();

        public override long Position
        {
            get => _seekable ? _innerStream.Position : throw new NotSupportedException();
            set
            {
                if (!_seekable)
                    throw new NotSupportedException();

                _innerStream.Position = value;
            }
        }

        internal int AsyncReadsCount { get; private set; }

        internal int AsyncWritesCount { get; private set; }

        internal int BytesRead { get; private set; }

        internal int MaximumWriteSize { get; private set; }

        internal int ReadDelayMilliseconds { get; set; }

        internal bool IsDisposed { get; private set; }

        internal TaskCompletionSource<bool> ReadGate { get; set; }

        internal TaskCompletionSource<bool> WriteGate { get; set; }

        internal TaskCompletionSource<bool> WriteStarted { get; set; } =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        internal TaskCompletionSource<bool> ReadStarted { get; } =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        public override int Read(byte[] buffer, int offset, int count) => throw new AssertionException("Synchronous read.");

        public override void Write(byte[] buffer, int offset, int count) => throw new AssertionException("Synchronous write.");

        public override void Flush() => throw new AssertionException("Synchronous flush.");

        public override long Seek(long offset, SeekOrigin origin) => _innerStream.Seek(offset, origin);

        public override void SetLength(long value) => _innerStream.SetLength(value);

        public override async Task<int> ReadAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AsyncReadsCount++;
            ReadStarted.TrySetResult(true);
            if (ReadGate != null)
                await ReadGate.Task.WaitAsync(cancellationToken);

            if (ReadDelayMilliseconds > 0)
                await Task.Delay(ReadDelayMilliseconds, cancellationToken);

            await Task.Yield();
            var bytesRead = _innerStream.Read(buffer, offset, Math.Min(count, _maximumReadSize));
            BytesRead += bytesRead;

            return bytesRead;
        }

        public override async Task WriteAsync(
            byte[] buffer,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            AsyncWritesCount++;
            MaximumWriteSize = Math.Max(MaximumWriteSize, count);
            WriteStarted.TrySetResult(true);
            if (WriteGate != null)
                await WriteGate.Task.WaitAsync(cancellationToken);

            await Task.Yield();
            _innerStream.Write(buffer, offset, count);
        }

        public override Task FlushAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        internal byte[] ToArray() => _innerStream.ToArray();

        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            if (disposing)
                _innerStream.Dispose();

            base.Dispose(disposing);
        }
    }
}
