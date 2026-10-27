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

        /// <summary>
        /// Asynchronously determines whether the end of the underlying stream has been reached.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>Whether the end of the stream has been reached.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The reader or underlying stream has been disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred while reading.</exception>
        public async ValueTask<bool> EndReachedAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return Position >= Length || (_isStreamWrapped &&
                await ((StreamWrapper)_stream).IsEndReachedAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Asynchronously changes the position, reading skipped bytes when a non-seekable stream requires it.
        /// </summary>
        /// <param name="position">The new position within the underlying stream.</param>
        /// <param name="cancellationToken">Token used to cancel repositioning.</param>
        /// <returns>A task representing repositioning.</returns>
        /// <exception cref="OperationCanceledException">Repositioning was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The reader or underlying stream has been disposed.</exception>
        /// <exception cref="IOException">The position cannot be reached or an I/O error occurred.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is negative.</exception>
        /// <remarks>Backward movement on non-seekable streams is limited by the reader's history buffer.</remarks>
        public async ValueTask SetPositionAsync(long position, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNegative(nameof(position), position, "Position is negative.");
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (!_useBuffering && _isStreamWrapped)
                await ((StreamWrapper)_stream).SetPositionAsync(position, cancellationToken).ConfigureAwait(false);
            else
                Position = position;
        }

        /// <summary>
        /// Asynchronously reads a byte and advances the position by one.
        /// </summary>
        /// <inheritdoc cref="ReadByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the next byte.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<byte> ReadByteAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
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

        /// <summary>
        /// Asynchronously reads up to the specified number of bytes.
        /// </summary>
        /// <inheritdoc cref="ReadBytes" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="count">Maximum number of bytes to read; must be non-negative.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the bytes read, possibly fewer than requested at the end of the stream.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async Task<byte[]> ReadBytesAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNegative(nameof(count), count, "Count is negative.");
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

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

        /// <summary>
        /// Asynchronously reads a DWORD (32-bit unsigned integer).
        /// </summary>
        /// <inheritdoc cref="ReadDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the DWORD value.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<uint> ReadDwordAsync(CancellationToken cancellationToken = default)
        {
            var bytes = await ReadBytesAsync(4, cancellationToken).ConfigureAwait(false);
            if (bytes.Length < 4)
                throw new NotEnoughBytesException("Not enough bytes to read a DWORD.", 4, bytes.Length);

            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        /// <summary>
        /// Asynchronously reads a signed byte.
        /// </summary>
        /// <inheritdoc cref="ReadSByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the signed byte.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<sbyte> ReadSByteAsync(CancellationToken cancellationToken = default)
        {
            return (sbyte)await ReadByteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a WORD (16-bit unsigned integer).
        /// </summary>
        /// <inheritdoc cref="ReadWord" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the WORD value.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<ushort> ReadWordAsync(CancellationToken cancellationToken = default)
        {
            var bytes = await ReadNumberBytesAsync(2, cancellationToken).ConfigureAwait(false);
            return (ushort)((bytes[0] << 8) | bytes[1]);
        }

        /// <summary>
        /// Asynchronously reads an INT16 (16-bit signed integer).
        /// </summary>
        /// <inheritdoc cref="ReadInt16" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the INT16 value.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<short> ReadInt16Async(CancellationToken cancellationToken = default)
        {
            return (short)await ReadWordAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a DWORD represented by three bytes.
        /// </summary>
        /// <inheritdoc cref="Read3ByteDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the three-byte DWORD value.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<uint> Read3ByteDwordAsync(CancellationToken cancellationToken = default)
        {
            var bytes = await ReadNumberBytesAsync(3, cancellationToken).ConfigureAwait(false);
            return ((uint)bytes[0] << 16) | ((uint)bytes[1] << 8) | bytes[2];
        }

        /// <summary>
        /// Asynchronously reads a string encoded using the MIDI ASCII encoding.
        /// </summary>
        /// <inheritdoc cref="ReadString" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="count">Number of ASCII bytes to read; must be non-negative.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the decoded string.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async Task<string> ReadStringAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNegative(nameof(count), count, "Count is negative.");

            var bytes = await ReadBytesAsync(count, cancellationToken).ConfigureAwait(false);
            return SmfConstants.DefaultTextEncoding.GetString(bytes);
        }

        /// <summary>
        /// Asynchronously reads a variable-length 32-bit integer.
        /// </summary>
        /// <inheritdoc cref="ReadVlqNumber" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the decoded 32-bit integer.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        /// <exception cref="VlqNumberOverflowException">The encoded value is too large.</exception>
        public async ValueTask<int> ReadVlqNumberAsync(CancellationToken cancellationToken = default)
        {
            return (int)await ReadVlqNumberCoreAsync(
                VlqIntMaxBytes,
                VlqIntLastCheckReference,
                cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a variable-length 64-bit integer.
        /// </summary>
        /// <inheritdoc cref="ReadVlqLongNumber" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the decoded 64-bit integer.</returns>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        /// <exception cref="VlqNumberOverflowException">The encoded value is too large.</exception>
        public ValueTask<long> ReadVlqLongNumberAsync(CancellationToken cancellationToken = default)
        {
            return ReadVlqNumberCoreAsync(VlqLongMaxBytes, VlqLongLastCheckReference, cancellationToken);
        }

        private async ValueTask<byte[]> ReadNumberBytesAsync(int count, CancellationToken cancellationToken)
        {
            var bytes = await ReadBytesAsync(count, cancellationToken).ConfigureAwait(false);
            if (bytes.Length != count)
                throw new NotEnoughBytesException("Not enough bytes to read a number.", count, bytes.Length);

            return bytes;
        }

        private async ValueTask<long> ReadVlqNumberCoreAsync(
            int maxBytes,
            long lastCheckReference,
            CancellationToken cancellationToken)
        {
            long value = 0;

            for (var count = 1; ; count++)
            {
                byte b;
                try
                {
                    b = await ReadByteAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (EndOfStreamException exception)
                {
                    throw new NotEnoughBytesException(
                        "Not enough bytes in the stream to read a variable-length quantity number.",
                        exception);
                }

                if (count == maxBytes && (b >= 128 || value > lastCheckReference))
                    throw new VlqNumberOverflowException("VLQ number is too large.");

                value = (value << 7) + (b & 127);
                if (b < 128)
                    return value;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiReader));
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
