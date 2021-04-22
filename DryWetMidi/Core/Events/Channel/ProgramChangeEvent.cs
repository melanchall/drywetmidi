using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Program Change message.
    /// </summary>
    /// <remarks>
    /// This message sent when the patch number changes.
    /// </remarks>
    public sealed class ProgramChangeEvent : ChannelEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramChangeEvent"/>.
        /// </summary>
        public ProgramChangeEvent()
            : base(MidiEventType.ProgramChange)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramChangeEvent"/> with the specified
        /// program number.
        /// </summary>
        /// <param name="programNumber">Program number.</param>
        public ProgramChangeEvent(SevenBitNumber programNumber)
            : this()
        {
            ProgramNumber = programNumber;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets program (patch) number.
        /// </summary>
        public SevenBitNumber ProgramNumber
        {
            get { return (SevenBitNumber)_dataByte1; }
            set { _dataByte1 = value; }
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            _dataByte1 = ReadDataByte(reader, settings);
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(_dataByte1);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 1;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new ProgramChangeEvent
            {
                _dataByte1 = _dataByte1,
                Channel = Channel
            };
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Program Change [{Channel}] ({ProgramNumber})";
        }

        #endregion
    }
}
