using System;
using System.Collections.Generic;
using System.IO;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides methods to convert an instance of the <see cref="MidiEvent"/> to bytes.
    /// </summary>
    public sealed class MidiEventToBytesConverter : IDisposable
    {
        #region Fields

        private readonly MemoryStream _dataBytesStream;
        private readonly MidiWriter _midiWriter;

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEventToBytesConverter"/> with the specified
        /// initial capacity of internal buffer.
        /// </summary>
        /// <param name="capacity">Initial capacity of the internal buffer.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is negative.</exception>
        public MidiEventToBytesConverter(int capacity)
        {
            ThrowIfArgument.IsNegative(nameof(capacity), capacity, "Capacity is negative.");

            _dataBytesStream = new MemoryStream(capacity);
            _midiWriter = new MidiWriter(_dataBytesStream, new WriterSettings { UseBuffering = false });
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiEventToBytesConverter"/>.
        /// </summary>
        public MidiEventToBytesConverter()
            : this(0)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets settings according to which MIDI data should be write to bytes.
        /// </summary>
        public WritingSettings WritingSettings { get; } = new WritingSettings();

        public bool WriteDeltaTimes { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Converts an instance of the <see cref="MidiEvent"/> to bytes array.
        /// </summary>
        /// <param name="midiEvent">MIDI event to convert.</param>
        /// <returns>Array of bytes representing <paramref name="midiEvent"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        public byte[] Convert(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            return Convert(midiEvent, 0);
        }

        // TODO: improve performance
        /// <summary>
        /// Converts an instance of the <see cref="MidiEvent"/> to bytes array using the specified
        /// minimum size of resulting array.
        /// </summary>
        /// <param name="midiEvent">MIDI event to convert.</param>
        /// <param name="minSize">Minimum size of bytes array representing <paramref name="midiEvent"/>.</param>
        /// <returns>Array of bytes representing <paramref name="midiEvent"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="minSize"/> is negative.</exception>
        public byte[] Convert(MidiEvent midiEvent, int minSize)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsNegative(nameof(minSize), minSize, "Min size is negative.");

            PrepareStream();

            if (WriteDeltaTimes)
                _midiWriter.WriteVlqNumber(midiEvent.DeltaTime);

            var eventWriter = EventWriterFactory.GetWriter(midiEvent);
            eventWriter.Write(midiEvent, _midiWriter, WritingSettings, true);

            return GetBytes(minSize);
        }

        public byte[] Convert(IEnumerable<MidiEvent> midiEvents)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            PrepareStream();

            byte? runningStatus = null;

            foreach (var midiEvent in midiEvents)
            {
                var eventToWrite = midiEvent;

                if (WritingSettings.NoteOffAsSilentNoteOn)
                {
                    var noteOffEvent = eventToWrite as NoteOffEvent;
                    if (noteOffEvent != null)
                        eventToWrite = new NoteOnEvent
                        {
                            DeltaTime = noteOffEvent.DeltaTime,
                            Channel = noteOffEvent.Channel,
                            NoteNumber = noteOffEvent.NoteNumber
                        };
                }

                if (WriteDeltaTimes)
                    _midiWriter.WriteVlqNumber(midiEvent.DeltaTime);

                var eventWriter = EventWriterFactory.GetWriter(midiEvent);

                var writeStatusByte = true;
                if (eventToWrite is ChannelEvent)
                {
                    var statusByte = eventWriter.GetStatusByte(eventToWrite);
                    writeStatusByte = runningStatus != statusByte || !WritingSettings.UseRunningStatus;
                    runningStatus = statusByte;
                }
                else
                    runningStatus = null;

                eventWriter.Write(midiEvent, _midiWriter, WritingSettings, writeStatusByte);
            }

            return GetBytes(0);
        }

        private byte[] GetBytes(int minSize)
        {
            var buffer = _dataBytesStream.GetBuffer();
            var dataSize = _dataBytesStream.Position;
            var result = new byte[Math.Max(dataSize, minSize)];
            Array.Copy(buffer, 0, result, 0, dataSize);

            return result;
        }

        private void PrepareStream()
        {
            _dataBytesStream.Seek(0, SeekOrigin.Begin);
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="MidiEventToBytesConverter"/> class.
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
                _midiWriter.Dispose();
                _dataBytesStream.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
