using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tools
{
    // The CSV codec runs against a bounded memory buffer, never the caller's stream.
    // Only the asynchronous pump accesses the caller's stream.
    internal sealed class CsvAsyncBuffer : Stream
    {
        private readonly byte[] _buffer = new byte[8192];
        private readonly SemaphoreSlim _available = new SemaphoreSlim(0);
        private readonly SemaphoreSlim _free = new SemaphoreSlim(1);
        private readonly CancellationToken _token;
        private int _count;
        private int _offset;
        private bool _reading;
        private bool _end;

        private CsvAsyncBuffer(CancellationToken token) => _token = token;

        internal static async Task SerializeAsync(Stream stream, Action<Stream> serialize, IProgress<AsyncOperationProgress>? progress, CancellationToken token)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            if (!stream.CanWrite)
                throw new ArgumentException("Stream must be writable.", nameof(stream));
            token.ThrowIfCancellationRequested();
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
            using var buffer = new CsvAsyncBuffer(lifetime.Token);
            var worker = AsyncOperation.Run(() =>
            {
                try
                {
                    serialize(buffer);
                    buffer.Complete();
                }
                catch { lifetime.Cancel(); throw; }
            }, lifetime.Token, progress);
            var pump = buffer.DrainAsync(stream, lifetime);
            try
            {
                await Task.WhenAll(worker, pump).ConfigureAwait(false);
                await stream.FlushAsync(token).ConfigureAwait(false);
            }
            catch
            {
                token.ThrowIfCancellationRequested();
                if (worker.IsFaulted) await worker.ConfigureAwait(false);
                if (pump.IsFaulted) await pump.ConfigureAwait(false);
                throw;
            }
        }

        internal static async Task<T> DeserializeAsync<T>(Stream stream, Func<Stream, T> deserialize, IProgress<AsyncOperationProgress>? progress, CancellationToken token)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            if (!stream.CanRead)
                throw new ArgumentException("Stream must be readable.", nameof(stream));
            token.ThrowIfCancellationRequested();
            using var lifetime = CancellationTokenSource.CreateLinkedTokenSource(token);
            using var buffer = new CsvAsyncBuffer(lifetime.Token);
            var worker = AsyncOperation.Run(() =>
            {
                try { return deserialize(buffer); }
                catch { lifetime.Cancel(); throw; }
            }, lifetime.Token, progress);
            var pump = buffer.FillAsync(stream, lifetime);
            try
            {
                await Task.WhenAll(worker, pump).ConfigureAwait(false);
                return await worker.ConfigureAwait(false);
            }
            catch
            {
                token.ThrowIfCancellationRequested();
                if (worker.IsFaulted) await worker.ConfigureAwait(false);
                if (pump.IsFaulted) await pump.ConfigureAwait(false);
                if (worker.Status == TaskStatus.RanToCompletion)
                    return worker.Result;
                throw;
            }
        }

        private async Task DrainAsync(Stream stream, CancellationTokenSource lifetime)
        {
            try
            {
                while (true)
                {
                    await _available.WaitAsync(_token).ConfigureAwait(false);
                    if (_count == 0)
                        return;
                    await stream.WriteAsync(_buffer, 0, _count, _token).ConfigureAwait(false);
                    _free.Release();
                }
            }
            catch { lifetime.Cancel(); throw; }
        }

        private async Task FillAsync(Stream stream, CancellationTokenSource lifetime)
        {
            try
            {
                while (true)
                {
                    await _free.WaitAsync(_token).ConfigureAwait(false);
                    _count = await stream.ReadAsync(_buffer, 0, _buffer.Length, _token).ConfigureAwait(false);
                    _available.Release();
                    if (_count == 0)
                        return;
                }
            }
            catch { lifetime.Cancel(); throw; }
        }

        private void Complete()
        {
            _free.Wait(_token);
            _count = 0;
            _available.Release();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _token.ThrowIfCancellationRequested();
            if (count == 0 || _end)
                return 0;
            if (!_reading)
            {
                _available.Wait(_token);
                _offset = 0;
                _reading = true;
                if (_count == 0)
                {
                    _end = true;
                    return 0;
                }
            }
            var length = Math.Min(count, _count - _offset);
            Buffer.BlockCopy(_buffer, _offset, buffer, offset, length);
            _offset += length;
            if (_offset == _count)
            {
                _reading = false;
                _free.Release();
            }
            return length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                _free.Wait(_token);
                var length = Math.Min(count, _buffer.Length);
                Buffer.BlockCopy(buffer, offset, _buffer, 0, length);
                _count = length;
                _available.Release();
                offset += length;
                count -= length;
            }
        }

        public override void Flush() => _token.ThrowIfCancellationRequested();
        public override bool CanRead => true;
        public override bool CanWrite => true;
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _available.Dispose();
                _free.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
