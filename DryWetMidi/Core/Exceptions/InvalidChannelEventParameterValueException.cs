using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid parameter
    /// of a channel event.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidChannelEventParameterValuePolicy"/>
    /// is set to <see cref="InvalidChannelEventParameterValuePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class InvalidChannelEventParameterValueException : MidiException
    {
        #region Constructors

        internal InvalidChannelEventParameterValueException(MidiEventType eventType, byte value)
            : base($"{value} is invalid value for parameter of channel event of {eventType} type.")
        {
            EventType = eventType;
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of channel event that caused this exception.
        /// </summary>
        public MidiEventType EventType { get; }

        /// <summary>
        /// Gets the value of the channel event's parameter that caused this exception.
        /// </summary>
        public byte Value { get; }

        #endregion
    }
}
