using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid parameter
    /// of a meta event.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidMetaEventParameterValuePolicy"/>
    /// is set to <see cref="InvalidMetaEventParameterValuePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    public sealed class InvalidMetaEventParameterValueException : MidiException
    {
        #region Constructors

        internal InvalidMetaEventParameterValueException(MidiEventType eventType, string propertyName, int value)
            : base($"{value} is invalid value for the {propertyName} property of a meta event of {eventType} type.")
        {
            EventType = eventType;
            PropertyName = propertyName;
            Value = value;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of event that caused this exception.
        /// </summary>
        public MidiEventType EventType { get; }

        /// <summary>
        /// Gets the name of event's property which value is invalid.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the value of the meta event's parameter that caused this exception.
        /// </summary>
        public int Value { get; }

        #endregion
    }
}
