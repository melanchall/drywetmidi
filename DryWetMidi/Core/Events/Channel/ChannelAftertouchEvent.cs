using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a Channel Pressure (Aftertouch) message.
    /// </summary>
    /// <remarks>
    /// This message is most often sent by pressing down on the key after it "bottoms out".
    /// This message is different from polyphonic after-touch. Use this message to send the
    /// single greatest pressure value (of all the current depressed keys).
    /// </remarks>
    public sealed class ChannelAftertouchEvent : ChannelEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelAftertouchEvent"/>.
        /// </summary>
        public ChannelAftertouchEvent()
            : base(MidiEventType.ChannelAftertouch)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelAftertouchEvent"/> with the specified
        /// aftertouch (pressure) value.
        /// </summary>
        /// <param name="aftertouchValue">Aftertouch (pressure) value.</param>
        public ChannelAftertouchEvent(SevenBitNumber aftertouchValue)
            : this()
        {
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets aftertouch (pressure) value.
        /// </summary>
        public SevenBitNumber AftertouchValue
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
            return new ChannelAftertouchEvent
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
            return $"Channel Aftertouch [{Channel}] ({AftertouchValue})";
        }

        #endregion
    }
}
