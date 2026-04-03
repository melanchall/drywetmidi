using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid parameter
    /// of a system common event.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidSystemCommonEventParameterValuePolicy"/>
    /// is set to <see cref="InvalidSystemCommonEventParameterValuePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class InvalidSystemCommonEventParameterValueException : MidiException
    {
        #region Constructors

        internal InvalidSystemCommonEventParameterValueException(MidiEventType eventType, string componentName, int componentValue)
            : base($"{componentValue} is invalid value for the {componentName} property of a system common event of {eventType} type.")
        {
            EventType = eventType;
            ComponentName = componentName;
            ComponentValue = componentValue;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of an event that caused this exception.
        /// </summary>
        public MidiEventType EventType { get; }

        /// <summary>
        /// Gets the name of MIDI Time Code event's component which value is invalid.
        /// </summary>
        public string ComponentName { get; }

        /// <summary>
        /// Gets the value of the system common event's parameter that caused this exception.
        /// </summary>
        public int ComponentValue { get; }

        #endregion
    }
}
