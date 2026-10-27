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
        /// Asynchronously writes a byte.
        /// </summary>
        /// <inheritdoc cref="WriteByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The byte to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteByteAsync(byte value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.WriteByte(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a byte array.
        /// </summary>
        /// <inheritdoc cref="WriteBytes(byte[])" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="bytes">Data to write. Do not modify it until writing completes.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteBytesAsync(byte[] bytes, CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);

            return WritePayloadAsync(bytes, bytes.Length, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a range of a byte array without copying the entire range.
        /// </summary>
        /// <param name="bytes">Array containing the data. Do not modify it until writing completes.</param>
        /// <param name="offset">Zero-based index of the first byte to write.</param>
        /// <param name="length">Number of bytes to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The specified range is outside the array.</exception>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        /// <exception cref="ObjectDisposedException">The writer or underlying stream has been disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing.</exception>
        public Task WriteBytesAsync(
            byte[] bytes,
            int offset,
            int length,
            CancellationToken cancellationToken = default)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsOutOfRange(nameof(offset), offset, 0, bytes.Length, "Offset is out of range.");
            ThrowIfArgument.IsOutOfRange(nameof(length), length, 0, bytes.Length - offset, "Length is out of range.");

            return WritePayloadAsync(bytes, length, cancellationToken, offset);
        }

        /// <summary>
        /// Asynchronously writes a signed byte.
        /// </summary>
        /// <inheritdoc cref="WriteSByte" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The signed byte to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteSByteAsync(sbyte value, CancellationToken cancellationToken = default)
        {
            return WriteByteAsync((byte)value, cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a WORD (16-bit unsigned integer).
        /// </summary>
        /// <inheritdoc cref="WriteWord" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The WORD value to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteWordAsync(ushort value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.WriteWord(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a DWORD (32-bit unsigned integer).
        /// </summary>
        /// <inheritdoc cref="WriteDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The DWORD value to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteDwordAsync(uint value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.WriteDword(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes an INT16 (16-bit signed integer).
        /// </summary>
        /// <inheritdoc cref="WriteInt16" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The signed integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteInt16Async(short value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.WriteInt16(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a string using the MIDI ASCII encoding.
        /// </summary>
        /// <inheritdoc cref="WriteString" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The string to write; null and empty strings write no bytes.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteStringAsync(string? value, CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            return WriteBytesAsync(SmfConstants.DefaultTextEncoding.GetBytes(value ?? string.Empty), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a variable-length 32-bit integer.
        /// </summary>
        /// <inheritdoc cref="WriteVlqNumber(int)" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteVlqNumberAsync(int value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.WriteVlqNumber(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a variable-length 64-bit integer.
        /// </summary>
        /// <inheritdoc cref="WriteVlqNumber(long)" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The integer to write.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task WriteVlqNumberAsync(long value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.WriteVlqNumber(value), cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes a DWORD represented by three bytes.
        /// </summary>
        /// <inheritdoc cref="Write3ByteDword" path="/*[not(self::summary) and not(self::param) and not(self::returns)]"/>
        /// <param name="value">The DWORD value to write as three bytes.</param>
        /// <param name="cancellationToken">Token used to cancel writing.</param>
        /// <returns>A task representing writing.</returns>
        /// <exception cref="OperationCanceledException">Writing was canceled.</exception>
        public Task Write3ByteDwordAsync(uint value, CancellationToken cancellationToken = default)
        {
            return WriteEncodedAsync(codec => codec.Write3ByteDword(value), cancellationToken);
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

        internal MidiIoProgress? AsyncProgress { get; set; }

        internal async Task WriteEncodedAsync(Action<MidiWriter> encode, CancellationToken cancellationToken)
        {
            ThrowIfDisposed();
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

        private async Task WritePayloadAsync(
            byte[] data,
            int count,
            CancellationToken cancellationToken,
            int sourceOffset = 0)
        {
            ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (!_useBuffering)
                await WriteTransportAsync(data, count, cancellationToken, sourceOffset).ConfigureAwait(false);
            else
            {
                for (var offset = 0; offset < count;)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var size = Math.Min(count - offset, _buffer!.Length - _bufferPosition);
                    Buffer.BlockCopy(data, sourceOffset + offset, _buffer, _bufferPosition, size);
                    _bufferPosition += size;
                    offset += size;
                    if (_bufferPosition == _buffer.Length)
                        await FlushBufferAsync(cancellationToken).ConfigureAwait(false);
                }
            }

            _length += count;
        }

        private async Task WriteTransportAsync(
            byte[] data,
            int count,
            CancellationToken cancellationToken,
            int sourceOffset = 0)
        {
            for (var offset = 0; offset < count;)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var size = Math.Min(65536, count - offset);
                await _stream.WriteAsync(data, sourceOffset + offset, size, cancellationToken).ConfigureAwait(false);
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

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(MidiWriter));
        }
    }
}
