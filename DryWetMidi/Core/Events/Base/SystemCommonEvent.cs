namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a system common event.
    /// </summary>
    /// <remarks>
    /// MIDI system common messages are those MIDI messages that prompt all devices on
    /// the MIDI system to respond (are not specific to a MIDI channel), but do not
    /// require an immediate response from the receiving MIDI devices.
    /// </remarks>
    public abstract class SystemCommonEvent : MidiEvent
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemCommonEvent"/> with the specified event type.
        /// </summary>
        /// <param name="eventType">The type of event.</param>
        protected SystemCommonEvent(MidiEventType eventType)
            : base(eventType)
        {
        }

        #endregion
    }
}
