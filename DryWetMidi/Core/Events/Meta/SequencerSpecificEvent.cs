using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Sequencer Specific meta event.
    /// </summary>
    /// <remarks>
    /// The MIDI sequencer specific meta message carries information that is specific to a
    /// MIDI sequencer produced by a certain MIDI manufacturer.
    /// </remarks>
    public sealed class SequencerSpecificEvent : MetaEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencerSpecificEvent"/>.
        /// </summary>
        public SequencerSpecificEvent()
            : base(MidiEventType.SequencerSpecific)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SequencerSpecificEvent"/> with the
        /// specified data.
        /// </summary>
        /// <param name="data">Sequencer specific data.</param>
        public SequencerSpecificEvent(byte[] data)
            : this()
        {
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets sequencer specific data.
        /// </summary>
        public byte[] Data { get; set; }

        #endregion

        #region Overrides

        /// <summary>
        /// Reads content of a MIDI meta event.
        /// </summary>
        /// <param name="reader">Reader to read the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be read.</param>
        /// <param name="size">Size of the event's content.</param>
        /// <exception cref="ArgumentOutOfRangeException">Sequencer specific event cannot be read since the size is
        /// negative number.</exception>
        protected override void ReadContent(MidiReader reader, ReadingSettings settings, int size)
        {
            ThrowIfArgument.IsNegative(
                nameof(size),
                size,
                "Sequencer specific event cannot be read since the size is negative number.");

            if (size == 0)
            {
                switch (settings.ZeroLengthDataPolicy)
                {
                    case ZeroLengthDataPolicy.ReadAsEmptyObject:
                        Data = new byte[0];
                        break;
                    case ZeroLengthDataPolicy.ReadAsNull:
                        Data = null;
                        break;
                }

                return;
            }

            var data = reader.ReadBytes(size);
            if (data.Length != size && settings.NotEnoughBytesPolicy == NotEnoughBytesPolicy.Abort)
                throw new NotEnoughBytesException("Not enough bytes in the stream to read the data of a sequencer specific event.", size, data.Length);

            Data = data;
        }

        /// <summary>
        /// Writes content of a MIDI meta event.
        /// </summary>
        /// <param name="writer">Writer to write the content with.</param>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <summary>
        /// Gets the size of the content of a MIDI meta event.
        /// </summary>
        /// <param name="settings">Settings according to which the event's content must be written.</param>
        /// <returns>Size of the event's content.</returns>
        protected override int GetContentSize(WritingSettings settings)
        {
            return Data?.Length ?? 0;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new SequencerSpecificEvent(Data?.Clone() as byte[]);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return "Sequencer Specific";
        }

        #endregion
    }
}
