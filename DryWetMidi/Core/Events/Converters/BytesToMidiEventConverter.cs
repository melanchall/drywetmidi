using System;
using System.IO;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides methods to convert bytes to an instance of the <see cref="MidiEvent"/>.
    /// </summary>
    public sealed class BytesToMidiEventConverter : IDisposable
    {
        #region Fields

        private readonly MemoryStream _dataBytesStream;
        private readonly MidiReader _midiReader;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="BytesToMidiEventConverter"/> with the specified
        /// initial capacity of internal buffer.
        /// </summary>
        /// <param name="capacity">Initial capacity of the internal buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.</exception>
        public BytesToMidiEventConverter(int capacity)
        {
            ThrowIfArgument.IsNegative(nameof(capacity), capacity, "Capacity is negative.");

            _dataBytesStream = new MemoryStream(capacity);
            _midiReader = new MidiReader(_dataBytesStream, new ReaderSettings());
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BytesToMidiEventConverter"/>.
        /// </summary>
        public BytesToMidiEventConverter()
            : this(0)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets settings according to which MIDI data should be read from bytes.
        /// </summary>
        public ReadingSettings ReadingSettings { get; } = new ReadingSettings();

        #endregion

        #region Methods

        // TODO: improve performance
        /// <summary>
        /// Converts the specified status byte and data bytes to an instance of the <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="statusByte">The status byte of MIDI event.</param>
        /// <param name="dataBytes">Data bytes of MIDI event (bytes except status byte). Can be <c>null</c>
        /// if MIDI event has no data bytes.</param>
        /// <returns><see cref="MidiEvent"/> read from <paramref name="statusByte"/> and <paramref name="dataBytes"/>.</returns>
        public MidiEvent Convert(byte statusByte, byte[] dataBytes)
        {
            _dataBytesStream.Seek(0, SeekOrigin.Begin);
            if (dataBytes != null)
                _dataBytesStream.Write(dataBytes, 0, dataBytes.Length);
            
            _midiReader.Position = 0;

            var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: false);
            return eventReader.Read(_midiReader, ReadingSettings, statusByte);
        }

        /// <summary>
        /// Converts the specified bytes to an instance of the <see cref="MidiEvent"/>. First byte is
        /// the status byte of MIDI event.
        /// </summary>
        /// <param name="bytes">Bytes representing a MIDI event.</param>
        /// <returns><see cref="MidiEvent"/> read from <paramref name="bytes"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is an empty array.</exception>
        public MidiEvent Convert(byte[] bytes)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsEmptyCollection(nameof(bytes), bytes, "Bytes is empty array.");

            return Convert(bytes, 0, bytes.Length);
        }

        /// <summary>
        /// Converts sub-array of the specified bytes to an instance of the <see cref="MidiEvent"/>.
        /// </summary>
        /// <param name="bytes">Bytes to take sub-array from.</param>
        /// <param name="offset">Offset of sub-array to read MIDI event from.</param>
        /// <param name="length">Length of sub-array to read MIDI event from.</param>
        /// <returns><see cref="MidiEvent"/> read from <paramref name="bytes"/> starting from
        /// <paramref name="offset"/> and taking <paramref name="length"/> of bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is an empty array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="offset"/> is out of range.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is out of range.</description>
        /// </item>
        /// </list>
        /// </exception>
        public MidiEvent Convert(byte[] bytes, int offset, int length)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsEmptyCollection(nameof(bytes), bytes, "Bytes is empty array.");
            ThrowIfArgument.IsOutOfRange(nameof(offset), offset, 0, bytes.Length - 1, "Offset is out of range.");
            ThrowIfArgument.IsOutOfRange(nameof(length), length, 0, bytes.Length - offset, "Length is out of range.");

            var dataBytes = new byte[bytes.Length - 1 - offset];
            Array.Copy(bytes, offset + 1, dataBytes, 0, dataBytes.Length);

            return Convert(bytes[offset], dataBytes);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="BytesToMidiEventConverter"/> class.
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
                _dataBytesStream.Dispose();
                _midiReader.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
