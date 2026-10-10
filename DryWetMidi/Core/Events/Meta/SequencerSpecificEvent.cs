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
        public SequencerSpecificEvent(byte[]? data)
            : this()
        {
            Data = data;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets sequencer specific data.
        /// </summary>
        public byte[]? Data { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        protected override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        /// <inheritdoc/>
        protected override int GetContentSize(WritingSettings settings)
        {
            return Data?.Length ?? 0;
        }

        /// <inheritdoc/>
        protected override MidiEvent CloneEvent()
        {
            return new SequencerSpecificEvent(Data?.Clone() as byte[]);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Sequencer Specific";
        }

        #endregion
    }
}
