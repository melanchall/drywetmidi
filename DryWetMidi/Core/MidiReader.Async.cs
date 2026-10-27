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
        /// Asynchronously determines whether the end of the underlying stream is reached.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel checking the stream.</param>
        /// <returns>A task containing <c>true</c> if the end of the stream is reached; otherwise, <c>false</c>.</returns>
        /// <remarks>Checking a non-seekable stream can require an asynchronous read into its replay buffer.</remarks>
        /// <inheritdoc cref="EndReached" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Checking the stream was canceled.</exception>
        public async ValueTask<bool> EndReachedAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return Position >= Length || (_isStreamWrapped &&
                await ((StreamWrapper)_stream).IsEndReachedAsync(cancellationToken).ConfigureAwait(false));
        }

        /// <summary>
        /// Sets the reader's position, asynchronously consuming bytes when moving forward on a non-seekable stream.
        /// </summary>
        /// <param name="position">Non-negative absolute position to move to.</param>
        /// <param name="cancellationToken">Token used to cancel moving the position.</param>
        /// <returns>A task representing moving the position.</returns>
        /// <remarks>
        /// Moving backward on a non-seekable stream requires the requested bytes to remain in its replay buffer.
        /// Moving forward can stop at the end of a non-seekable stream before the requested position.
        /// Cancellation or failure can leave the position partially advanced. The reader and its stream remain open.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="position"/> is negative.</exception>
        /// <exception cref="InvalidOperationException">The requested backward position is outside the non-seekable stream's replay buffer.</exception>
        /// <exception cref="ObjectDisposedException">The reader or its underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="OperationCanceledException">Moving the position was canceled.</exception>
        public async ValueTask SetPositionAsync(long position, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNegative(nameof(position), position, "Position is negative.");
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (!_useBuffering && _isStreamWrapped)
            {
                if (position < Position && Position - position > int.MaxValue)
                    throw new InvalidOperationException("Failed to move position back beyond the start of the buffer.");

                await ((StreamWrapper)_stream).SetPositionAsync(position, cancellationToken).ConfigureAwait(false);
            }
            else
                Position = position;
        }

        /// <summary>
        /// Asynchronously reads one byte and advances the current position.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the next byte.</returns>
        /// <inheritdoc cref="ReadByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
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
        /// Asynchronously reads up to the requested number of bytes and advances the current position.
        /// </summary>
        /// <param name="count">Non-negative number of bytes to read.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the bytes read, possibly fewer than requested at the end of the stream.</returns>
        /// <inheritdoc cref="ReadBytes" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
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
        /// Asynchronously reads a big-endian 32-bit unsigned integer.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the integer read.</returns>
        /// <inheritdoc cref="ReadDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
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
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the signed byte read.</returns>
        /// <inheritdoc cref="ReadSByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<sbyte> ReadSByteAsync(CancellationToken cancellationToken = default)
        {
            return (sbyte)await ReadByteAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a big-endian 16-bit unsigned integer.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the integer read.</returns>
        /// <inheritdoc cref="ReadWord" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<ushort> ReadWordAsync(CancellationToken cancellationToken = default)
        {
            return (ushort)await ReadNumberAsync(2, "WORD", cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a big-endian 16-bit signed integer.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the integer read.</returns>
        /// <inheritdoc cref="ReadInt16" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<short> ReadInt16Async(CancellationToken cancellationToken = default)
        {
            return (short)await ReadNumberAsync(2, "INT16", cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a big-endian unsigned integer stored in three bytes.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the integer read.</returns>
        /// <inheritdoc cref="Read3ByteDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public ValueTask<uint> Read3ByteDwordAsync(CancellationToken cancellationToken = default)
        {
            return ReadNumberAsync(3, "3-byte DWORD", cancellationToken);
        }

        /// <summary>
        /// Asynchronously reads an ASCII string of up to the specified length.
        /// </summary>
        /// <param name="count">Non-negative number of characters to read.</param>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the string read.</returns>
        /// <inheritdoc cref="ReadString" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<string> ReadStringAsync(int count, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNegative(nameof(count), count, "Count is negative.");

            var bytes = await ReadBytesAsync(count, cancellationToken).ConfigureAwait(false);
            return SmfConstants.DefaultTextEncoding.GetString(bytes);
        }

        /// <summary>
        /// Asynchronously reads a 32-bit variable-length quantity.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the integer read.</returns>
        /// <inheritdoc cref="ReadVlqNumber" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="VlqNumberOverflowException">The number exceeds the supported range.</exception>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public async ValueTask<int> ReadVlqNumberAsync(CancellationToken cancellationToken = default)
        {
            return (int)await ReadVlqNumberCoreAsync(VlqIntMaxBytes, VlqIntLastCheckReference, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads a 64-bit variable-length quantity.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel reading.</param>
        /// <returns>A task containing the integer read.</returns>
        /// <inheritdoc cref="ReadVlqLongNumber" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="VlqNumberOverflowException">The number exceeds the supported range.</exception>
        /// <exception cref="OperationCanceledException">Reading was canceled.</exception>
        public ValueTask<long> ReadVlqLongNumberAsync(CancellationToken cancellationToken = default)
        {
            return ReadVlqNumberCoreAsync(VlqLongMaxBytes, VlqLongLastCheckReference, cancellationToken);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiReader));
        }

        private async ValueTask<uint> ReadNumberAsync(int size, string name, CancellationToken cancellationToken)
        {
            var bytes = await ReadBytesAsync(size, cancellationToken).ConfigureAwait(false);
            if (bytes.Length < size)
                throw new NotEnoughBytesException($"Not enough bytes in the stream to read a {name}.", size, bytes.Length);

            uint result = 0;

            foreach (var value in bytes)
            {
                result = (result << 8) | value;
            }

            return result;
        }

        private async ValueTask<long> ReadVlqNumberCoreAsync(
            int maxBytes,
            long lastCheckReference,
            CancellationToken cancellationToken)
        {
            long result = 0;

            try
            {
                for (var count = 1; ; count++)
                {
                    if (count > maxBytes)
                        throw new VlqNumberOverflowException("VLQ number is too large.");

                    var value = await ReadByteAsync(cancellationToken).ConfigureAwait(false);
                    if (count == maxBytes && ((value >> 7) != 0 || result > lastCheckReference))
                        throw new VlqNumberOverflowException("VLQ number is too large.");

                    result = (result << 7) + (value & 127);
                    if (value < 128)
                        return result;
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new NotEnoughBytesException("Not enough bytes in the stream to read a variable-length quantity number.", exception);
            }
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
