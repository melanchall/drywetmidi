using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiWriter
    {
        internal MidiIoProgress? AsyncProgress { get; set; }

        internal async Task WriteEncodedAsync(Action<MidiWriter> encode, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using var memory = new MemoryStream();
            using (var codec = new MidiWriter(memory, new WriterSettings { UseBuffering = false }))
            {
                codec._length = _length;
                encode(codec);
            }
            await WritePayloadAsync(memory.GetBuffer(), checked((int)memory.Length), cancellationToken).ConfigureAwait(false);
        }

        internal async Task WriteDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await WritePayloadAsync(data, data.Length, cancellationToken).ConfigureAwait(false);
        }

        private async Task WritePayloadAsync(byte[] data, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!_useBuffering)
                await WriteTransportAsync(data, count, cancellationToken).ConfigureAwait(false);
            else
            {
                for (var offset = 0; offset < count;)
                {
                    var size = Math.Min(count - offset, _buffer!.Length - _bufferPosition);
                    Buffer.BlockCopy(data, offset, _buffer, _bufferPosition, size);
                    _bufferPosition += size;
                    offset += size;
                    if (_bufferPosition == _buffer.Length)
                        await FlushBufferAsync(cancellationToken).ConfigureAwait(false);
                }
            }
            _length += count;
        }

        private async Task WriteTransportAsync(byte[] data, int count, CancellationToken cancellationToken)
        {
            for (var offset = 0; offset < count;)
            {
                var size = Math.Min(65536, count - offset);
                await _stream.WriteAsync(data, offset, size, cancellationToken).ConfigureAwait(false);
                AsyncProgress?.Add(size);
                offset += size;
            }
        }

        private async Task FlushBufferAsync(CancellationToken cancellationToken)
        {
            if (_bufferPosition == 0)
                return;
            await WriteTransportAsync(_buffer!, _bufferPosition, cancellationToken).ConfigureAwait(false);
            _bufferPosition = 0;
        }

        /// <summary>Asynchronously flushes buffered MIDI bytes without closing the underlying stream.</summary>
        /// <returns>A task representing disposal.</returns>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
            try
            {
                await FlushBufferAsync(CancellationToken.None).ConfigureAwait(false);
                await _stream.FlushAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally { _disposed = true; }
        }
    }
}
