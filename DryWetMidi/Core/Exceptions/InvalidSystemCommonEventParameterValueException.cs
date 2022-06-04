using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

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
    [Serializable]
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

        private InvalidSystemCommonEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EventType = (MidiEventType)info.GetValue(nameof(EventType), typeof(MidiEventType));
            ComponentName = info.GetString(nameof(ComponentName));
            ComponentValue = info.GetInt32(nameof(ComponentValue));
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

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(EventType), EventType);
            info.AddValue(nameof(ComponentName), ComponentName);
            info.AddValue(nameof(ComponentValue), ComponentValue);
        }

        #endregion
    }
}
