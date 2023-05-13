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

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SysExEvent"/> with the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        protected SysExEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether this system exclusive event is completed or not.
        /// </summary>
        public bool Completed => Data?.LastOrDefault() == EndOfEventByte;

        /// <summary>
        /// Gets or sets the event's data.
        /// </summary>
        public byte[] Data { get; set; }

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
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        internal sealed override int GetSize(WritingSettings settings)
        {
            return Data?.Length ?? 0;
        }

        #endregion
    }
}
