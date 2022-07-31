using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
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

        private readonly WritingSettings _writingSettings = new WritingSettings();

        private BytesFormat _bytesFormat = BytesFormat.File;

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
        /// Gets or sets a value indicating whether 'running status' (to turn off writing of the status
        /// bytes of consecutive events of the same type) should be used or not. The default value is <c>false</c>.
        /// </summary>
        public bool UseRunningStatus
        {
            get { return _writingSettings.UseRunningStatus; }
            set { _writingSettings.UseRunningStatus = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether Note Off events should be written as Note On ones
        /// with velocity of zero, or not. In conjunction with <see cref="UseRunningStatus"/> set to <c>true</c>
        /// can give some compression of MIDI data. The default value is <c>false</c>.
        /// </summary>
        public bool NoteOffAsSilentNoteOn
        {
            get { return _writingSettings.NoteOffAsSilentNoteOn; }
            set { _writingSettings.NoteOffAsSilentNoteOn = value; }
        }

        /// <summary>
        /// Gets or sets collection of custom meta events types.
        /// </summary>
        /// <remarks>
        /// <para>Types within this collection must be derived from the <see cref="MetaEvent"/>
        /// class and have parameterless constructor. No exception will be thrown
        /// while writing a MIDI file if some types don't meet these requirements.</para>
        /// </remarks>
        public EventTypesCollection CustomMetaEventTypes
        {
            get { return _writingSettings.CustomMetaEventTypes; }
            set { _writingSettings.CustomMetaEventTypes = value; }
        }

        /// <summary>
        /// Gets or sets an <see cref="Encoding"/> that will be used to write the text of a
        /// text-based meta event. The default is <see cref="Encoding.ASCII"/>.
        /// </summary>
        public Encoding TextEncoding
        {
            get { return _writingSettings.TextEncoding; }
            set { _writingSettings.TextEncoding = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether delta-times of events must be written to bytes
        /// array or not. The default value is <c>false</c>.
        /// </summary>
        public bool WriteDeltaTimes { get; set; }

        /// <summary>
        /// Gets or sets the format of target bytes layout. The default is <see cref="BytesFormat.File"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public BytesFormat BytesFormat
        {
            get { return _bytesFormat; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);
                _bytesFormat = value;
            }
        }

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

            if (midiEvent is NormalSysExEvent && BytesFormat == BytesFormat.Device)
            {
                _midiWriter.WriteByte(EventStatusBytes.Global.NormalSysEx);
                _midiWriter.WriteBytes(((NormalSysExEvent)midiEvent).Data);
            }
            else
            {
                var eventWriter = EventWriterFactory.GetWriter(midiEvent);
                eventWriter.Write(midiEvent, _midiWriter, _writingSettings, true);
            }

            return GetBytes(minSize);
        }

        /// <summary>
        /// Converts collection of <see cref="MidiEvent"/> to bytes array.
        /// </summary>
        /// <param name="midiEvents">MIDI events to convert.</param>
        /// <returns>Array of bytes representing <paramref name="midiEvents"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvents"/> is <c>null</c>.</exception>
        public byte[] Convert(IEnumerable<MidiEvent> midiEvents)
        {
            ThrowIfArgument.IsNull(nameof(midiEvents), midiEvents);

            PrepareStream();

            byte? runningStatus = null;

            foreach (var midiEvent in midiEvents)
            {
                var eventToWrite = midiEvent;

                if (NoteOffAsSilentNoteOn)
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

                if (eventToWrite is NormalSysExEvent && BytesFormat == BytesFormat.Device)
                {
                    _midiWriter.WriteByte(EventStatusBytes.Global.NormalSysEx);
                    _midiWriter.WriteBytes(((NormalSysExEvent)eventToWrite).Data);
                    continue;
                }

                var writeStatusByte = true;
                if (eventToWrite is ChannelEvent)
                {
                    var statusByte = eventWriter.GetStatusByte(eventToWrite);
                    writeStatusByte = runningStatus != statusByte || !UseRunningStatus;
                    runningStatus = statusByte;
                }
                else
                    runningStatus = null;

                eventWriter.Write(midiEvent, _midiWriter, _writingSettings, writeStatusByte);
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
