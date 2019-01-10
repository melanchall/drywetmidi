using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid parameter
    /// of a channel event.
    /// </summary>
    /// <remarks>
    /// Note that this exception will be thrown only if <see cref="ReadingSettings.InvalidChannelEventParameterValuePolicy"/>
    /// is set to <see cref="InvalidChannelEventParameterValuePolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.
    /// </remarks>
    [Serializable]
    public sealed class InvalidChannelEventParameterValueException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidChannelEventParameterValueException"/>.
        /// </summary>
        public InvalidChannelEventParameterValueException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidChannelEventParameterValueException"/> with
        /// the specified error message and invalid channel event's parameter value.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="value">The value of the channel event's parameter that caused this exception.</param>
        public InvalidChannelEventParameterValueException(string message, byte value)
            : base(message)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidChannelEventParameterValueException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private InvalidChannelEventParameterValueException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetByte(ValueSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value of the channel event's parameter that caused this exception.
        /// </summary>
        public byte Value { get; }

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
