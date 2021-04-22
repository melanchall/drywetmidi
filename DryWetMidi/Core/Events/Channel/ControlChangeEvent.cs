using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Control Change message.
    /// </summary>
    /// <remarks>
    /// This message is sent when a controller value changes. Controllers include devices
    /// such as pedals and levers.
    /// </remarks>
    public sealed class ControlChangeEvent : ChannelEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlChangeEvent"/>.
        /// </summary>
        public ControlChangeEvent()
            : base(MidiEventType.ControlChange)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ControlChangeEvent"/> with the specified
        /// controller number and controller value.
        /// </summary>
        /// <param name="controlNumber">Controller number.</param>
        /// <param name="controlValue">Controller value.</param>
        public ControlChangeEvent(SevenBitNumber controlNumber, SevenBitNumber controlValue)
            : this()
        {
            ControlNumber = controlNumber;
            ControlValue = controlValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets controller number.
        /// </summary>
        public SevenBitNumber ControlNumber
        {
            get { return (SevenBitNumber)_dataByte1; }
            set { _dataByte1 = value; }
        }

        /// <summary>
        /// Gets or sets controller value.
        /// </summary>
        public SevenBitNumber ControlValue
        {
            get { return (SevenBitNumber)_dataByte2; }
            set { _dataByte2 = value; }
        }

        #endregion

        #region Overrides

        internal override void Read(MidiReader reader, ReadingSettings settings, int size)
        {
            _dataByte1 = ReadDataByte(reader, settings);
            _dataByte2 = ReadDataByte(reader, settings);
        }

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            writer.WriteByte(_dataByte1);
            writer.WriteByte(_dataByte2);
        }

        internal override int GetSize(WritingSettings settings)
        {
            return 2;
        }

        /// <summary>
        /// Clones event by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the event.</returns>
        protected override MidiEvent CloneEvent()
        {
            return new ControlChangeEvent
            {
                _dataByte1 = _dataByte1,
                _dataByte2 = _dataByte2,
                Channel = Channel
            };
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Control Change [{Channel}] ({ControlNumber}, {ControlValue})";
        }

        #endregion
    }
}
