using System;
using System.Runtime.Serialization;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    /// <summary>
    /// The exception that is thrown when the reading engine has encountered an invalid MIDI time
    /// code component (i.e. a value that doesn't belong to values of <see cref="MidiTimeCodeComponent"/>)
    /// during reading <see cref="MidiTimeCodeEvent"/>.
    /// </summary>
    [Serializable]
    public sealed class InvalidMidiTimeCodeComponentException : MidiException
    {
        #region Constants

        private const string ValueSerializationPropertyName = "Value";

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMidiTimeCodeComponentException"/>.
        /// </summary>
        public InvalidMidiTimeCodeComponentException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMidiTimeCodeComponentException"/> with
        /// the specified error message and invalid value that represents MIDI time code component.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="value">The value representing MIDI time code component that caused this exception.</param>
        public InvalidMidiTimeCodeComponentException(string message, byte value)
            : base(message)
        {
            Value = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidMidiTimeCodeComponentException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized
        /// object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        private InvalidMidiTimeCodeComponentException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Value = info.GetByte(ValueSerializationPropertyName);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the value representing MIDI time code component that caused this exception.
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
