using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a MIDI file channel event.
    /// </summary>
    public abstract class ChannelEvent : MidiEvent
    {
        #region Fields

        internal byte _dataByte1;
        internal byte _dataByte2;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelEvent"/> with the specified parameters count.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        protected ChannelEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets channel for this event.
        /// </summary>
        /// <remarks>
        /// Channel is a zero-based number in DryWetMIDI, valid values are from <c>0</c> to <c>15</c>.
        /// Other libraries and software can use one-based channel numbers (i.e.from <c>1</c>
        /// to <c>16</c>) so be aware about that: channel <c>10</c> in such software will be <c>9</c>
        /// in DryWetMIDI.
        /// </remarks>
        public FourBitNumber Channel { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Reads channel event's data byte using the specified reader and settings.
        /// </summary>
        /// <param name="reader">Reader to read data byte with.</param>
        /// <param name="settings">Settings according to which a data byte should be read and processed.</param>
        /// <returns>A data byte read with <paramref name="reader"/>.</returns>
        protected byte ReadDataByte(MidiReader reader, ReadingSettings settings)
        {
            var value = reader.ReadByte();
            if (value > SevenBitNumber.MaxValue)
            {
                switch (settings.InvalidChannelEventParameterValuePolicy)
                {
                    case InvalidChannelEventParameterValuePolicy.Abort:
                        throw new InvalidChannelEventParameterValueException(EventType, value);
                    case InvalidChannelEventParameterValuePolicy.ReadValid:
                        value &= SevenBitNumber.MaxValue;
                        break;
                    case InvalidChannelEventParameterValuePolicy.SnapToLimits:
                        value = SevenBitNumber.MaxValue;
                        break;
                }
            }

            return value;
        }

        #endregion
    }
}
