using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Provides methods to convert bytes to an instance of the <see cref="MidiEvent"/>.
    /// </summary>
    public sealed class BytesToMidiEventConverter : IDisposable
    {
        #region Constants

        private static readonly IEventReader MetaEventReader = new MetaEventReader();

        #endregion

        #region Fields

        private readonly MemoryStream _dataBytesStream;
        private MidiReader _midiReader;

        private BytesFormat _bytesFormat = BytesFormat.File;

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
        /// Gets or sets reaction of the reading engine on unknown channel event. The default is
        /// <see cref="UnknownChannelEventPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="UnknownChannelEventPolicy.Abort"/> is used, an instance of the
        /// <see cref="UnknownChannelEventException"/> will be thrown if channel event has unknown status byte.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public UnknownChannelEventPolicy UnknownChannelEventPolicy
        {
            get { return ReadingSettings.UnknownChannelEventPolicy; }
            set { ReadingSettings.UnknownChannelEventPolicy = value; }
        }

        /// <summary>
        /// Gets or sets a callback used to read unknown channel event if <see cref="UnknownChannelEventPolicy"/>
        /// set to <see cref="UnknownChannelEventPolicy.UseCallback"/>.
        /// </summary>
        public UnknownChannelEventCallback UnknownChannelEventCallback
        {
            get { return ReadingSettings.UnknownChannelEventCallback; }
            set { ReadingSettings.UnknownChannelEventCallback = value; }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on <c>Note On</c> events with velocity of zero.
        /// The default is <see cref="SilentNoteOnPolicy.NoteOff"/>.
        /// </summary>
        /// <remarks>
        /// <para>Although it is recommended to treat silent <c>Note On</c> event as <c>Note Off</c> you can turn
        /// this behavior off to get original event.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public SilentNoteOnPolicy SilentNoteOnPolicy
        {
            get { return ReadingSettings.SilentNoteOnPolicy; }
            set { ReadingSettings.SilentNoteOnPolicy = value; }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a channel event's
        /// parameter value. The default is <see cref="InvalidChannelEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>Valid values are 0-127 so, for example, 128 is the invalid one
        /// and will be processed according with this policy. If <see cref="InvalidChannelEventParameterValuePolicy.Abort"/>
        /// is used, an instance of the <see cref="InvalidChannelEventParameterValueException"/> will be thrown if
        /// event's parameter value just read is invalid.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidChannelEventParameterValuePolicy InvalidChannelEventParameterValuePolicy
        {
            get { return ReadingSettings.InvalidChannelEventParameterValuePolicy; }
            set { ReadingSettings.InvalidChannelEventParameterValuePolicy = value; }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a meta event's
        /// parameter value. The default is <see cref="InvalidMetaEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>For example, 255 is the invalid value for the <see cref="KeySignatureEvent.Scale"/>
        /// and will be processed according with this policy. If <see cref="InvalidMetaEventParameterValuePolicy.Abort"/>
        /// is used, an instance of the <see cref="InvalidMetaEventParameterValueException"/> will be thrown if event's
        /// parameter value just read is invalid.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidMetaEventParameterValuePolicy InvalidMetaEventParameterValuePolicy
        {
            get { return ReadingSettings.InvalidMetaEventParameterValuePolicy; }
            set { ReadingSettings.InvalidMetaEventParameterValuePolicy = value; }
        }

        // TODO: test
        /// <summary>
        /// Gets or sets reaction of the reading engine on invalid value of a system common event's
        /// parameter value. The default is <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>For example, 255 is the invalid value for the <see cref="SongSelectEvent.Number"/>
        /// and will be processed according with this policy. If <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/>
        /// is used, an instance of the <see cref="InvalidSystemCommonEventParameterValueException"/> will be thrown if event's
        /// parameter value just read is invalid.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public InvalidSystemCommonEventParameterValuePolicy InvalidSystemCommonEventParameterValuePolicy
        {
            get { return ReadingSettings.InvalidSystemCommonEventParameterValuePolicy; }
            set { ReadingSettings.InvalidSystemCommonEventParameterValuePolicy = value; }
        }

        /// <summary>
        /// Gets or sets collection of custom meta events types.
        /// </summary>
        /// <remarks>
        /// <para>Types within this collection must be derived from the <see cref="MetaEvent"/>
        /// class and have parameterless constructor. No exception will be thrown
        /// if some types don't meet these requirements.</para>
        /// </remarks>
        public EventTypesCollection CustomMetaEventTypes
        {
            get { return ReadingSettings.CustomMetaEventTypes; }
            set { ReadingSettings.CustomMetaEventTypes = value; }
        }

        /// <summary>
        /// Gets or sets an <see cref="Encoding"/> that will be used to read the text of a
        /// text-based meta events. The default is <see cref="Encoding.ASCII"/>.
        /// </summary>
        /// <remarks>
        /// <para>Value of this property will be used only if <see cref="DecodeTextCallback"/> is not set.</para>
        /// </remarks>
        public Encoding TextEncoding
        {
            get { return ReadingSettings.TextEncoding; }
            set { ReadingSettings.TextEncoding = value; }
        }

        /// <summary>
        /// Gets or sets a callback used to decode a string from the specified bytes during reading a text-based
        /// meta event's text. The default is <c>null</c>.
        /// </summary>
        /// <remarks>
        /// <para>If callback is not set, <see cref="TextEncoding"/> will be used.</para>
        /// </remarks>
        public DecodeTextCallback DecodeTextCallback
        {
            get { return ReadingSettings.DecodeTextCallback; }
            set { ReadingSettings.DecodeTextCallback = value; }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on zero-length objects such as strings or arrays.
        /// The default is <see cref="ZeroLengthDataPolicy.ReadAsEmptyObject"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public ZeroLengthDataPolicy ZeroLengthDataPolicy
        {
            get { return ReadingSettings.ZeroLengthDataPolicy; }
            set { ReadingSettings.ZeroLengthDataPolicy = value; }
        }

        /// <summary>
        /// Gets or sets reaction of the reading engine on lack of bytes in the underlying stream
        /// that are needed to read MIDI data (for example, DWORD requires 4 bytes available).
        /// The default is <see cref="NotEnoughBytesPolicy.Abort"/>.
        /// </summary>
        /// <remarks>
        /// <para>If <see cref="NotEnoughBytesPolicy.Abort"/> is used, an instance of the
        /// <see cref="NotEnoughBytesException"/> will be thrown if the reader's underlying stream doesn't
        /// have enough bytes to read MIDI data.</para>
        /// </remarks>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public NotEnoughBytesPolicy NotEnoughBytesPolicy
        {
            get { return ReadingSettings.NotEnoughBytesPolicy; }
            set { ReadingSettings.NotEnoughBytesPolicy = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether incoming bytes array contains delta-times before
        /// each event data (and thus they must be read) or not. The default value is <c>false</c>.
        /// </summary>
        public bool ReadDeltaTimes { get; set; }

        /// <summary>
        /// Gets or sets the format of source bytes layout. The default is <see cref="BytesFormat.File"/>.
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

        internal ReadingSettings ReadingSettings { get; } = new ReadingSettings();

        #endregion

        #region Methods

        /// <summary>
        /// Converts sub-array of the specified bytes to a collection of <see cref="MidiEvent"/>. 
        /// </summary>
        /// <param name="bytes">Bytes to take sub-array from.</param>
        /// <param name="offset">Offset of sub-array to read MIDI events from.</param>
        /// <param name="length">Length of sub-array to read MIDI events from.</param>
        /// <returns>Collection of <see cref="MidiEvent"/> read from <paramref name="bytes"/> starting from
        /// <paramref name="offset"/> and taking <paramref name="length"/> of bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is an empty array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="offset"/> is out of range.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="length"/> is out of range.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter
        /// just read is invalid (is out of [0; 127] range) and that should be treated as error according to the
        /// <see cref="InvalidChannelEventParameterValuePolicy"/>.</exception>
        /// <exception cref="InvalidMetaEventParameterValueException">Value of a meta event's parameter
        /// just read is invalid and that should be treated as error according to the
        /// <see cref="InvalidMetaEventParameterValuePolicy"/>.</exception>
        /// <exception cref="NotEnoughBytesException">MIDI events data cannot be read since the sub-array <paramref name="bytes"/>
        /// doesn't have enough bytes and that should be treated as error according to the <see cref="NotEnoughBytesPolicy"/>.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        public ICollection<MidiEvent> ConvertMultiple(byte[] bytes, int offset, int length)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsEmptyCollection(nameof(bytes), bytes, "Bytes is empty array.");
            ThrowIfArgument.IsOutOfRange(nameof(offset), offset, 0, bytes.Length - 1, "Offset is out of range.");
            ThrowIfArgument.IsOutOfRange(nameof(length), length, 0, bytes.Length - offset, "Length is out of range.");

            PrepareStreamWithBytes(bytes, offset, length);

            var result = new List<MidiEvent>();
            byte? channelEventStatusByte = null;

            try
            {
                while (_midiReader.Position < length)
                {
                    long deltaTime = 0;

                    if (ReadDeltaTimes)
                    {
                        deltaTime = _midiReader.ReadVlqLongNumber();
                        if (deltaTime < 0)
                            deltaTime = 0;
                    }

                    var statusByte = _midiReader.ReadByte();
                    if (statusByte <= SevenBitNumber.MaxValue)
                    {
                        if (channelEventStatusByte == null)
                            throw new UnexpectedRunningStatusException();

                        statusByte = channelEventStatusByte.Value;
                        _midiReader.Position--;
                    }

                    var midiEvent = ReadEvent(statusByte);
                    if (midiEvent is ChannelEvent)
                        channelEventStatusByte = statusByte;

                    midiEvent.DeltaTime = deltaTime;
                    result.Add(midiEvent);
                }
            }
            catch (EndOfStreamException ex)
            {
                switch (NotEnoughBytesPolicy)
                {
                    case NotEnoughBytesPolicy.Abort:
                        throw new NotEnoughBytesException("Not enough bytes to read an event.", ex);
                }
            }

            return result;
        }

        /// <summary>
        /// Converts the specified bytes to a collection of <see cref="MidiEvent"/>. 
        /// </summary>
        /// <param name="bytes">Bytes to convert to collection of <see cref="MidiEvent"/>.</param>
        /// <returns>Collection of <see cref="MidiEvent"/> read from <paramref name="bytes"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is an empty array.</exception>
        /// <exception cref="InvalidChannelEventParameterValueException">Value of a channel event's parameter
        /// just read is invalid (is out of [0; 127] range) and that should be treated as error according to the
        /// <see cref="InvalidChannelEventParameterValuePolicy"/>.</exception>
        /// <exception cref="InvalidMetaEventParameterValueException">Value of a meta event's parameter
        /// just read is invalid and that should be treated as error according to the
        /// <see cref="InvalidMetaEventParameterValuePolicy"/>.</exception>
        /// <exception cref="NotEnoughBytesException">MIDI events data cannot be read since the sub-array <paramref name="bytes"/>
        /// doesn't have enough bytes and that should be treated as error according to the <see cref="NotEnoughBytesPolicy"/>.</exception>
        /// <exception cref="UnexpectedRunningStatusException">Unexpected running status is encountered.</exception>
        public ICollection<MidiEvent> ConvertMultiple(byte[] bytes)
        {
            ThrowIfArgument.IsNull(nameof(bytes), bytes);
            ThrowIfArgument.IsEmptyCollection(nameof(bytes), bytes, "Bytes is empty array.");

            return ConvertMultiple(bytes, 0, bytes.Length);
        }

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
            PrepareStreamWithBytes(dataBytes, 0, dataBytes?.Length ?? 0);
            return ReadEvent(statusByte);
        }

        /// <summary>
        /// Converts the specified bytes to an instance of the <see cref="MidiEvent"/>. First byte is
        /// the status byte of MIDI event. If bytes array contains multiple events, only first one will be read.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ConvertMultiple(byte[])"/> or <see cref="ConvertMultiple(byte[], int, int)"/> to read
        /// multiple events.
        /// </remarks>
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
        /// Converts sub-array of the specified bytes to an instance of the <see cref="MidiEvent"/>. First byte
        /// at the specified offset is the status byte of MIDI event. If sub-array contains multiple events, only
        /// first one will be read.
        /// </summary>
        /// <remarks>
        /// Use <see cref="ConvertMultiple(byte[])"/> or <see cref="ConvertMultiple(byte[], int, int)"/> to read
        /// multiple events.
        /// </remarks>
        /// <param name="bytes">Bytes to take sub-array from.</param>
        /// <param name="offset">Offset of sub-array to read MIDI event from.</param>
        /// <param name="length">Length of sub-array to read MIDI event from.</param>
        /// <returns><see cref="MidiEvent"/> read from <paramref name="bytes"/> starting from
        /// <paramref name="offset"/> and taking <paramref name="length"/> of bytes.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="bytes"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="bytes"/> is an empty array.</exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <para>One of the following errors occurred:</para>
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

            PrepareStreamWithBytes(bytes, offset, length);

            long deltaTime = 0;

            if (ReadDeltaTimes)
            {
                deltaTime = _midiReader.ReadVlqLongNumber();
                if (deltaTime < 0)
                    deltaTime = 0;
            }

            var statusByte = _midiReader.ReadByte();
            var midiEvent = ReadEvent(statusByte);
            midiEvent.DeltaTime = deltaTime;

            return midiEvent;
        }

        private void PrepareStreamWithBytes(byte[] bytes, int offset, int length)
        {
            _dataBytesStream.Seek(0, SeekOrigin.Begin);
            if (bytes != null)
                _dataBytesStream.Write(bytes, offset, length);

            _midiReader?.Dispose();
            _midiReader = new MidiReader(_dataBytesStream, new ReaderSettings());
            _midiReader.Position = 0;
        }

        private MidiEvent ReadEvent(byte statusByte)
        {
            if (BytesFormat == BytesFormat.Device && statusByte == EventStatusBytes.Global.NormalSysEx)
            {
                var data = ReadDeviceSysExBytes();
                return new NormalSysExEvent(data);
            }

            var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: false);
            if (BytesFormat == BytesFormat.File && statusByte == EventStatusBytes.Global.Meta)
                eventReader = MetaEventReader;

            return eventReader.Read(_midiReader, ReadingSettings, statusByte);
        }

        private byte[] ReadDeviceSysExBytes()
        {
            const int bufferSize = 100;

            // TODO: improve
            var result = new List<byte[]>();
            var position = _midiReader.Position;

            while (!_midiReader.EndReached)
            {
                var size = (int)Math.Min(bufferSize, _midiReader.Length - _midiReader.Position);
                var buffer = _midiReader.ReadBytes(size);
                var endIndex = Array.IndexOf(buffer, SysExEvent.EndOfEventByte);
                if (endIndex >= 0)
                {
                    var newBuffer = new byte[endIndex + 1];
                    Array.Copy(buffer, newBuffer, newBuffer.Length);
                    result.Add(newBuffer);
                    break;
                }

                result.Add(buffer);
            }

            var bytes = result.SelectMany(b => b).ToArray();

            _midiReader.Position = position + bytes.Length;
            return bytes;
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
                _midiReader?.Dispose();
            }

            _disposed = true;
        }

        #endregion
    }
}
