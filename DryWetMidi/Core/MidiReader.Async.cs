using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiReader
    {
        private readonly byte[] _asyncByte = new byte[1];
        private MidiIoProgress? _asyncProgress;

        internal void SetProgress(MidiIoProgress? progress)
        {
            _asyncProgress = progress;
            if (_isStreamWrapped)
                ((StreamWrapper)_stream).AsyncBytesRead = progress == null ? null : count => progress.Add(count);
        }

        private async Task<int> ReadStreamAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            var read = await _stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            if (!_isStreamWrapped)
                _asyncProgress?.Add(read);

            return read;
        }

        internal async ValueTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_useBuffering && _buffer == null)
            {
                using var memory = new MemoryStream();
                var packet = new byte[65536];
                int count;

                while ((count = await ReadStreamAsync(packet, 0, packet.Length, cancellationToken).ConfigureAwait(false)) > 0)
                {
                    memory.Write(packet, 0, count);
                }

                _buffer = memory.ToArray();
                _bufferStart = 0;
                _bufferSize = _buffer.Length;
            }
        }

        internal async ValueTask<bool> EndReachedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Position >= Length || (_isStreamWrapped &&
                await ((StreamWrapper)_stream).IsEndReachedAsync(cancellationToken).ConfigureAwait(false));
        }

        internal async ValueTask SetPositionAsync(long position, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_useBuffering && _isStreamWrapped)
                await ((StreamWrapper)_stream).SetPositionAsync(position, cancellationToken).ConfigureAwait(false);
            else
                Position = position;
        }

        internal async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_useBuffering)
            {
                if (!await EnsureBufferAsync(cancellationToken).ConfigureAwait(false))
                    throw new EndOfStreamException();

                var value = _buffer![_bufferPosition];
                Position++;

                return value;
            }

            if (await ReadStreamAsync(_asyncByte, 0, 1, cancellationToken).ConfigureAwait(false) == 0)
                throw new EndOfStreamException();

            return _asyncByte[0];
        }

        internal async Task<byte[]> ReadBytesAsync(int count, CancellationToken cancellationToken)
        {
            if (!_isStreamWrapped)
                count = (int)Math.Min(count, Math.Max(0, Length - Position));

            using var memory = new MemoryStream(Math.Min(count, 4096));
            await CopyBytesAsync(memory, count, cancellationToken).ConfigureAwait(false);

            return memory.ToArray();
        }

        internal async Task CopyBytesAsync(MemoryStream destination, long count, CancellationToken cancellationToken)
        {
            var packet = new byte[(int)Math.Min(count, 65536)];

            while (count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var wanted = (int)Math.Min(count, packet.Length);
                var read = 0;
                if (_useBuffering)
                {
                    if (!await EnsureBufferAsync(cancellationToken).ConfigureAwait(false))
                        break;

                    read = Math.Min(wanted, _bufferSize - _bufferPosition);
                    Buffer.BlockCopy(_buffer!, _bufferPosition, packet, 0, read);
                    Position += read;
                }
                else
                    read = await ReadStreamAsync(packet, 0, wanted, cancellationToken).ConfigureAwait(false);

                if (read == 0)
                    break;

                destination.Write(packet, 0, read);
                count -= read;
            }
        }

        internal async ValueTask<uint> ReadDwordAsync(CancellationToken cancellationToken)
        {
            var bytes = await ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
            if (bytes.Length < 4)
                throw new NotEnoughBytesException("Not enough bytes to read a DWORD.", 4, bytes.Length);

            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        private async ValueTask<bool> EnsureBufferAsync(CancellationToken cancellationToken)
        {
            if (await EndReachedAsync(cancellationToken).ConfigureAwait(false))
                return false;

            if (_position < _bufferStart || _position >= _bufferStart + _bufferSize)
            {
                _stream.Position = (_position / _buffer!.Length) * _buffer.Length;
                _bufferStart = _stream.Position;
                var count = 0;

                while (count < _buffer.Length)
                {
                    var read = await ReadStreamAsync(_buffer, count, _buffer.Length - count, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                        break;

                    count += read;
                }

                _bufferPosition = (int)(_position % _buffer.Length);
                _bufferSize = count;
                if (count == 0)
                    return false;
            }

            return _bufferSize > 0;
        }
    }
}
