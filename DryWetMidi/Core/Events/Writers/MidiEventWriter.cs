using System;
using System.IO;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class MidiEventWriter : IDisposable
    {
        #region Fields

        private readonly MemoryStream _dataBytesStream;
        private readonly MidiWriter _midiWriter;

        private bool _disposed;

        #endregion

        #region Constructor

        public MidiEventWriter(int capacity)
        {
            ThrowIfArgument.IsNegative(nameof(capacity), capacity, "Capacity is negative.");

            _dataBytesStream = new MemoryStream(capacity);
            _midiWriter = new MidiWriter(_dataBytesStream);
        }

        public MidiEventWriter()
            : this(0)
        {
        }

        #endregion

        #region Properties

        public WritingSettings WritingSettings { get; set; } = new WritingSettings();

        #endregion

        #region Methods

        public byte[] Write(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            return Write(midiEvent, 0);
        }

        public byte[] Write(MidiEvent midiEvent, int minSize)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsNegative(nameof(minSize), minSize, "Min size is negative.");

            _dataBytesStream.Seek(0, SeekOrigin.Begin);

            var settings = WritingSettings ?? new WritingSettings();
            var eventWriter = EventWriterFactory.GetWriter(midiEvent);
            eventWriter.Write(midiEvent, _midiWriter, settings, true);

            var buffer = _dataBytesStream.GetBuffer();
            var dataSize = _dataBytesStream.Position;
            var result = new byte[Math.Max(dataSize, minSize)];
            Array.Copy(buffer, 0, result, 0, dataSize);

            return result;
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MidiEventReader"/> class.
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
                _midiWriter.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
