using System;
using System.IO;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class BytesToMidiEventConverter : IDisposable
    {
        #region Fields

        private readonly MemoryStream _dataBytesStream;
        private readonly MidiReader _midiReader;

        private bool _disposed;

        #endregion

        #region Constructor

        public BytesToMidiEventConverter(int capacity)
        {
            ThrowIfArgument.IsNegative(nameof(capacity), capacity, "Capacity is negative.");

            _dataBytesStream = new MemoryStream(capacity);
            _midiReader = new MidiReader(_dataBytesStream);
        }

        public BytesToMidiEventConverter()
            : this(0)
        {
        }

        #endregion

        #region Properties

        public ReadingSettings ReadingSettings { get; } = new ReadingSettings();

        #endregion

        #region Methods

        public MidiEvent Convert(byte statusByte, byte[] dataBytes)
        {
            _dataBytesStream.Seek(0, SeekOrigin.Begin);
            if (dataBytes != null)
                _dataBytesStream.Write(dataBytes, 0, dataBytes.Length);
            _dataBytesStream.Seek(0, SeekOrigin.Begin);

            var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: false);
            return eventReader.Read(_midiReader, ReadingSettings, statusByte);
        }

        public MidiEvent Convert(byte[] bytes)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsEmptyCollection(nameof(bytes), bytes, "Bytes is empty array.");

            return Convert(bytes, 0, bytes.Length);
        }

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
