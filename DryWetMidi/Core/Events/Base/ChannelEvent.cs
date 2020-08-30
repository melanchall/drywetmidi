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
        public FourBitNumber Channel { get; set; }

        #endregion

        #region Methods

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
