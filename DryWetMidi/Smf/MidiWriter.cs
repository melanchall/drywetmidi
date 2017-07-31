using Melanchall.DryWetMidi.Common;
using System;
using System.IO;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// Writer of the MIDI data types.
    /// </summary>
    public sealed class MidiWriter : IDisposable
    {
        #region Fields

        private readonly BinaryWriter _binaryWriter;
        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiWriter"/> with the specified stream.
        /// </summary>
        /// <param name="stream">Stream to write MIDI file to.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not support writing,
        /// or is already closed.</exception>
        public MidiWriter(Stream stream)
        {
            _binaryWriter = new BinaryWriter(stream, SmfUtilities.DefaultEncoding);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clears all buffers for the current writer and causes any buffered data to be
        /// written to the underlying file.
        /// </summary>
        public void Flush()
        {
            _binaryWriter.Flush();
        }

        /// <summary>
        /// Writes an unsigned byte to the underlying stream and advances the stream position
        /// by one byte.
        /// </summary>
        /// <param name="value">The unsigned byte to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteByte(byte value)
        {
            _binaryWriter.Write(value);
        }

        /// <summary>
        /// Writes a byte array to the underlying stream.
        /// </summary>
        /// <param name="bytes">A byte array containing the data to write.</param>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is null.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteBytes(byte[] bytes)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);

            _binaryWriter.Write(bytes);
        }

        /// <summary>
        /// Writes a signed byte to the underlying stream and advances the stream position by one byte.
        /// </summary>
        /// <param name="value">The signed byte to write.</param>
        /// <exception cref="ObjectDisposedException">Method was called after the writer was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public void WriteSByte(sbyte value)
        {
            _binaryWriter.Write(value);
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
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            WriteBytes(bytes);
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
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            WriteBytes(bytes);
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
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            WriteBytes(bytes);
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
            if (chars != null && chars.Length > 0)
                _binaryWriter.Write(chars);
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
            var bytes = value.GetVlqBytes();
            WriteBytes(bytes);
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
            const int mask = 255;
            var bytes = new byte[3];

            for (int i = bytes.Length; --i >= 0;)
            {
                bytes[i] = (byte)(value & mask);
                value >>= 8;
            }

            WriteBytes(bytes);
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
                _binaryWriter.Dispose();

            _disposed = true;
        }

        #endregion
    }
}
