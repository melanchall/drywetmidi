using Melanchall.DryWetMidi.Common;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Core
{
    public sealed partial class MidiWriter
    {
        /// <summary>
        /// Asynchronously writes an unsigned byte.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteByteAsync(byte value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            return WritePayloadAsync(new[] { value }, 1, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a byte array.
        /// </summary>
        /// <param name="bytes">The bytes to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteBytes(byte[])" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfDisposed();

            return WritePayloadAsync(bytes, bytes.Length, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a range of a byte array.
        /// </summary>
        /// <param name="bytes">The array containing the bytes to write.</param>
        /// <param name="offset">Zero-based start of the range.</param>
        /// <param name="length">Number of bytes to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The range is outside <paramref name="bytes"/>.</exception>
        /// <exception cref="ObjectDisposedException">The writer or its underlying stream was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred.</exception>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteBytesAsync(
            byte[] bytes,
            int offset,
            int length,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsOutOfRange(nameof(offset), offset, 0, bytes.Length, "Offset is out of range.");
            ThrowIfArgument.IsOutOfRange(nameof(length), length, 0, bytes.Length - offset, "Length is out of range.");
            ThrowIfDisposed();

            return WritePayloadAsync(bytes, length, cancellationToken, offset);
        }

        /// <summary>
        /// Asynchronously writes a signed byte.
        /// </summary>
        /// <param name="value">The byte to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteSByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteSByteAsync(sbyte value, CancellationToken cancellationToken = default)
        {
            return WriteByteAsync((byte)value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a big-endian 16-bit unsigned integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteWord" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteWordAsync(ushort value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _numberBuffer[0] = (byte)(value >> 8);
            _numberBuffer[1] = (byte)value;

            return WriteBytesAsync(_numberBuffer, 0, 2, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a big-endian 32-bit unsigned integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteDwordAsync(uint value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _numberBuffer[0] = (byte)(value >> 24);
            _numberBuffer[1] = (byte)(value >> 16);
            _numberBuffer[2] = (byte)(value >> 8);
            _numberBuffer[3] = (byte)value;

            return WriteBytesAsync(_numberBuffer, 0, 4, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a big-endian 16-bit signed integer.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteInt16" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteInt16Async(short value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            return WriteWordAsync((ushort)value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a string as ASCII bytes.
        /// </summary>
        /// <param name="value">The string to write; <c>null</c> and empty strings write no bytes.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteString" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteStringAsync(string? value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return string.IsNullOrEmpty(value)
                ? Task.CompletedTask
                : WriteBytesAsync(SmfConstants.DefaultTextEncoding.GetBytes(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a 32-bit variable-length quantity.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteVlqNumber(int)" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteVlqNumberAsync(int value, CancellationToken cancellationToken = default)
        {
            return WriteVlqNumberAsync((long)value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a 64-bit variable-length quantity.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="WriteVlqNumber(long)" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteVlqNumberAsync(long value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var bytesCount = value.GetVlqBytes(_numberBuffer);

            return WriteBytesAsync(_numberBuffer, _numberBuffer.Length - bytesCount, bytesCount, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes an unsigned integer as three big-endian bytes.
        /// </summary>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <inheritdoc cref="Write3ByteDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task Write3ByteDwordAsync(uint value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            _numberBuffer[0] = (byte)(value >> 16);
            _numberBuffer[1] = (byte)(value >> 8);
            _numberBuffer[2] = (byte)value;

            return WriteBytesAsync(_numberBuffer, 0, 3, cancellationToken);
        }

        /// <summary>
        /// Asynchronously flushes buffered MIDI bytes without closing the underlying stream.
        /// </summary>
        /// <returns>A task representing disposal.</returns>
        /// <exception cref="IOException">An I/O error occurred while flushing the stream.</exception>
        /// <exception cref="ObjectDisposedException">The underlying stream has been disposed.</exception>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            try
            {
                await FlushBufferAsync(CancellationToken.None).ConfigureAwait(false);
                await _stream.FlushAsync(CancellationToken.None).ConfigureAwait(false);
            }
            finally
            {
                _disposed = true;
            }
        }

        internal async Task WriteDataAsync(byte[] data, CancellationToken cancellationToken)
        {
            await WritePayloadAsync(data, data.Length, cancellationToken).ConfigureAwait(false);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiWriter));
        }

        private async Task WritePayloadAsync(
            byte[] data,
            int count,
            CancellationToken cancellationToken,
            int dataOffset = 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!_useBuffering)
                await _stream.WriteAsync(data, dataOffset, count, cancellationToken).ConfigureAwait(false);
            else
                await WriteBytesWithBufferingAsync(data, dataOffset, count, cancellationToken).ConfigureAwait(false);

            _length += count;
        }

        private async Task WriteBytesWithBufferingAsync(
            byte[] data,
            int offset,
            int count,
            CancellationToken cancellationToken)
        {
            if (_bufferPosition > 0)
            {
                var bytesToBuffer = Math.Min(_buffer!.Length - _bufferPosition, count);
                Buffer.BlockCopy(data, offset, _buffer, _bufferPosition, bytesToBuffer);
                _bufferPosition += bytesToBuffer;
                offset += bytesToBuffer;
                count -= bytesToBuffer;

                if (_bufferPosition == _buffer.Length)
                    await FlushBufferAsync(cancellationToken).ConfigureAwait(false);
            }

            while (count >= _buffer!.Length)
            {
                await _stream.WriteAsync(data, offset, _buffer.Length, cancellationToken).ConfigureAwait(false);
                offset += _buffer.Length;
                count -= _buffer.Length;
            }

            if (count > 0)
            {
                Buffer.BlockCopy(data, offset, _buffer, 0, count);
                _bufferPosition = count;
            }
        }

        private async Task FlushBufferAsync(CancellationToken cancellationToken)
        {
            if (_bufferPosition == 0)
                return;

            await _stream.WriteAsync(_buffer!, 0, _bufferPosition, cancellationToken).ConfigureAwait(false);
            _bufferPosition = 0;
        }
    }
}
