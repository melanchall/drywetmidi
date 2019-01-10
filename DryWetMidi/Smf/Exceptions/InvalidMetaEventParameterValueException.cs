using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid parameter
    /// of a meta event.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidMetaEventParameterValuePolicy"/>
    /// is set to <see cref="InvalidMetaEventParameterValuePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class InvalidMetaEventParameterValueException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMetaEventParameterValueException"/>.
        /// </summary>
        public InvalidMetaEventParameterValueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMetaEventParameterValueException"/> with
        /// the specified error message and invalid meta event's parameter value.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="value">The value of the meta event's parameter that caused this exception.</param>
        public InvalidMetaEventParameterValueException(string message, int value)
            : base(message)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMetaEventParameterValueException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private InvalidMetaEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetInt32(ValueSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value of the meta event's parameter that caused this exception.
        /// </summary>
        public int Value { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            ThrowIfArgument.IsNull(nameof(info), info);

            info.AddValue(ValueSerializationPropertyName, Value);

            base.GetObjectData(info, context);
        }

        #endregion
    }
}
