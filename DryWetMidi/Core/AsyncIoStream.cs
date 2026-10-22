using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    // Custom codecs have synchronous contracts. Run them on a worker, but never perform
    // synchronous transport I/O there. Span conversions use a bounded scratch buffer.
    internal sealed class AsyncIoStream : Stream
    {
        private const int PacketSize = 65536;
        private readonly Stream _stream;
        private CancellationToken _cancellationToken;
        private bool _async;
        private byte[]? _scratch;

        internal AsyncIoStream(Stream stream)
        {
            _stream = stream;
        }

        internal bool IsMemoryStream => _stream is MemoryStream;
        internal bool IsAsync => _async;

        internal async Task<T> RunAsync<T>(Func<T> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _cancellationToken = cancellationToken;
            _async = true;
            try
            {
                return await Task.Run(action, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                _async = false;
                _cancellationToken = default;
            }
        }

        internal Task RunAsync(Action action, CancellationToken cancellationToken)
        {
            return RunAsync(() => { action(); return true; }, cancellationToken);
        }

        internal async Task RunAsyncOperation(Func<Task> action, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _cancellationToken = cancellationToken;
            _async = true;
            try
            {
                await action().ConfigureAwait(false);
            }
            finally
            {
                _async = false;
                _cancellationToken = default;
            }
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;
        public override long Position { get => _stream.Position; set => _stream.Position = value; }
        public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
        public override void SetLength(long value) => _stream.SetLength(value);

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!_async)
                return _stream.Read(buffer, offset, count);
            return _stream.ReadAsync(buffer, offset, Math.Min(count, PacketSize), _cancellationToken)
                .ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public override int Read(Span<byte> buffer)
        {
            if (!_async)
                return _stream.Read(buffer);
            _scratch ??= new byte[PacketSize];
            var count = Read(_scratch, 0, Math.Min(buffer.Length, _scratch.Length));
            _scratch.AsSpan(0, count).CopyTo(buffer);
            return count;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!_async)
            {
                _stream.Write(buffer, offset, count);
                return;
            }
            while (count > 0)
            {
                var size = Math.Min(count, PacketSize);
                _stream.WriteAsync(buffer, offset, size, _cancellationToken)
                    .ConfigureAwait(false).GetAwaiter().GetResult();
                offset += size;
                count -= size;
            }
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!_async)
            {
                _stream.Write(buffer);
                return;
            }
            _scratch ??= new byte[PacketSize];
            while (!buffer.IsEmpty)
            {
                var size = Math.Min(buffer.Length, _scratch.Length);
                buffer.Slice(0, size).CopyTo(_scratch);
                Write(_scratch, 0, size);
                buffer = buffer.Slice(size);
            }
        }

        public override void Flush()
        {
            if (_async)
                _stream.FlushAsync(_cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
            else
                _stream.Flush();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_async)
                    _stream.DisposeAsync().AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                else
                    _stream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
