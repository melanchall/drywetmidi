using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI file system exclusive event.
    /// </summary>
    /// <remarks>
    /// System exclusive events are used to specify a MIDI system exclusive message, either as one unit or in packets,
    /// or as an "escape" to specify any arbitrary bytes to be transmitted.
    /// </remarks>
    public abstract class SysExEvent : MidiEvent
    {
        #region Constants

        /// <summary>
        /// The value indicating the end of a system exclusive event.
        /// </summary>
        public const byte EndOfEventByte = 0xF7;

        #endregion

        #region Fields

        private readonly byte _statusByte;
        private byte[] _data;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SysExEvent"/> with the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        protected SysExEvent(MidiEventType eventType, byte statusByte)
            : base(eventType)
        {
            _statusByte = statusByte;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this system exclusive event is completed or not.
        /// </summary>
        public bool Completed { get; private set; }

        /// <summary>
        /// Gets or sets the event's data.
        /// </summary>
        public byte[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;

                if (value != null && value.Length > 0)
                {
                    Completed = value[value.Length - 1] == EndOfEventByte;
                    StartsWithStatusByte = value[0] == _statusByte;
                }
                else
                {
                    Completed = false;
                    StartsWithStatusByte = false;
                }
            }
        }

        internal bool StartsWithStatusByte { get; private set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        internal sealed override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            ThrowIfArgument.IsNegative(
                nameof(size),
                size,
                "Non-negative size have to be specified in order to read sys ex event.");

            var data = reader.ReadBytes(size);
            if (data.Length != size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("Not enough bytes in the stream to read the data of a sys ex event.", size, data.Length);

            Data = data;
        }

        /// <summary>
        /// Writes content of a MIDI event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        internal sealed override void Write(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null && data.Length > 0)
            {
                if (StartsWithStatusByte)
                    writer.WriteBytes(data, 1, data.Length - 1);
                else
                    writer.WriteBytes(data);
            }
        }

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        internal sealed override int GetSize(WritingSettings settings)
        {
            var result = Data?.Length ?? 0;
            if (StartsWithStatusByte)
                result--;

            return result;
        }

        public override string ToString()
        {
            const int margin = 3;

            var data = Data;
            var dataLength = data?.Length ?? 0;
            if (dataLength == 0)
                return "no data";

            var result = $"{dataLength} byte(s): ";
            if (dataLength <= margin * 2)
                return $"{result}{string.Join(" ", data)}";

            return $"{result}{data[0]} {data[1]} {data[2]} ... {data[dataLength - 3]} {data[dataLength - 2]} {data[dataLength - 1]}";
        }

        #endregion
    }
}
