using Melanchall.DryWetMidi.Common;
using System;
using System.IO;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Writer of the MIDI data types.
    /// </summary>
    public sealed class MidiWriter : IDisposable
    {
        #region Fields

        private readonly WriterSettings _settings;

        private readonly Stream _stream;

        private readonly byte[] _numberBuffer = new byte[9];
        
        private readonly bool _useBuffering;
        private byte[] _buffer;
        private int _bufferPosition;
        private long _length;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiWriter"/> with the specified stream.
        /// </summary>
        /// <param name="stream">Stream to write MIDI file to.</param>
        /// <param name="settings">Settings according to which MIDI data should be written.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="stream"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="settings"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not support writing,
        /// or is already closed.</exception>
        public MidiWriter(Stream stream, WriterSettings settings)
        {
            ThrowIfArgument.IsNull(nameof(stream), stream);
            ThrowIfArgument.IsNull(nameof(settings), settings);

            _settings = settings;

            _stream = stream;
            _useBuffering = settings.UseBuffering;

            if (_useBuffering)
                PrepareBuffer();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the current count of written bytes.
        /// </summary>
        public long Length
        {
            get { return _length; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes an unsigned byte to the underlying stream and advances the stream position
        /// by one byte.
        /// </summary>
        /// <param name="value">The unsigned byte to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteByte(byte value)
        {
            if (_useBuffering)
            {
                if (_bufferPosition == _buffer.Length)
                    FlushBuffer();

                _buffer[_bufferPosition] = value;
                _bufferPosition++;
            }
            else
                _stream.WriteByte(value);

            _length++;
        }

        /// <summary>
        /// Writes a byte array to the underlying stream.
        /// </summary>
        /// <param name="bytes">A byte array containing the data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteBytes(byte[] bytes)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);

            WriteBytes(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Writes a signed byte to the underlying stream and advances the stream position by one byte.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteSByte(sbyte value)
        {
            WriteByte((byte)value);
        }

        /// <summary>
        /// Writes a WORD value (16-bit unsigned integer) to the underlying stream and
        /// advances the current position by two bytes.
        /// </summary>
        /// <param name="value">WORD value to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteWord(ushort value)
        {
            _numberBuffer[0] = (byte)((value >> 8) & 0xFF);
            _numberBuffer[1] = (byte)(value & 0xFF);

            WriteBytes(_numberBuffer, 0, 2);
        }

        /// <summary>
        /// Writes a DWORD value (32-bit unsigned integer) to the underlying stream and
        /// advances the current position by four bytes.
        /// </summary>
        /// <param name="value">DWORD value to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteDword(uint value)
        {
            _numberBuffer[0] = (byte)((value >> 24) & 0xFF);
            _numberBuffer[1] = (byte)((value >> 16) & 0xFF);
            _numberBuffer[2] = (byte)((value >> 8) & 0xFF);
            _numberBuffer[3] = (byte)(value & 0xFF);

            WriteBytes(_numberBuffer, 0, 4);
        }

        /// <summary>
        /// Writes a INT16 value (16-bit signed integer) to the underlying stream and
        /// advances the current position by two bytes.
        /// </summary>
        /// <param name="value">INT16 value to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteInt16(short value)
        {
            _numberBuffer[0] = (byte)((value >> 8) & 0xFF);
            _numberBuffer[1] = (byte)(value & 0xFF);

            WriteBytes(_numberBuffer, 0, 2);
        }

        /// <summary>
        /// Writes a string to the underlying stream as set of ASCII bytes.
        /// </summary>
        /// <param name="value">The string to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteString(string value)
        {
            var chars = value?.ToCharArray();
            if (chars == null || chars.Length == 0)
                return;

            var bytes = SmfConstants.DefaultTextEncoding.GetBytes(chars);
            WriteBytes(bytes);
        }

        /// <summary>
        /// Writes a 32-bit signed integer to the underlying stream in compressed format called
        /// variable-length quantity (VLQ).
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteVlqNumber(int value)
        {
            WriteVlqNumber((long)value);
        }

        /// <summary>
        /// Writes a 64-bit signed integer to the underlying stream in compressed format called
        /// variable-length quantity (VLQ).
        /// </summary>
        /// <param name="value">The value to write.</param>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// </remarks>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteVlqNumber(long value)
        {
            var bytesCount = value.GetVlqBytes(_numberBuffer);
            WriteBytes(_numberBuffer, _numberBuffer.Length - bytesCount, bytesCount);
        }

        /// <summary>
        /// Writes a DWORD value (32-bit unsigned integer) to the underlying stream as three bytes
        /// and advances the current position by three bytes.
        /// </summary>
        /// <param name="value">DWORD value to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void Write3ByteDword(uint value)
        {
            _numberBuffer[0] = (byte)((value >> 16) & 0xFF);
            _numberBuffer[1] = (byte)((value >> 8) & 0xFF);
            _numberBuffer[2] = (byte)(value & 0xFF);

            WriteBytes(_numberBuffer, 0, 3);
        }

        private void PrepareBuffer()
        {
            if (!_useBuffering)
                return;

            _buffer = new byte[_settings.BufferSize];
        }

        private void WriteBytes(byte[] bytes, int offset, int length)
        {
            if (_useBuffering)
                WriteBytesWithBuffering(bytes, offset, length);
            else
                _stream.Write(bytes, offset, length);

            _length += length;
        }

        private void FlushBuffer()
        {
            _stream.Write(_buffer, 0, _bufferPosition);
            _bufferPosition = 0;
        }

        private void WriteBytesWithBuffering(byte[] bytes, int offset, int length)
        {
            if (_bufferPosition + length <= _buffer.Length)
            {
                WriteBytesToBuffer(bytes, offset, length);
            }
            else
            {
                var firstBytesCount = _buffer.Length - _bufferPosition;
                WriteBytesToBuffer(bytes, offset, firstBytesCount);
                FlushBuffer();
                WriteBytesWithBuffering(bytes, offset + firstBytesCount, length - firstBytesCount);
            }
        }

        private void WriteBytesToBuffer(byte[] bytes, int offset, int length)
        {
            Buffer.BlockCopy(bytes, offset, _buffer, _bufferPosition, length);
            _bufferPosition += length;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MidiWriter"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_useBuffering)
                    FlushBuffer();
                
                _stream.Flush();
            }

            _disposed = true;
        }

        #endregion
    }
}
