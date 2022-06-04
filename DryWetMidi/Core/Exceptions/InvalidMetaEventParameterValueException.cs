using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

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
    [Serializable]
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

        private InvalidMetaEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            EventType = (MidiEventType)info.GetValue(nameof(EventType), typeof(MidiEventType));
            PropertyName = info.GetString(nameof(PropertyName));
            Value = info.GetInt32(nameof(Value));
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
            info.AddValue(nameof(PropertyName), PropertyName);
            info.AddValue(nameof(Value), Value);
        }

        #endregion
    }
}
