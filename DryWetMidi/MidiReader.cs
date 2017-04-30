using System;
using System.IO;
using System.Text;

namespace Melanchall.DryMidi
{
    /// <summary>
    /// Reader of the MIDI data types.
    /// </summary>
    public sealed class MidiReader : IDisposable
    {
        #region Fields

        private readonly BinaryReader _binaryReader;
        private bool _disposed;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiReader"/> with the specified stream.
        /// </summary>
        /// <param name="stream">Stream to read MIDI file from.</param>
        /// <exception cref="ArgumentNullException"><paramref name="stream"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="stream"/> does not support reading,
        /// or is already closed.</exception>
        public MidiReader(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            _binaryReader = new BinaryReader(stream, Encoding.ASCII);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the position within the underlying stream.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        /// <exception cref="ObjectDisposedException">Property was called after the reader was disposed.</exception>
        public long Position
        {
            get { return _binaryReader.BaseStream.Position; }
            set { _binaryReader.BaseStream.Position = value; }
        }

        /// <summary>
        /// Gets a value indicating whether end of the underlying stream is reached.
        /// </summary>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        /// <exception cref="ObjectDisposedException">Property was called after the reader was disposed.</exception>
        public bool EndReached => _binaryReader.BaseStream.Position >= _binaryReader.BaseStream.Length;

        #endregion

        #region Methods

        /// <summary>
        /// Reads a byte from the underlying stream and advances the current position by one byte.
        /// </summary>
        /// <returns>The next byte read from the underlying stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the underlying stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public byte ReadByte()
        {
            return _binaryReader.ReadByte();
        }

        /// <summary>
        /// Reads a signed byte from the underlying stream and advances the current position by one byte.
        /// </summary>
        /// <returns>A signed byte read from the underlying stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the underlying stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public sbyte ReadSByte()
        {
            return _binaryReader.ReadSByte();
        }

        /// <summary>
        /// Reads the specified number of bytes from the underlying stream into a byte array
        /// and advances the current position by that number of bytes.
        /// </summary>
        /// <param name="count">The number of bytes to read. This value must be 0 or a
        /// non-negative number or an exception will occur.</param>
        /// <returns>A byte array containing data read from the underlying stream. This might be less
        /// than the number of bytes requested if the end of the stream is reached.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public byte[] ReadBytes(int count)
        {
            return _binaryReader.ReadBytes(count);
        }

        /// <summary>
        /// Reads a WORD value (16-bit unsigned integer) from the underlying stream and
        /// advances the current position by two bytes.
        /// </summary>
        /// <returns>A 16-bit unsigned integer read from the underlying stream.</returns>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public ushort ReadWord()
        {
            var bytes = ReadBytes(sizeof(ushort));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt16(bytes, 0);
        }

        /// <summary>
        /// Reads a DWORD value (32-bit unsigned integer) from the underlying stream and
        /// advances the current position by four bytes.
        /// </summary>
        /// <returns>A 32-bit unsigned integer read from the underlying stream.</returns>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public uint ReadDword()
        {
            var bytes = ReadBytes(sizeof(uint));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToUInt32(bytes, 0);
        }

        /// <summary>
        /// Reads an INT16 value (16-bit signed integer) from the underlying stream and
        /// advances the current position by two bytes.
        /// </summary>
        /// <returns>A 16-bit signed integer read from the underlying stream.</returns>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public short ReadInt16()
        {
            var bytes = ReadBytes(sizeof(short));
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt16(bytes, 0);
        }

        /// <summary>
        /// Reads the specified number of characters from the underlying stream, returns the
        /// data as string, and advances the current position by that number of characters.
        /// </summary>
        /// <param name="count">The length of string to read.</param>
        /// <returns>The string being read.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is negative.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public string ReadString(int count)
        {
            var chars = _binaryReader.ReadChars(count);
            return new string(chars);
        }

        /// <summary>
        /// Reads a 32-bit signed integer presented in compressed format called variable-length quantity (VLQ)
        /// to the underlying stream.
        /// </summary>
        /// <remarks>
        /// Numbers in VLQ format are represented 7 bits per byte, most significant bits first.
        /// All bytes except the last have bit 7 set, and the last byte has bit 7 clear. If the
        /// number is between 0 and 127, it is thus represented exactly as one byte.
        /// The largest number which is allowed is 0FFFFFFF so that the VLQ representations
        /// must fit in 32 bits in a routine to write variable-length numbers.
        /// </remarks>
        /// <returns>A 32-bit signed integer read from the underlying stream.</returns>
        /// <exception cref="EndOfStreamException">The end of the underlying stream is reached.</exception>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public int ReadVlqNumber()
        {
            int result = 0;
            byte b;

            do
            {
                b = ReadByte();
                result = (result << 7) + (b & 127);
            }
            while (b >> 7 != 0);

            return result;
        }

        /// <summary>
        /// Reads a DWORD value (32-bit unsigned integer) presented by 3 bytes from the underlying
        /// stream and advances the current position by three bytes.
        /// </summary>
        /// <returns>A 32-bit unsigned integer read from the underlying stream.</returns>
        /// <exception cref="ObjectDisposedException">Method was called after the reader was disposed.</exception>
        /// <exception cref="IOException">An I/O error occurred on the underlying stream.</exception>
        public uint Read3ByteDword()
        {
            var bytes = ReadBytes(3);

            var bytesForInt = new byte[sizeof(uint)];
            Array.Copy(bytes, 0, bytesForInt, bytesForInt.Length - bytes.Length, bytes.Length);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytesForInt);

            return BitConverter.ToUInt32(bytesForInt, 0);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MidiReader"/> class.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _binaryReader.Dispose();
            _disposed = true;
        }

        #endregion
    }
}
